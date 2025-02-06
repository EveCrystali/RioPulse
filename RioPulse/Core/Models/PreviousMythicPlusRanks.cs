using System.Text.Json.Serialization;
namespace RioPulse.Models;

public class PreviousMythicPlusRanks
{
    [JsonPropertyName("overall")]
    public Rank Overall { get; set; }
    [JsonPropertyName("tank")]
    public Rank Tank { get; set; }
    [JsonPropertyName("healer")]
    public Rank Healer { get; set; }
    [JsonPropertyName("dps")]
    public Rank Dps { get; set; }
    [JsonPropertyName("class")]
    public Rank Class { get; set; }
    [JsonPropertyName("class_tank")]
    public Rank ClassTank { get; set; }
    [JsonPropertyName("class_healer")]
    public Rank ClassHealer { get; set; }
    [JsonPropertyName("class_dps")]
    public Rank ClassDps { get; set; }
}