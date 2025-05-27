using System;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
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

        CharacterStatistics statistics = new();

        if (!snapshots.Any())
            return statistics;

        await ProcessHistoricalData(snapshots, statistics);

        // Extraire la région du personnage à partir du plus ancien snapshot pour la passer à CalculateMetrics
        // Ceci correspond à la logique que vous aviez avec orderedCharacterSnapshot.FirstOrDefault()
        string characterRegion = snapshots.OrderBy(s => s.Timestamp).FirstOrDefault()?.Character.Region ?? "eu";
        CalculateMetrics(statistics, characterRegion);

        return statistics;
    }

    private async Task ProcessHistoricalData(IEnumerable<CharacterSnapshot> snapshots, CharacterStatistics stats)
    {
        List<CharacterSnapshot> orderedCharacterSnapshot = [.. snapshots.OrderBy(s => s.Timestamp)];

        foreach (CharacterSnapshot? snapshot in orderedCharacterSnapshot)
        {
            UpdateScoreHistory(snapshot!, stats); // Assumant que snapshot ne sera pas null ici après filtrage/validation en amont si nécessaire
            await TrackGuildEvolution(snapshot!, stats); // Rendre asynchrone si des opérations I/O sont faites (comme c'est le cas)
            TrackDungeonBests(snapshot!, stats);
        }

        stats.CurrentMythicPlusScore = orderedCharacterSnapshot[^1].Character.MythicPlusScoresBySeason?[0]?.Scores["all"] ?? 0;
    }

    private void CalculateMetrics(CharacterStatistics stats, string characterRegion)
    {
        if (stats.ScoreHistory.Count < 2) return;

        // Calcul de tendance linéaire
        stats.ScoreEvolution = CalculateLinearTrend(stats.ScoreHistory.OrderBy(s => s.Timestamp).Select(s => s.Score).ToArray());

        // Détection des patterns en utilisant la région passée en paramètre
        stats.WeeklyTrendsSummary = DetectWeeklyPatterns(stats.ScoreHistory, characterRegion);
    }

    private static float CalculateLinearTrend(float[] scores)
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

    private static void UpdateScoreHistory(CharacterSnapshot snapshot, CharacterStatistics stats)
    {
        if (snapshot.Character.MythicPlusScoresBySeason != null && snapshot.Character.MythicPlusScoresBySeason.Count > 0)
        {
            var currentSeasonScores = snapshot.Character.MythicPlusScoresBySeason[0];
            if (currentSeasonScores.Scores.TryGetValue("all", out float score))
            {
                stats.ScoreHistory.Add(new ScoreEntry
                {
                    Timestamp = snapshot.Timestamp,
                    Score = score
                });
            }
        }
    }

    private async Task TrackGuildEvolution(CharacterSnapshot snapshot, CharacterStatistics stats) // Doit être async à cause de LoadJsonAsync
    {
        // snapshot.Character.Guild est susceptible de ne pas avoir GuildMembers rempli
        // si le CharacterSnapshot est celui du personnage principal et non un membre de guilde enrichi.
        // La logique actuelle repose sur snapshot.Character.Guild.GuildMembers pour obtenir la liste des membres.
        // Si cette liste n'est pas disponible dans le snapshot du personnage principal, cette méthode ne pourra pas fonctionner comme prévu.
        // Une solution serait de charger le ExtendedCharacterSnapshot correspondant au timestamp du snapshot actuel,
        // ou de s'assurer que Character.Guild.GuildMembers est peuplé en amont.

        if (snapshot.Character.Guild == null || string.IsNullOrEmpty(snapshot.Character.Guild.Name))
        {
            // Pas de guilde ou nom de guilde manquant, impossible de suivre l'évolution.
            return;
        }

        // Si GuildMembers n'est pas dans le snapshot, on ne peut pas calculer le rang.
        // La logique ci-dessous suppose que GuildMembers *est* disponible, même si c'est juste une liste de noms/royaumes.
        // Elle tente ensuite de charger le *dernier* snapshot de chaque membre, ce qui n'est pas historiquement exact
        // pour le rang au moment du `snapshot` analysé.
        // Pour une analyse historiquement correcte, il faudrait les scores des membres au moment de `snapshot.Timestamp`.

        if (snapshot.Character.Guild.GuildMembers == null || !snapshot.Character.Guild.GuildMembers.Any())
        {
            // Console.WriteLine($"TrackGuildEvolution: No guild members found in snapshot for {snapshot.Character.Name} at {snapshot.Timestamp}. Cannot determine rank.");
            return;
        }

        var latestScores = new Dictionary<string, float>();
        foreach (GuildMember member in snapshot.Character.Guild.GuildMembers)
        {
            if (member.Character == null || string.IsNullOrEmpty(member.Character.Name))
                continue;

            // Tenter de charger le snapshot le plus récent pour ce membre
            // Note: Ceci donne le score actuel du membre, pas son score au moment du 'snapshot' du personnage principal.
            // Pour un rang historiquement précis, il faudrait accéder aux scores des membres tels qu'ils étaient à snapshot.Timestamp.
            string? latestFile = GetLatestJsonFile(member.Character.Name); // Assurez-vous que _dataPath est correct pour ce chemin
            if (!string.IsNullOrEmpty(latestFile))
            {
                CharacterSnapshot? memberSnapshot = await LoadJsonAsync(latestFile);
                if (memberSnapshot?.Character?.MythicPlusScoresBySeason?.FirstOrDefault()?.Scores.TryGetValue("all", out float score) == true)
                {
                    latestScores[member.Character.Name] = score;
                }
                else
                {
                    latestScores[member.Character.Name] = 0; // Default score if not found
                }
            }
            else
            {
                latestScores[member.Character.Name] = 0; // Default score if no file found
            }
        }

        // S'assurer que le personnage principal est dans la liste des scores (au cas où son propre "dernier fichier" n'a pas été chargé explicitement)
        if (!latestScores.ContainsKey(snapshot.Character.Name))
        {
            var mainCharacterScore = snapshot.Character.MythicPlusScoresBySeason?.FirstOrDefault()?.Scores.TryGetValue("all", out float score) == true ? score : 0;
            latestScores[snapshot.Character.Name] = mainCharacterScore;
        }


        int guildRank = latestScores.Any() ? latestScores.OrderByDescending(x => x.Value).ToList().FindIndex(x => x.Key == snapshot.Character.Name) + 1 : 0;
        if (guildRank == 0 && latestScores.Any()) // Si FindIndex retourne -1 (non trouvé) mais qu'il y a des scores
        {
            // Cela peut arriver si le personnage principal n'est pas dans la liste des membres de la guilde du snapshot,
            // ou si son nom ne correspond pas.
            Console.WriteLine($"Warning: Character {snapshot.Character.Name} not found in guild score list for rank calculation at {snapshot.Timestamp}.");
        }


        stats.GuildRankHistory.Add(new GuildRankEntry
        {
            Timestamp = snapshot.Timestamp,
            Rank = guildRank,
            GuildMemberCount = snapshot.Character.Guild.GuildMembers.Count // Nombre de membres au moment du snapshot
        });
    }

    private void TrackDungeonBests(CharacterSnapshot snapshot, CharacterStatistics stats)
    {
        List<IMythicPlusRun> runsToConsider = new();
        if (snapshot.Character.MythicPlusBestRuns != null)
        {
            runsToConsider.AddRange(snapshot.Character.MythicPlusBestRuns.Cast<IMythicPlusRun>());
        }
        if (snapshot.Character.MythicPlusAlternateRuns != null)
        {
            // Raider.IO API: "alternate runs are still good scores but not high enough to be part of the character's main score"
            // On peut les inclure si on veut une vue plus large, ou les ignorer si on ne veut que les "top"
            // Pour l'instant, incluons-les pour la comparaison.
            runsToConsider.AddRange(snapshot.Character.MythicPlusAlternateRuns.Cast<IMythicPlusRun>());
        }

        foreach (var run in runsToConsider)
        {
            if (run == null || string.IsNullOrEmpty(run.Dungeon))
                continue;

            var existingBest = stats.BestDungeons.FirstOrDefault(d => d.DungeonName == run.Dungeon);
            bool isNewOrBetter = false;

            if (existingBest == null)
            {
                isNewOrBetter = true;
            }
            else
            {
                // Comparaison: Niveau de clé > Score > Temps de complétion (plus bas est mieux)
                if (run.MythicLevel > existingBest.MythicLevel)
                {
                    isNewOrBetter = true;
                }
                else if (run.MythicLevel == existingBest.MythicLevel)
                {
                    if (run.Score > existingBest.Score) // Le score Raider.IO est un bon indicateur global
                    {
                        isNewOrBetter = true;
                    }
                    else if (run.Score == existingBest.Score && run.ClearTimeMs < existingBest.ClearTimeMs)
                    {
                        isNewOrBetter = true;
                    }
                }
            }

            if (isNewOrBetter)
            {
                if (existingBest != null)
                {
                    stats.BestDungeons.Remove(existingBest);
                }

                stats.BestDungeons.Add(new DungeonRun // Assurez-vous que le modèle DungeonRun dans CharacterStatistics correspond
                {
                    DungeonName = run.Dungeon,
                    MythicLevel = run.MythicLevel,
                    ClearTimeMs = run.ClearTimeMs,
                    Score = run.Score,
                    NumKeystoneUpgrades = run.NumKeystoneUpgrades,
                    Affixes = run.Affixes?.Select(a => new Models.Affix { Name = a.Name, Description = a.Description }).ToList() ?? new List<Models.Affix>(), // Mapper si les modèles diffèrent
                    Timestamp = snapshot.Timestamp // Timestamp du snapshot où ce run a été observé comme un "best"
                });
            }
        }
    }

    // Method to get the latest .json file of a character
    private string? GetLatestJsonFile(string characterName)
    {
        // S'assurer que characterName est nettoyé pour éviter les problèmes de chemin (Path.Combine gère la plupart des cas)
        // et que _dataPath est correctement initialisé.
        if (string.IsNullOrWhiteSpace(characterName) || string.IsNullOrWhiteSpace(_dataPath))
        {
            return null;
        }
        string characterSpecificPath = Path.Combine(_dataPath, characterName);
        if (!Directory.Exists(characterSpecificPath))
        {
            return null;
        }

        return Directory.GetFiles(characterSpecificPath, "*.json")
                        .OrderByDescending(f => File.GetCreationTimeUtc(f)) // Ou GetLastWriteTimeUtc(f)
                        .FirstOrDefault();
    }

    private string DetectWeeklyPatterns(List<ScoreEntry> scoreHistory, string region)
    {
        if (scoreHistory == null || scoreHistory.Count < 7) // Besoin d'au moins une semaine de données pour des patterns
            return "Données insuffisantes pour détecter des tendances hebdomadaires.";

        var weeklyChanges = new Dictionary<DayOfWeek, List<float>>();
        for (int i = 0; i < 7; i++)
        {
            weeklyChanges[(DayOfWeek)i] = new List<float>();
        }

        var orderedHistory = scoreHistory.OrderBy(s => s.Timestamp).ToList();
        ScoreEntry? previousEntry = null;

        foreach (var entry in orderedHistory)
        {
            if (previousEntry != null)
            {
                // Calculer le jour de la semaine de la *fin* de la période (entry.Timestamp)
                DayOfWeek dayOfWeek = entry.Timestamp.DayOfWeek;
                float scoreChange = entry.Score - previousEntry.Score;

                // On s'intéresse aux gains positifs
                if (scoreChange > 0)
                {
                    weeklyChanges[dayOfWeek].Add(scoreChange);
                }
            }
            previousEntry = entry;
        }

        var patternSummary = new StringBuilder("Tendances hebdomadaires : ");
        bool foundPattern = false;

        // Analyser les jours avec le plus de gains ou les gains moyens les plus élevés
        var analysis = weeklyChanges
            .Where(kvp => kvp.Value.Any())
            .Select(kvp => new
            {
                Day = kvp.Key,
                AverageGain = kvp.Value.Average(),
                TotalGain = kvp.Value.Sum(),
                GainCount = kvp.Value.Count
            })
            .OrderByDescending(x => x.AverageGain) // Ou TotalGain, ou GainCount
            .ToList();

        if (analysis.Any())
        {
            var bestDay = analysis[0];
            patternSummary.Append($"Gain moyen le plus élevé le {bestDay.Day} ({bestDay.AverageGain:F2} points sur {bestDay.GainCount} occurrences). ");

            // Mentionner le jour du reset
            DayOfWeek resetDay = GetStartOfWeek(DateTime.Today, region).DayOfWeek; // Le jour où la semaine "commence" pour le score
            // Le reset est le mardi pour US (semaine commence mardi), mercredi pour EU (semaine commence mercredi)
            // Les gains importants sont souvent observés après le reset.
            var gainsPostReset = weeklyChanges[resetDay];
            if (gainsPostReset.Any())
            {
                patternSummary.Append($"Gains notables observés le jour du reset ({resetDay}): moyenne {gainsPostReset.Average():F2}.");
            }
            foundPattern = true;
        }

        if (!foundPattern)
        {
            patternSummary.Append("Aucun pattern hebdomadaire clair détecté.");
        }

        return patternSummary.ToString();
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
        List<float> weeklyScoreChanges = [];
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
            using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            return await JsonSerializer.DeserializeAsync<CharacterSnapshot>(memoryStream);
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
