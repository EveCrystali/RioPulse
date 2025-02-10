using System.Text.Json.Serialization;
namespace RioPulse.Core.Models;

public class MythicPlusRecentRun
{
    [JsonPropertyName("dungeon")]
    public string Dungeon { get; set; }

    [JsonPropertyName("short_name")]
    public string ShortName { get; set; }

    [JsonPropertyName("mythic_level")]
    public int MythicLevel { get; set; }

    [JsonPropertyName("keystone_run_id")]
    public long KeystoneRunId { get; set; }

    [JsonPropertyName("completed_at")]
    public DateTime CompletedAt { get; set; }

    [JsonPropertyName("clear_time_ms")]
    public long ClearTimeMs { get; set; }

    [JsonPropertyName("par_time_ms")]
    public long ParTimeMs { get; set; }

    [JsonPropertyName("num_keystone_upgrades")]
    public int NumKeystoneUpgrades { get; set; }

    [JsonPropertyName("map_challenge_mode_id")]
    public int MapChallengeModeId { get; set; }

    [JsonPropertyName("zone_id")]
    public int ZoneId { get; set; }

    [JsonPropertyName("zone_expansion_id")]
    public int ZoneExpansionId { get; set; }

    [JsonPropertyName("icon_url")]
    public string IconUrl { get; set; }

    [JsonPropertyName("background_image_url")]
    public string BackgroundImageUrl { get; set; }

    [JsonPropertyName("score")]
    public double Score { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }

    [JsonPropertyName("affixes")]
    public List<Affix> Affixes { get; set; }
}