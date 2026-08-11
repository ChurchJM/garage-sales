using System.Text.Json.Serialization;

public record GarageSaleTypeDTO(
    [property: JsonRequired] int Id,
    [property: JsonRequired] string Name,
    [property: JsonRequired] string Description
);