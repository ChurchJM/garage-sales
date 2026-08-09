using System.Text.Json.Serialization;

// This DTO is almost identical to GarageSaleCreateDTO, except that Owner is omitted for privacy.
public record GarageSaleSummaryDTO(
    [property: JsonRequired] string SaleType,
    [property: JsonRequired] string Street,
    [property: JsonRequired] string Zip,
    string? Description,
    [property: JsonRequired] List<GarageSaleScheduleDTO> Schedules,
    List<FeaturedItemDTO> FeaturedItems
);