using System.Text.Json.Serialization;

public record GarageSaleCreateDTO(
    [property: JsonRequired] string SaleType,
    [property: JsonRequired] string Owner,
    [property: JsonRequired] string Street,
    [property: JsonRequired] string Zip,
    string? Description,
    [property: JsonRequired] List<GarageSaleScheduleDTO> Schedules,
    List<FeaturedItemDTO> FeaturedItems
);