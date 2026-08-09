using Microsoft.AspNetCore.Mvc;

public record GarageSaleQueryParamsDTO(
    [property: FromQuery] DateTime? AfterDate = null,
    [property: FromQuery] DateTime? BeforeDate = null,
    [property: FromQuery] string? FromStreet = null,
    [property: FromQuery] string? FromZip = null,
    [property: FromQuery] double? RadiusMiles = null,
    [property: FromQuery] string? FeaturedItemCategory = null,
    [property: FromQuery] string? Keyword = null
);