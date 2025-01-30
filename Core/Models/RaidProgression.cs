using System.Text.Json.Serialization;


namespace RioPulse.Models;

public class RaidProgression
{
    public string RaidName { get; set; }
    [JsonPropertyName("summary")]
    public string Summary { get; set; }
    [JsonPropertyName("expansion_id")]
    public string ExpansionId { get; set; }
    [JsonPropertyName("total_bosses")]
    public string TotalBosses { get; set; }
    [JsonPropertyName("normal_bosses_killed")]
    public string NormalBossesKilled { get; set; }
    [JsonPropertyName("heroic_bosses_killed")]
    public string HeroicBossesKilled { get; set; }
    [JsonPropertyName("mythic_bosses_killed")]
    public string MythicBossesKilled { get; set; }
}