using System.Text.Json.Serialization;
namespace RioPulse.Models;

public class Segment
{
    [JsonPropertyName("score")]
    public double Score { get; set; }
    [JsonPropertyName("color")]
    public string Color { get; set; }
}