using System.Text.Json.Serialization;
namespace RioPulse.Models;

public class MythicPlusScoresBySeason
{
    [JsonPropertyName("season")]
    public string Season { get; set; }
    [JsonPropertyName("scores")]
    public Scores Scores { get; set; }
    [JsonPropertyName("segments")]
    public Segments Segments { get; set; }
}