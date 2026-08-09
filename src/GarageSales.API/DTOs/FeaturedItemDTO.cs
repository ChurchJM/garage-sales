using System.Text.Json.Serialization;

public record FeaturedItemDTO(
    [property: JsonRequired] string Category,
    [property: JsonRequired] string Description
);