using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RioPulse.Core.Models;

public abstract class BaseMythicPlusRun : IMythicPlusRun
{
    [JsonPropertyName("dungeon")]
    public virtual string Dungeon { get; set; } = string.Empty;

    [JsonPropertyName("short_name")]
    public virtual string ShortName { get; set; } = string.Empty;

    [JsonPropertyName("mythic_level")]
    public virtual int MythicLevel { get; set; }

    [JsonPropertyName("keystone_run_id")]
    public virtual long KeystoneRunId { get; set; }

    [JsonPropertyName("completed_at")]
    public virtual DateTime CompletedAt { get; set; }

    [JsonPropertyName("clear_time_ms")]
    public virtual long ClearTimeMs { get; set; }

    [JsonPropertyName("par_time_ms")]
    public virtual long ParTimeMs { get; set; }

    [JsonPropertyName("num_keystone_upgrades")]
    public virtual int NumKeystoneUpgrades { get; set; }

    [JsonPropertyName("map_challenge_mode_id")]
    public virtual int MapChallengeModeId { get; set; }

    [JsonPropertyName("zone_id")]
    public virtual int ZoneId { get; set; }

    [JsonPropertyName("zone_expansion_id")]
    public virtual int ZoneExpansionId { get; set; }

    [JsonPropertyName("icon_url")]
    public virtual string IconUrl { get; set; } = string.Empty;

    [JsonPropertyName("background_image_url")]
    public virtual string BackgroundImageUrl { get; set; } = string.Empty;

    [JsonPropertyName("score")]
    public virtual double Score { get; set; }

    [JsonPropertyName("url")]
    public virtual string Url { get; set; } = string.Empty;

    [JsonPropertyName("affixes")]
    public virtual List<Affix> Affixes { get; set; } = new List<Affix>();
}