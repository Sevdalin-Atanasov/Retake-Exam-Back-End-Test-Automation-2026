using System.Text.Json.Serialization;

public class ApiResponseDto
{
    [JsonPropertyName("msg")]
    public string Msg { get; set; }
}