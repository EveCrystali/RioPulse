using System;
using System.Collections.Generic;

namespace RioPulse.Core.Models;

public class CharacterStatistics
{
    public float CurrentMythicPlusScore { get; set; }
    public float ScoreEvolution { get; set; }
    public float AverageScorePerWeek { get; set; }
    public float AverageScorePerDay { get; set; }
    public int TotalRunsTimed { get; set; }
    public int TotalRunsCompleted { get; set; }
    public int WeeklyRunsCompleted { get; set; }
    public Dictionary<string, int> DungeonCompletion { get; set; }
    public int HighestKeyCompleted { get; set; }
    public Dictionary<string, float> SpecScores { get; set; }
    public int RankServer { get; set; }
    public int RankRegion { get; set; }
    public int RankWorld { get; set; }

    public DateTime LastUpdateTime { get; set; }
    public List<ScoreEntry> ScoreHistory { get; set; } = new List<ScoreEntry>();
    public List<GuildEntry> GuildHistory { get; set; } = new List<GuildEntry>();

    public List<DungeonRun> BestDungeons { get; set; } = new List<DungeonRun>();
    public string WeeklyTrendsSummary { get; set; } // Changed from Dictionary<int, float> to string
    public List<GuildRankEntry> GuildRankHistory { get; set; } = new List<GuildRankEntry>(); // Added for guild rank history
}

// This record might be useful elsewhere, but GuildRankHistory is more direct for historical tracking.
// public record PositionEvolution(
//     DateTime StartDate,
//     DateTime EndDate,
//     int StartRank,
//     int EndRank);

public class GuildRankEntry
{
    public DateTime Timestamp { get; set; }
    public int Rank { get; set; }
    public int GuildMemberCount { get; set; }
}
