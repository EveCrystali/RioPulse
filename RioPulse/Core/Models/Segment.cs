using System.Text.Json.Serialization;
namespace RioPulse.Core.Models;

public class MythicPlusSegment
{
    [JsonPropertyName("score")]
    public float Score { get; set; }

    [JsonPropertyName("color")]
    public string Color { get; set; }
}