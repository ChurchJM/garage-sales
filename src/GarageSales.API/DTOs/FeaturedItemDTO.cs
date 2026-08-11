using System.Text.Json.Serialization;

public record FeaturedItemDTO(
    [property: JsonRequired] string Category,
    [property: JsonRequired] string Name,
    string? Description,
    [property: JsonRequired] double Price
);