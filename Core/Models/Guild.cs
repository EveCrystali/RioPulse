using System.Text.Json.Serialization;
namespace RioPulse.Models;

public class Guild
{
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("faction")]
    public string Faction { get; set; }
    [JsonPropertyName("region")]
    public string Region { get; set; }
    [JsonPropertyName("realm")]
    public string Realm { get; set; }
    [JsonPropertyName("profile_url")]
    public string ProfileUrl { get; set; }
    [JsonPropertyName("last_crawled_at")]
    public DateTime? LastCrawledAt { get; set; } 
    [JsonPropertyName("raid_rankings")]
    public Dictionary<string, RaidRankings> RaidRankings { get; set; }
    [JsonPropertyName("raid_progression")]
    public Dictionary<string, RaidProgression> RaidProgression { get; set; }
    [JsonPropertyName("members")]
    public List<Character> Members { get; set; }
}