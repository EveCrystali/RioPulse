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
    public int TotalBosses { get; set; } = 0;

    [JsonPropertyName("normal_bosses_killed")]
    public int NormalBossesKilled { get; set; } = 0;

    [JsonPropertyName("heroic_bosses_killed")]
    public int HeroicBossesKilled { get; set; } = 0;

    [JsonPropertyName("mythic_bosses_killed")]
    public int MythicBossesKilled { get; set; } = 0;
}