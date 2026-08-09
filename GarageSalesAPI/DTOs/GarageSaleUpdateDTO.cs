using System.Text.Json.Serialization;

// GarageSaleUpdateDTO is identical to GarageSaleCreateDTO, but my understanding is that having separate
// DTOs is the standard design pattern (for future-proofing).
public record GarageSaleUpdateDTO(
    [property: JsonRequired] string SaleType,
    [property: JsonRequired] string Owner,
    [property: JsonRequired] string Street,
    [property: JsonRequired] string Zip,
    string? Description,
    [property: JsonRequired] List<GarageSaleScheduleDTO> Schedules,
    List<FeaturedItemDTO> FeaturedItems
);