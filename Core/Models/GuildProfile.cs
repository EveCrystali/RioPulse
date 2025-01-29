namespace RioPulse.Models;

public class GuildProfile
{
    public string Name { get; set; }
    public string Faction { get; set; }
    public string Region { get; set; }
    public string Realm { get; set; }
    public string ProfileUrl { get; set; }
    public Dictionary<string, RaidRankings> RaidRankings { get; set; }
    public Dictionary<string, RaidProgression> RaidProgression { get; set; }
}