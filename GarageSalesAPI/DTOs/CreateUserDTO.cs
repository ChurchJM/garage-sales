using System.Text.Json.Serialization;

public record CreateUserDTO(
    [property: JsonRequired] string UserName,
    [property: JsonRequired] string Password,
    [property: JsonRequired] string Email,
    [property: JsonRequired] string Street,
    [property: JsonRequired] string Zip
);