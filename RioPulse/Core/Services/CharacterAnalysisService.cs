using System;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using RioPulse.Core.Models;

namespace RioPulse.Core.Services;

public class CharacterAnalysisService
{
    private readonly CharacterHistoryService _characterHistoryService;
    private readonly RaiderIoService _raiderIoService;

    private readonly string _dataPath;

    private readonly ConcurrentDictionary<string, List<CharacterSnapshot>> _snapshotCache = new();


    public CharacterAnalysisService(RaiderIoService raiderIoService, CharacterHistoryService characterHistoryService)
    {
        _raiderIoService = raiderIoService;
        _characterHistoryService = characterHistoryService;
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        _dataPath = configuration["DataStorage"];
    }

    public async Task<CharacterStatistics> AnalyzeCharacterProgressionAsync(string characterName)
    {

        if (string.IsNullOrWhiteSpace(characterName))
            throw new ArgumentException("Nom de personnage invalide");

        if (characterName.Any(c => !char.IsLetterOrDigit(c)))
            throw new ArgumentException("Caractères spéciaux interdits");

        if (!_snapshotCache.TryGetValue(characterName, out List<CharacterSnapshot>? snapshots))
        {
            snapshots = await _characterHistoryService.GetCharacterHistory(characterName);
            _snapshotCache.TryAdd(characterName, snapshots);
        }

        string[] characterFiles = Directory.GetFiles(Path.Combine(_dataPath, characterName), "*.json");
        CharacterStatistics statistics = new CharacterStatistics();

        if (!snapshots.Any())
            return statistics;

        ProcessHistoricalData(snapshots, statistics);
        CalculateMetrics(statistics);

        return statistics;
    }

    private void ProcessHistoricalData(IEnumerable<CharacterSnapshot> snapshots, CharacterStatistics stats)
    {
        List<CharacterSnapshot> orderedCharacterSnapshot = snapshots.OrderBy(s => s.Timestamp).ToList();

        foreach (CharacterSnapshot? snapshot in orderedCharacterSnapshot)
        {
            UpdateScoreHistory(snapshot, stats);
            TrackGuildEvolution(snapshot, stats);
            TrackDungeonBests(snapshot, stats);
        }

        stats.CurrentMythicPlusScore = orderedCharacterSnapshot.Last().Character.MythicPlusScoresBySeason?[0]?.Scores["all"] ?? 0;
    }

    private void CalculateMetrics(CharacterStatistics stats)
    {
        if (stats.ScoreHistory.Count < 2) return;

        float[] scores = stats.ScoreHistory
            .OrderBy(s => s.Timestamp)
            .Select(s => s.Score)
            .ToArray();

        // Calcul de tendance linéaire
        stats.ScoreEvolution = CalculateLinearTrend(scores);

        // Détection des patterns
        stats.WeeklyTrends = DetectWeeklyPatterns(scores);
    }

    private float CalculateLinearTrend(float[] scores)
    {
        float[] xValues = Enumerable.Range(0, scores.Length).Select(i => (float)i).ToArray();
        float[] yValues = scores;

        float meanX = xValues.Average();
        float meanY = yValues.Average();

        float numerator = 0;
        float denominator = 0;

        for (int i = 0; i < xValues.Length; i++)
        {
            numerator += (xValues[i] - meanX) * (yValues[i] - meanY);
            denominator += (xValues[i] - meanX) * (xValues[i] - meanX);
        }

        const float epsilon = 1e-6f;
        return Math.Abs(denominator) < epsilon ? 0 : numerator / denominator;
    }


    private void UpdateStatistics(CharacterStatistics statistics, Character character, string filePath)
    {
        if (character.MythicPlusScoresBySeason != null && character.MythicPlusScoresBySeason.Count > 0)
        {
            MythicPlusScoresBySeason currentSeasonScores = character.MythicPlusScoresBySeason[0];

            // Update current score (always the best since files are ordered by date)
            if (currentSeasonScores.Scores.TryGetValue("all", out float value))
            {
                statistics.CurrentMythicPlusScore = value;
            }

            // Update score history
            if (currentSeasonScores.Scores.ContainsKey("all"))
            {
                statistics.ScoreHistory.Add(new ScoreEntry
                {
                    Timestamp = DateTime.TryParse(Path.GetFileNameWithoutExtension(filePath), System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime parsedDate) ? parsedDate : File.GetLastWriteTime(filePath),
                    Score = currentSeasonScores.Scores["all"]
                });
            }
        }

        // Update guild history
        if (character.Guild != null)
        {
            statistics.GuildHistory.Add(new GuildEntry
            {
                // Utiliser la date de modification du fichier comme timestamp
                Timestamp = File.GetLastWriteTime(filePath),
                GuildName = character.Guild.Name
            });
        }
    }

    private void TrackGuildEvolution(CharacterSnapshot snapshot, CharacterStatistics stats)
    {
        if (snapshot.Character.Guild?.GuildMembers == null)
            return;

        Dictionary<string, float> latestScores = new Dictionary<string, float>();
        foreach (GuildMember member in snapshot.Character.Guild.GuildMembers)
        {
            string latestFile = GetLatestJsonFile(member.Character.Name);
            if (latestFile != null)
            {
                CharacterSnapshot json = LoadJsonAsync(latestFile).Result;
                var scores = json.MythicPlusScoresBySeason?[0]?.Scores;
                if (scores != null)
                {
                    latestScores[member.Character.Name] = scores["all"] ?? 0;
                }
            }
        }

        int guildRank = latestScores.OrderByDescending(x => x.Value).ToList().FindIndex(x => x.Key == snapshot.Character.Name) + 1;

        stats.GuildRankHistory.Add(new GuildRankEntry
        {
            Timestamp = snapshot.Timestamp,
            Rank = guildRank,
            GuildMemberCount = snapshot.Character.Guild.GuildMembers.Count
        });
    }

    // Method to get the latest .json file of a character
    private string GetLatestJsonFile(string characterName)
    {
        List<string> characterFiles = Directory.GetFiles(Path.Combine(_dataPath, characterName), "*.json")
            .OrderBy(f => f)
            .ToList();

        return characterFiles.LastOrDefault();
    }



    private void CalculateScoreEvolution(CharacterStatistics statistics, Character character)
    {
        if (statistics.ScoreHistory.Count < 2)
        {
            statistics.ScoreEvolution = 0; // Not enough data to calculate evolution
            return;
        }

        // Sort score history by timestamp
        List<ScoreEntry> sortedScoreHistory = statistics.ScoreHistory.OrderBy(s => s.Timestamp).ToList();

        IEnumerable<IGrouping<DateTime, ScoreEntry>> weeklyScores = sortedScoreHistory.GroupBy(s => GetStartOfWeek(s.Timestamp, character.Region.ToLower()));

        // Calculate weekly score changes
        List<float> weeklyScoreChanges = new List<float>();
        DateTime? previousWeekStart = null;
        float previousWeekScore = 0;
        foreach (IGrouping<DateTime, ScoreEntry> week in weeklyScores)
        {
            DateTime currentWeekStart = week.Key;
            float currentWeekScore = week.Max(s => s.Score);

            if (previousWeekStart.HasValue)
            {
                weeklyScoreChanges.Add(currentWeekScore - previousWeekScore);
            }

            previousWeekStart = currentWeekStart;
            previousWeekScore = currentWeekScore;
        }

        // Calculate average weekly score change
        if (weeklyScoreChanges.Count > 0)
        {
            statistics.ScoreEvolution = weeklyScoreChanges.Average();
        }
        else
        {
            statistics.ScoreEvolution = 0;
        }
    }

    private static async Task<CharacterSnapshot?> LoadJsonAsync(string filePath)
    {
        if (!File.Exists(filePath)) return default;

        string json = await File.ReadAllTextAsync(filePath);
        try
        {
            return JsonSerializer.Deserialize<CharacterSnapshot>(json);
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Error deserializing JSON from file {filePath}: {ex.Message}");
            return default;
        }
    }

    private static DateTime GetStartOfWeek(DateTime date, string region)
    {
        if (region == "us")
        {
            // Pour la région NA, le reset est le mardi
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Tuesday)) % 7;
            return date.AddDays(-1 * diff).Date;
        }
        else
        {
            // Pour les autres régions (EU, etc.), le reset est le mercredi
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Wednesday)) % 7;
            return date.AddDays(-1 * diff).Date;
        }
    }

}
