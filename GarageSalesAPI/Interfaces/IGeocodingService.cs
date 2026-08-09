public interface IGeocodingService
{
    Task<GeoapifyResponse?> GeocodeAddressAsync(
        string street, string zip);
}