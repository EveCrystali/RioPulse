namespace RioPulse.Models;

public class Character
{
    public string Name { get; set; }
    public string Race { get; set; }
    public string Class { get; set; }
    public string ActiveSpecName { get; set; }
    public string ActiveSpecRole { get; set; }
    public string Gender { get; set; }
    public string Faction { get; set; }
    public string ThumbnailUrl { get; set; }
    public string Region { get; set; }
    public string Realm { get; set; }
    public string LastCrawledAt { get; set; }
    public string ProfileUrl { get; set; }
    public string ProfileBanner { get; set; }
    public string UseAnimatedBanner { get; set; }
    public Gear Gear { get; set; }
    public List<RaidProgression> RaidsProgression { get; set; }
    public List<MythicPlusScoresBySeason> MythicPlusScoresBySeason { get; set; }
}