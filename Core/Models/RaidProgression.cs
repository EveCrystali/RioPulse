namespace RioPulse.Models;

public class RaidProgression
{
    public string RaidName { get; set; }
    public string Summary { get; set; }
    public string ExpansionId { get; set; }
    public string TotalBosses { get; set; }
    public string NormalBossesKilled { get; set; }
    public string HeroicBossesKilled { get; set; }
    public string MythicBossesKilled { get; set; }
}