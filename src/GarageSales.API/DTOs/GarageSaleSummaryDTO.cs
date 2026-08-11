using System.Text.Json.Serialization;

public record GarageSaleSummaryDTO(
    [property: JsonRequired] int Id,
    string? Owner, // null unless admin is requesting the record.
    [property: JsonRequired] string SaleType,
    [property: JsonRequired] string Street,
    [property: JsonRequired] string Zip,
    string? Description,
    double? DistanceMiles,
    [property: JsonRequired] List<GarageSaleScheduleDTO> Schedules,
    List<FeaturedItemDTO> FeaturedItems
);