using System.Text.Json.Serialization;
namespace RioPulse.Models;

public class MythicPlusSegment
{
    [JsonPropertyName("score")]
    public float Score { get; set; }

    [JsonPropertyName("color")]
    public string Color { get; set; }
}