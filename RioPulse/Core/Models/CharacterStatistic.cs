using System;
using System.Collections.Generic;

namespace RioPulse.Core.Models;

public class CharacterStatistics
{
    public float CurrentMythicPlusScore { get; set; }
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
    public float ScoreEvolution { get; set; }
    public DateTime LastUpdateTime { get; set; }
    public List<ScoreEntry> ScoreHistory { get; set; } = new List<ScoreEntry>();
    public List<GuildEntry> GuildHistory { get; set; } = new List<GuildEntry>();
}
