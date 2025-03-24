using System;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using RioPulse.Core.Models;

namespace RioPulse.Core.Services;

public class CharacterAnalysisService
{
    private readonly CharacterHistoryService _characterHistoryService;
    private readonly RaiderIoService _raiderIoService;

    private readonly string _dataPath;

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
        string[] characterFiles = Directory.GetFiles(Path.Combine(_dataPath, characterName), "*.json");
        CharacterStatistics statistics = new CharacterStatistics();

        // Sort files by modification date (oldest first)
        foreach (string? file in characterFiles.OrderBy(f => File.GetLastWriteTime(f)))
        {
            // load the snapshot 
            Character? character = null;
            try
            {
                if (file.Contains("Extended"))
                {
                    ExtendedCharacterSnapshot? extendedSnapshot = await LoadJsonAsync<ExtendedCharacterSnapshot>(file);
                    if (extendedSnapshot != null)
                    {
                        character = extendedSnapshot.Character;
                    }
                }
                else
                {
                    CharacterSnapshot? snapshot = await LoadJsonAsync<CharacterSnapshot>(file);
                    if (snapshot != null)
                    {
                        character = snapshot.Character;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading or processing file {file}: {ex.Message}");
                continue; // Passer au fichier suivant en cas d'erreur
            }

            if (character != null)
            {
                UpdateStatistics(statistics, character, file);
            }
        }

        return statistics;
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
                    // Utiliser la date de modification du fichier comme timestamp
                    Timestamp = File.GetLastWriteTime(filePath),
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

    private async Task<T?> LoadJsonAsync<T>(string filePath)
    {
        if (!File.Exists(filePath)) return default;

        string json = await File.ReadAllTextAsync(filePath);
        try
        {
            return JsonSerializer.Deserialize<T>(json);
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Error deserializing JSON from file {filePath}: {ex.Message}");
            return default;
        }
    }
}
