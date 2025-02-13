using System.Text.Json.Serialization;
namespace RioPulse.Core.Models;

public class Character
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("race")]
    public string Race { get; set; }

    [JsonPropertyName("class")]
    public string Class { get; set; }

    [JsonPropertyName("active_spec_name")]
    public string? ActiveSpecName { get; set; }

    [JsonPropertyName("active_spec_role")]
    public string? ActiveSpecRole { get; set; }

    [JsonPropertyName("gender")]
    public string Gender { get; set; }

    [JsonPropertyName("faction")]
    public string Faction { get; set; }

    [JsonPropertyName("achievement_points")]
    public int? AchievementPoints { get; set; }

    [JsonPropertyName("thumbnail_url")]
    public string? ThumbnailUrl { get; set; }

    [JsonPropertyName("region")]
    public string Region { get; set; }

    [JsonPropertyName("realm")]
    public string Realm { get; set; }

    [JsonPropertyName("last_crawled_at")]
    public string? LastCrawledAt { get; set; }

    [JsonPropertyName("profile_url")]
    public string ProfileUrl { get; set; }

    [JsonPropertyName("profile_banner")]
    public string? ProfileBanner { get; set; }

    [JsonPropertyName("use_animated_banner")]
    public bool? UseAnimatedBanner { get; set; }

    [JsonPropertyName("gear")]
    public Gear? Gear { get; set; }

    [JsonPropertyName("raid_progression")]
    public Dictionary<string, RaidProgression>? RaidProgression { get; set; }

    [JsonPropertyName("mythic_plus_scores_by_season")]
    public List<MythicPlusScoresBySeason>? MythicPlusScoresBySeason { get; set; }

    [JsonPropertyName("guild")]
    public Guild? Guild { get; set; }
}