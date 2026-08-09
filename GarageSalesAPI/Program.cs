using System.Net;
using System.Net.Sockets;
using GarageSalesAPI.Entities;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using Point = NetTopologySuite.Geometries.Point;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<GarageSalesDbContext>(options =>
{
    // options.EnableSensitiveDataLogging();
    // options.EnableDetailedErrors();
});

// The complex configuration here is necessary because my home internet is Starlink, which uses IPV6.
// Had to force IPV4 to avoid hanging/timeouts. You might not need this in your environment.
builder.Services.AddHttpClient<IGeocodingService, GeoapifyGeocodingService>(client => client.Timeout = TimeSpan.FromSeconds(20))
    .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
{
    UseProxy = false, // Prevents Windows proxy auto-discovery stalls
    ConnectCallback = async (context, cancellationToken) =>
    {
        // 1. Fetch DNS addresses for the target host
        var addresses = await Dns.GetHostAddressesAsync(context.DnsEndPoint.Host, cancellationToken);
        
        // 2. Filter specifically for an IPv4 address (AddressFamily.InterNetwork)
        var ipv4 = addresses.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork)
            ?? throw new InvalidOperationException($"No IPv4 address found for {context.DnsEndPoint.Host}");

        // 3. Open the TCP socket directly using IPv4
        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        await socket.ConnectAsync(ipv4, context.DnsEndPoint.Port, cancellationToken);
        
        return new NetworkStream(socket, ownsSocket: true);
    }
});

builder.Services.AddScoped<IEmailNotificationService, MailpitEmailNotificationService>();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//     app.MapOpenApi();
// }

app.UseHttpsRedirection();

app.MapGet("/", () => "I'm alive!");

// Confirm the listing after user reviews the draft. Send appropriate email notifications.
app.MapPost("/api/garagesales/{id:int}/confirm", async (int id, GarageSalesDbContext db, IEmailNotificationService emailSvc) =>
{
    var draftSale = await db.GarageSales.FirstOrDefaultAsync(gs => gs.Id == id);
    if(draftSale is null)
        return Results.BadRequest($"No garage sale exists with id {id}");
    if(!draftSale.Draft)
        return Results.BadRequest($"Garage sale with id {id} has already been confirmed.");

    draftSale.Draft = false;
    await db.SaveChangesAsync();

    var saleLocation = await db.GarageSales.Where(gs => gs.Id == id)
        .Select(gs => gs.Address.Location).FirstOrDefaultAsync();

    // Address could not be geocoded during garage sale creation.
    if(saleLocation is null)
        return Results.Ok();

    var saleDate = DateOnly.FromDateTime(draftSale.GarageSaleSchedules.Min(s => s.From));
    var saleAddress = $"{draftSale.Address.Street}, {draftSale.Address.City}, {draftSale.Address.State}, {draftSale.Address.Zip}";

    var notifications = await db.Notifications.MatchingSaleLocation(saleLocation)
        .Select(n => new
        {
            Email = n.User.Email,
            UserName = n.User.UserName,
            SaleDistance = n.User.Address.Location.Distance(saleLocation) / SpatialQueryExtensions.MetersPerMile
        }).Distinct().ToListAsync();

    foreach(var notification in notifications)
    {
        await emailSvc.SendGarageSaleNotificationAsync(
            notification.Email,
            notification.UserName,
            saleAddress,
            notification.SaleDistance,
            saleDate);
    }

    return Results.Ok();
});

// Garage sale creation endpoint.
app.MapPost("/api/garagesales", async (GarageSaleCreateDTO dto, GarageSalesDbContext db, GeoapifyGeocodingService geo) =>
{
    // Check 2 sale limit per resident per year.
    var userSalecount = await db.GarageSales.ThisYearByUserName(dto.Owner).CountAsync();
    if(userSalecount >= 2)
        return Results.BadRequest($"User {dto.Owner} has already held two garage sales this year.");

    // Ensure all schedules fall within two consecutive days.
    var (valid, error) = dto.Schedules.ValidateScheduleSpan();
    if(!valid)
        return Results.BadRequest(error);

    // Perform Id lookups
    var saleType = await db.GarageSaleTypes.FirstOrDefaultAsync(gst => gst.Name == dto.SaleType);
    if (saleType is null)
        return Results.BadRequest($"Unknown sale type: '{dto.SaleType}'");
    
    var owner = await db.Users.FirstOrDefaultAsync(u => u.UserName == dto.Owner);
    if (owner is null)
        return Results.BadRequest($"Unknown user: '{dto.Owner}'");    

    var categoryNames = (dto.FeaturedItems ?? [])
        .Select(i => i.Category)
        .Distinct();

    var categories = await db.ItemCategories
        .Where(c => categoryNames.Contains(c.Name))
        .ToDictionaryAsync(c => c.Name, c => c.Id);
    
    var missingCategories = categoryNames.Except(categories.Keys);
    if (missingCategories.Any())
    {
        return Results.BadRequest($"Unknown category/categories: {string.Join(", ", missingCategories)}");
    }

    // Check if we've already stored and geocoded an address, saving an API call.
    var address = await db.Addresses.FirstOrDefaultAsync(a =>
            string.Equals(a.Street, dto.Street, StringComparison.OrdinalIgnoreCase)
            && string.Equals(a.Zip, dto.Zip, StringComparison.OrdinalIgnoreCase));

    if(address is null)
    {
        address = new(){Street=dto.Street, City="Palm Coast", State="FL", Zip=dto.Zip};
        var geocoded = await geo.GeocodeAddressAsync(dto.Street, dto.Zip);
        var feature = geocoded?.Features?.FirstOrDefault();
        if (feature is not null)
        {
            var lat = feature.Properties.Lat;
            var lon = feature.Properties.Lon;
            address.Lat = lat;
            address.Lon = lon;
            address.Location = new Point(lon, lat) {SRID = 4326}; //SRID = 4326 indicates WGS 84 coordinate reference system (used by Geoapify).
        }
        db.Addresses.Add(address);
    }

    var garageSale = new GarageSale()
    {
        SaleTypeId = saleType.Id,
        OwnerId = owner.Id,
        Address = address,
        Description = dto.Description,
        Draft = true,
        GarageSaleSchedules = dto.Schedules.Select(s => new GarageSaleSchedule
        {
           From = s.From,
           To = s.To
        }).ToList(),
        FeaturedItems = dto.FeaturedItems?.Select(fi => new FeaturedItem
        {
            CategoryId = categories.GetValueOrDefault(fi.Category),
            Description = fi.Description
        }).ToList() ?? new List<FeaturedItem>()
    };
    
    db.GarageSales.Add(garageSale);
    await db.SaveChangesAsync();

    return Results.Created($"/api/garagesales/{garageSale.Id}", new { id = garageSale.Id });
});

// Update Garage Sale
app.MapPut("/api/garagesales/{id:int}", async (int id, GarageSaleUpdateDTO dto, GarageSalesDbContext db, GeoapifyGeocodingService geo) =>
{
    var existingSale = await db.GarageSales.FirstOrDefaultAsync(gs => gs.Id == id);
    if(existingSale is null)
        return Results.NotFound($"Garage sale with id {id} not found.");
    
    // No need to check number of user's sales in edit endpoint.

    // Ensure all schedules fall within two consecutive days.
    var (valid, error) = dto.Schedules.ValidateScheduleSpan();
    if(!valid)
        return Results.BadRequest(error);

    // Perform Id lookups
    var saleType = await db.GarageSaleTypes.FirstOrDefaultAsync(gst => gst.Name == dto.SaleType);
    if (saleType is null)
        return Results.BadRequest($"Unknown sale type: '{dto.SaleType}'");
    
    var owner = await db.Users.FirstOrDefaultAsync(u => u.UserName == dto.Owner);
    if (owner is null)
        return Results.BadRequest($"Unknown user: '{dto.Owner}'");    

    var categoryNames = (dto.FeaturedItems ?? [])
        .Select(i => i.Category)
        .Distinct();

    var categories = await db.ItemCategories
        .Where(c => categoryNames.Contains(c.Name))
        .ToDictionaryAsync(c => c.Name, c => c.Id);
    
    // Need to geocode new address?
    bool addressChanged = existingSale.Address.Street != dto.Street ||
                          existingSale.Address.Zip != dto.Zip;
    if(addressChanged)
    {
        Address address = new(){Street=dto.Street, City="Palm Coast", State="FL", Zip=dto.Zip};
        var geocoded = await geo.GeocodeAddressAsync(dto.Street, dto.Zip);
        var feature = geocoded?.Features?.FirstOrDefault();
        if (feature is not null)
        {
            var lat = feature.Properties.Lat;
            var lon = feature.Properties.Lon;
            address.Lat = lat;
            address.Lon = lon;
            address.Location = new Point(lon, lat) {SRID = 4326}; //SRID = 4326 indicates WGS 84 coordinate reference system (used by Geoapify).
        }
        db.Addresses.Add(address);
        existingSale.Address = address;
    }

    existingSale.SaleTypeId = saleType.Id;
    existingSale.OwnerId = owner.Id;
    existingSale.Description = dto.Description;

    // Since existing garage sales already have tracked collections for Schedules and FeaturedItems,
    // mutate them instead of creating fresh lists.
    existingSale.GarageSaleSchedules.Clear();
    foreach(var schedule in dto.Schedules)
    {
        existingSale.GarageSaleSchedules.Add(new GarageSaleSchedule
        {
            From = schedule.From,
            To = schedule.To
        });
    }

    existingSale.FeaturedItems.Clear();
    if(dto.FeaturedItems is not null)
    {
        foreach(var fi in dto.FeaturedItems)
        {
            existingSale.FeaturedItems.Add(new FeaturedItem
            {
                CategoryId = categories.GetValueOrDefault(fi.Category),
                Description = fi.Description
            });
        }
    }

    await db.SaveChangesAsync();

    return Results.Ok();        
});

app.MapDelete("/api/garagesales/{id:int}", async (int id, GarageSalesDbContext db) =>
{
    var existingSale = await db.GarageSales.FirstOrDefaultAsync(gs => gs.Id == id);
    if(existingSale is null)
        return Results.NotFound($"Garage sale with id {id} not found.");
    
    db.GarageSales.Remove(existingSale);
    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapGet("/api/garagesales/{id:int}", async (int id, GarageSalesDbContext db) =>
{
    var existingSale = await db.GarageSales.AsNoTracking().FirstOrDefaultAsync(gs => gs.Id == id);
    if(existingSale is null)
        return Results.NotFound($"Garage sale with id {id} not found.");

    var scheduleDTOs = existingSale.GarageSaleSchedules.Select(s => new GarageSaleScheduleDTO(s.From, s.To)).ToList();

    var itemDTOs = existingSale.FeaturedItems.Select(fi => new FeaturedItemDTO(fi.Category.Name, fi.Description)).ToList();

    var summaryDTO = new GarageSaleSummaryDTO
    (
        existingSale.SaleType.Name,
        existingSale.Address.Street,
        existingSale.Address.Zip,
        existingSale.Description,
        scheduleDTOs,
        itemDTOs
    );

    return Results.Ok(summaryDTO);
});

app.MapGet("/api/garagesales", async ([AsParameters] GarageSaleQueryParamsDTO queryParams, GarageSalesDbContext db, IGeocodingService geo) =>
{
    var query = db.GarageSales.AsNoTracking();

    if(!string.IsNullOrWhiteSpace(queryParams.Keyword))
    {
        query = query.Where(gs => !string.IsNullOrWhiteSpace(gs.Description) 
        && gs.Description.Contains(queryParams.Keyword, StringComparison.OrdinalIgnoreCase));
    }

    if(!string.IsNullOrWhiteSpace(queryParams.FeaturedItemCategory))
    {
        query = query.Where(gs => gs.FeaturedItems.Any(fi => fi.Category.Name == queryParams.FeaturedItemCategory));
    }

    // Query locates garage sales BEGINNING between AfterDate and BeforeDate.
    if(queryParams.AfterDate.HasValue || queryParams.BeforeDate.HasValue)
    {
        var afterDate = queryParams.AfterDate?.Date;
        var beforeDate = queryParams.BeforeDate?.Date.AddDays(1).AddTicks(-1); // E.g., 11:59:59 PM on equested BeforeDate

        query = query.Where(gs => gs.GarageSaleSchedules.Any(s =>
            (!afterDate.HasValue || s.From >= afterDate.Value) &&
            (!beforeDate.HasValue || s.From <= beforeDate.Value)
        ));
    }

    // Is it a good idea to submit the query before this point to reduce the amount of distance calculations,
    // or is these optimizations already made by default?
    if(!string.IsNullOrWhiteSpace(queryParams.FromStreet) && !string.IsNullOrWhiteSpace(queryParams.FromZip)
        && queryParams.RadiusMiles.HasValue)
    {
        var address = await db.Addresses.FirstOrDefaultAsync(a =>
            string.Equals(a.Street, queryParams.FromStreet, StringComparison.OrdinalIgnoreCase)
            && string.Equals(a.Zip, queryParams.FromZip, StringComparison.OrdinalIgnoreCase));

        Geometry pointOfOrigin = null;
        if(address is not null && address.Location is not null)
        {
            pointOfOrigin = address.Location;
        }
        else
        {
            var geocoded = await geo.GeocodeAddressAsync(queryParams.FromStreet, queryParams.FromZip);
            var feature = geocoded?.Features?.FirstOrDefault();
            if (feature is not null)
            {
                var lat = feature.Properties.Lat;
                var lon = feature.Properties.Lon;
                pointOfOrigin = new Point(lon, lat) {SRID = 4326}; //SRID = 4326 indicates WGS 84 coordinate reference system (used by Geoapify).
            }
        }

        if(pointOfOrigin is not null)
            query = query.WithinRadiusOf(pointOfOrigin, queryParams.RadiusMiles.Value);
    }

    var matchingSales = await query
        .Select(gs => new GarageSaleSummaryDTO(
            gs.SaleType.Name,
            gs.Address.Street,
            gs.Address.Zip,
            gs.Description,
            gs.GarageSaleSchedules.Select(s => new GarageSaleScheduleDTO(s.From, s.To)).ToList(),
            gs.FeaturedItems.Select(fi => new FeaturedItemDTO(fi.Category.Name, fi.Description)).ToList()
        ))
        .ToListAsync();
    
    return Results.Ok(matchingSales);
});

app.MapPost("/api/users", async (CreateUserDTO dto, GarageSalesDbContext db, IGeocodingService geo) =>
{
    var existingUser = await db.Users.FirstOrDefaultAsync(u => u.UserName == dto.UserName);
    if(existingUser is not null)
        return Results.Conflict($"User with user name {dto.UserName} already exists.");

    var address = await db.Addresses.FirstOrDefaultAsync(a =>
            string.Equals(a.Street, dto.Street, StringComparison.OrdinalIgnoreCase)
            && string.Equals(a.Zip, dto.Zip, StringComparison.OrdinalIgnoreCase));

    if(address is null)
    {
        address = new(){Street=dto.Street, City="Palm Coast", State="FL", Zip=dto.Zip};
        var geocoded = await geo.GeocodeAddressAsync(dto.Street, dto.Zip);
        var feature = geocoded?.Features?.FirstOrDefault();
        if (feature is not null)
        {
            var lat = feature.Properties.Lat;
            var lon = feature.Properties.Lon;
            address.Lat = lat;
            address.Lon = lon;
            address.Location = new Point(lon, lat) {SRID = 4326}; //SRID = 4326 indicates WGS 84 coordinate reference system (used by Geoapify).
        }
    }

    var user = new User
    {
        UserName = dto.UserName,
        Email = dto.Email,
        Password = dto.Password,
        Address = address,
        IsAdmin = false
    };

    db.Users.Add(user);
    await db.SaveChangesAsync();

    return Results.Created($"/api/users/{user.Id}", new { id = user.Id });
});

app.Run();