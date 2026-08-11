using System.Text.Json.Serialization;

public record LoginDTO(
    [property: JsonRequired] string UserName, 
    [property: JsonRequired] string Password);