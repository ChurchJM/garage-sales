using System.Web;
using GarageSales.API.Entities;
using Microsoft.EntityFrameworkCore;

public class GeoapifyGeocodingService : IGeocodingService
{
    private readonly HttpClient _httpClient;
    private readonly GarageSalesDbContext _db;

    public GeoapifyGeocodingService(HttpClient httpClient, GarageSalesDbContext db)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "GarageSalesAPI/1.0");
        _db = db;
    }

    public async Task<GeoapifyResponse?> GeocodeAddressAsync(
        string street, 
        string zip)
    {
        var apiKey = await _db.Secrets
            .Where(s => s.Key == "Geoapify API Key")
            .Select(s => s.Value)
            .FirstOrDefaultAsync();

        var query = $"{street}, Palm Coast, FL {zip}";
        query = HttpUtility.UrlEncode(query);

        var requestUri = $"https://api.geoapify.com/v1/geocode/search?text={query}&apiKey={apiKey}";

        var response = await _httpClient.GetAsync(requestUri);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var geoapifyResponse = await response.Content.ReadFromJsonAsync<GeoapifyResponse>();
        
        var topFeature = geoapifyResponse?.Features?.FirstOrDefault(); // Best match
        if (topFeature?.Properties is null)
        {
            return null;
        }

        return geoapifyResponse;
    }    
}