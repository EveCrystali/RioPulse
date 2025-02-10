using System.Text.Json.Serialization;
namespace RioPulse.Core.Models;

public class RaidRankings
{
    [JsonPropertyName("normal")]
    public Rank Normal { get; set; }

    [JsonPropertyName("heroic")]
    public Rank Heroic { get; set; }

    [JsonPropertyName("mythic")]
    public Rank Mythic { get; set; }
}