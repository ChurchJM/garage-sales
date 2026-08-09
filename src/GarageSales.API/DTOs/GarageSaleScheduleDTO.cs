using System.Text.Json.Serialization;

public record GarageSaleScheduleDTO(
    [property: JsonRequired] DateTime From,
    [property: JsonRequired] DateTime To
);