using System.Text.Json.Serialization;
namespace RioPulse.Core.Models;

public class MythicPlusScoresBySeason
{
    [JsonPropertyName("season")]
    public string Season { get; set; }

    [JsonPropertyName("scores")]
    public Dictionary<string, float> Scores { get; set; }

    [JsonPropertyName("segments")]
    public Dictionary<string, MythicPlusSegment>? Segments { get; set; }
}