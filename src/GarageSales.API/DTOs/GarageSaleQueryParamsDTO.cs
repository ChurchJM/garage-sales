public record GarageSaleQueryParamsDTO(
    DateTime? AfterDate = null,
    DateTime? BeforeDate = null,
    string? FromStreet = null,
    string? FromZip = null,
    double? RadiusMiles = null,
    string? FeaturedItemCategory = null,
    string? Keyword = null
);