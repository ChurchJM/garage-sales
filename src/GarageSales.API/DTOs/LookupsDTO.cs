using System.Text.Json.Serialization;

public record LookupsDTO(
    [property: JsonRequired] List<GarageSaleTypeDTO> GarageSaleTypes,
    [property: JsonRequired] List<ItemCategoryDTO> ItemCategories
);