using System.Text.Json.Serialization;
namespace RioPulse.Models;

public class Rank
{   
    [JsonPropertyName("world")]
    public int World { get; set; }
    [JsonPropertyName("region")]
    public int Region { get; set; }
    [JsonPropertyName("realm")]
    public int Realm { get; set; }
}