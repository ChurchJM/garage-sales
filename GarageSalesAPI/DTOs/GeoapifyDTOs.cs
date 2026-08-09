// Geoapify GeoJSON Response Deserialization DTOs
using System.Text.Json.Serialization;

public record GeoapifyResponse(
    [property: JsonPropertyName("features")] List<Feature>? Features);

public record Feature(
    [property: JsonPropertyName("properties")] FeatureProperties? Properties);

public record FeatureProperties(
    [property: JsonPropertyName("lat")] double Lat,
    [property: JsonPropertyName("lon")] double Lon,
    [property: JsonPropertyName("address_line1")] string? Street,
    [property: JsonPropertyName("postcode")] string? Zip
);