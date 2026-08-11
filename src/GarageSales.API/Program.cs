using System.Net;
using System.Net.Sockets;
using System.Security.Claims;
using GarageSales.API.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using Scalar.AspNetCore;
using Point = NetTopologySuite.Geometries.Point;

var builder = WebApplication.CreateBuilder(args);

string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<GarageSalesDbContext>(options =>
{
    options.UseSqlServer(connectionString);
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

var keysFolder = Path.Combine(builder.Environment.ContentRootPath, "..", "shared-keys");

Console.WriteLine($"[DataProtection API] Using Key Store Path: {keysFolder}");

// Necessary for web project to decrypt cookie created by API login.
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysFolder))
    .SetApplicationName("GarageSalesApp");

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "GarageSaleAuth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Events.OnRedirectToLogin = context =>
        {
            // Return 401 Unauthorized for API requests instead of redirecting to a login page
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization();

// Allow GarageSales.Web project to make JavaScript calls to API.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebFrontend", policy =>
    {
        policy.WithOrigins(
            "https://localhost:7120",
            "http://localhost:5042"
        ).AllowAnyHeader().AllowAnyMethod().AllowCredentials();;
    });
});

builder.Services.AddOpenApi();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Available at http://localhost:5166/scalar/v1
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Garage Sales API");
    }); 
}

app.UseCors("AllowWebFrontend");

app.UseHttpsRedirection();

#region GET Mappings

app.MapGet("/", () => "I'm alive!");

app.MapGet("/api/garagesales", async ([AsParameters] GarageSaleQueryParamsDTO queryParams, GarageSalesDbContext db, IGeocodingService geo) =>
{
    var query = db.GarageSales.AsNoTracking();

    if(!string.IsNullOrWhiteSpace(queryParams.Keyword))
    {
        query = query.Where(gs => !string.IsNullOrWhiteSpace(gs.Description) 
        && gs.Description.Contains(queryParams.Keyword));
    }

    if(queryParams.SaleTypeId.HasValue)
    {
        query = query.Where(gs => gs.SaleTypeId == queryParams.SaleTypeId.Value);
    }
    else if(!string.IsNullOrWhiteSpace(queryParams.GarageSaleType))
    {
        query = query.Where(gs => gs.SaleType.Name == queryParams.GarageSaleType);
    }

    if(queryParams.ItemCategoryId.HasValue)
    {
        query = query.Where(gs => gs.FeaturedItems.Any(fi => fi.CategoryId == queryParams.ItemCategoryId.Value));
    }
    else if(!string.IsNullOrWhiteSpace(queryParams.FeaturedItemCategory))
    {
        query = query.Where(gs => gs.FeaturedItems.Any(fi => fi.Category.Name == queryParams.FeaturedItemCategory));
    }

    // Query locates garage sales BEGINNING between AfterDate and BeforeDate.
    if(queryParams.AfterDate.HasValue || queryParams.BeforeDate.HasValue)
    {
        var afterDate = queryParams.AfterDate?.Date;
        var beforeDate = queryParams.BeforeDate?.Date.AddDays(1).AddTicks(-1); // E.g., 11:59:59 PM on requested BeforeDate

        query = query.Where(gs => gs.GarageSaleSchedules.Any(s =>
            (!afterDate.HasValue || s.From >= afterDate.Value) &&
            (!beforeDate.HasValue || s.From <= beforeDate.Value)
        ));
    }

    Geometry pointOfOrigin = null;
    if(!string.IsNullOrWhiteSpace(queryParams.FromStreet) && !string.IsNullOrWhiteSpace(queryParams.FromZip)
        && queryParams.RadiusMiles.HasValue)
    {
        var address = await db.Addresses.FirstOrDefaultAsync(a =>
            a.Street.ToUpper() == queryParams.FromStreet.ToUpper()
            && a.Zip == queryParams.FromZip);

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
            gs.Id,
            null,
            gs.SaleType.Name,
            gs.Address.Street,
            gs.Address.Zip,
            gs.Description,
            pointOfOrigin != null ? Math.Round(gs.Address.Location.Distance(pointOfOrigin) / SpatialQueryExtensions.MetersPerMile, 2) : null,
            gs.GarageSaleSchedules.Select(s => new GarageSaleScheduleDTO(s.From, s.To)).ToList(),
            gs.FeaturedItems.Select(fi => new FeaturedItemDTO(fi.Category.Name, fi.Name, fi.Description, fi.Price)).ToList()
        ))
        .ToListAsync();
    
    return Results.Ok(matchingSales);
});

app.MapGet("/api/garagesales/{id:int}", async (int id, GarageSalesDbContext db) =>
{
    var existingSale = await db.GarageSales.AsNoTracking().FirstOrDefaultAsync(gs => gs.Id == id);
    if(existingSale is null)
        return Results.NotFound($"Garage sale with id {id} not found.");

    var scheduleDTOs = existingSale.GarageSaleSchedules.Select(s => new GarageSaleScheduleDTO(s.From, s.To)).ToList();

    var itemDTOs = existingSale.FeaturedItems.Select(fi => new FeaturedItemDTO(fi.Category.Name, fi.Name, fi.Description, fi.Price)).ToList();

    var summaryDTO = new GarageSaleSummaryDTO
    (
        existingSale.Id,
        null,
        existingSale.SaleType.Name,
        existingSale.Address.Street,
        existingSale.Address.Zip,
        existingSale.Description,
        null,
        scheduleDTOs,
        itemDTOs
    );

    return Results.Ok(summaryDTO);
});

app.MapGet("/api/lookups", async(GarageSalesDbContext db) =>
{
    var saleTypes = await db.GarageSaleTypes.Select(gst => new GarageSaleTypeDTO
    (
       gst.Id, gst.Name, gst.Description 
    )).ToListAsync();

    var itemCategories = await db.ItemCategories.Select(ic => new ItemCategoryDTO
    (
        ic.Id, ic.Name, ic.Description
    )).ToListAsync();

    var lookups = new LookupsDTO(saleTypes, itemCategories);

    return Results.Ok(lookups);
});

app.MapGet("/api/auth/me", (ClaimsPrincipal user) =>
{
    if(user.Identity?.IsAuthenticated != true)
    {
        return Results.Unauthorized();
    }

    return Results.Ok(new
    {
        Id = user.FindFirstValue(ClaimTypes.NameIdentifier),
        Email = user.FindFirstValue(ClaimTypes.Email),
        UserName = user.FindFirstValue(ClaimTypes.Name),
        IsAdmin = user.IsInRole("Admin") 
    });
});

app.MapGet("/api/mygaragesales", async (ClaimsPrincipal user, GarageSalesDbContext db) =>
{
    var userIdString = user.FindFirstValue(ClaimTypes.NameIdentifier);
    if (!int.TryParse(userIdString, out int currentUserId))
    {
        return Results.Unauthorized();
    }

    List<GarageSaleSummaryDTO> results = null;
    var query = db.GarageSales.AsNoTracking();
    if(!user.IsInRole("Admin"))
    {
        query = query.Where(gs => gs.OwnerId == currentUserId);
    }
    
    results = await query.Select(gs => new GarageSaleSummaryDTO(
        gs.Id,
        gs.Owner.UserName,
        gs.SaleType.Name,
        gs.Address.Street,
        gs.Address.Zip,
        gs.Description,
        null,
        gs.GarageSaleSchedules.Select(s => new GarageSaleScheduleDTO(s.From, s.To)).ToList(),
        gs.FeaturedItems.Select(fi => new FeaturedItemDTO(fi.Category.Name, fi.Name, fi.Description, fi.Price)).ToList()
    ))
    .ToListAsync();
    
    return Results.Ok(results);
});

#endregion

#region POST Mappings

// Garage sale creation endpoint.
app.MapPost("/api/garagesales", async (GarageSaleCreateDTO dto, GarageSalesDbContext db, IGeocodingService geo) =>
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
            a.Street.ToUpper() == dto.Street.ToUpper()
            && a.Zip == dto.Zip);

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
            Name = fi.Name,
            Description = fi.Description,
            Price = fi.Price
        }).ToList() ?? new List<FeaturedItem>()
    };
    
    db.GarageSales.Add(garageSale);
    await db.SaveChangesAsync();

    return Results.Created($"/api/garagesales/{garageSale.Id}", new { id = garageSale.Id });
});

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

    try
    {
        foreach(var notification in notifications)
        {
            await emailSvc.SendGarageSaleNotificationAsync(
                notification.Email,
                notification.UserName,
                saleAddress,
                notification.SaleDistance,
                saleDate);
        }
    }
    catch(Exception ex)
    {
        Console.WriteLine("Please ensure Mailpit is running.");
    }

    return Results.Ok();
});

app.MapPost("/api/users", async (CreateUserDTO dto, GarageSalesDbContext db, IGeocodingService geo) =>
{
    var existingUser = await db.Users.FirstOrDefaultAsync(u => u.UserName == dto.UserName);
    if(existingUser is not null)
        return Results.Conflict($"User with user name {dto.UserName} already exists.");

    var address = await db.Addresses.FirstOrDefaultAsync(a =>
            a.Street.ToUpper() == dto.Street.ToUpper()
            && a.Zip == dto.Zip);

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

app.MapPost("/api/auth/login", async (LoginDTO dto, GarageSalesDbContext db, HttpContext httpContext) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.UserName == dto.UserName);
    if(user is null || user.Password != dto.Password)
    {
        return Results.Unauthorized();
    }

    var claims = new List<Claim>()
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.UserName),
        new Claim(ClaimTypes.Email, user.Email)
    };

    if(user.IsAdmin)
    {
        claims.Add(new Claim(ClaimTypes.Role, "Admin"));
    }

    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

    await httpContext.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        claimsPrincipal,
        new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddDays(1)}
    );

    return Results.Ok(new {user.Id, user.UserName, user.Email, user.IsAdmin});
});

app.MapPost("/api/auth/logout", async (HttpContext httpContext) =>
{
    await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Ok();
});

#endregion

#region PUT Mappings

// Update Garage Sale
app.MapPut("/api/garagesales/{id:int}", async (int id, GarageSaleUpdateDTO dto, GarageSalesDbContext db, 
IGeocodingService geo, ClaimsPrincipal user) =>
{
    var userIdString = user.FindFirstValue(ClaimTypes.NameIdentifier);
    if (!int.TryParse(userIdString, out int currentUserId))
    {
        return Results.Unauthorized();
    }

    var existingSale = await db.GarageSales.FirstOrDefaultAsync(gs => gs.Id == id);
    if(existingSale is null)
        return Results.NotFound($"Garage sale with id {id} not found.");
    
    // Users may only edit their own sales, but admins can edit everything.
    if(existingSale.OwnerId != currentUserId && !user.IsInRole("Admin"))
    {
        return Results.Forbid();
    }

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
                Name = fi.Name,
                Description = fi.Description,
                Price = fi.Price
            });
        }
    }

    await db.SaveChangesAsync();

    return Results.Ok();        
});

#endregion

#region DELETE Mappings

app.MapDelete("/api/garagesales/{id:int}", async (int id, GarageSalesDbContext db, ClaimsPrincipal user) =>
{
    var userIdString = user.FindFirstValue(ClaimTypes.NameIdentifier);
    if (!int.TryParse(userIdString, out int currentUserId))
    {
        return Results.Unauthorized();
    }

    var existingSale = await db.GarageSales.FirstOrDefaultAsync(gs => gs.Id == id);
    if(existingSale is null)
        return Results.NotFound($"Garage sale with id {id} not found.");
    
    // Users may only edit their own sales, but admins can edit everything.
    if(existingSale.OwnerId != currentUserId && !user.IsInRole("Admin"))
    {
        return Results.Forbid();
    }
    
    db.GarageSales.Remove(existingSale);
    await db.SaveChangesAsync();

    return Results.NoContent();
});

#endregion

app.Run();