namespace RioPulse.Models;

public class MythicPlusBestRun
{
    public string Dungeon { get; set; }
    public string ShortName { get; set; }
    public int MythicLevel { get; set; }
    public long KeystoneRunId { get; set; }
    public DateTime CompletedAt { get; set; }
    public long ClearTimeMs { get; set; }
    public long ParTimeMs { get; set; }
    public int NumKeystoneUpgrades { get; set; }
    public int MapChallengeModeId { get; set; }
    public int ZoneId { get; set; }
    public int ZoneExpansionId { get; set; }
    public string IconUrl { get; set; }
    public string BackgroundImageUrl { get; set; }
    public double Score { get; set; }
    public string Url { get; set; }
    public List<Affix> Affixes { get; set; }
}