using System;
using System.Collections.Generic;

namespace RioPulse.Core.Models;

/// <summary>
/// Represents a specific Mythic+ dungeon run, typically used for tracking best performances in CharacterStatistics.
/// </summary>
public class DungeonRun
{
    public string DungeonName { get; set; }
    public int MythicLevel { get; set; }
    public long ClearTimeMs { get; set; }
    public double Score { get; set; }
    public int NumKeystoneUpgrades { get; set; }
    public List<Affix> Affixes { get; set; } = new List<Affix>();
    public DateTime Timestamp { get; set; } // Timestamp of the snapshot when this run was recorded as a best
}