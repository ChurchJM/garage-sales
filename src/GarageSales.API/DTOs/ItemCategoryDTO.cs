using System.Text.Json.Serialization;

public record ItemCategoryDTO(
    [property: JsonRequired] int Id,
    [property: JsonRequired] string Name,
    [property: JsonRequired] string Description
);