using System.Text.Json;
using RioPulse.Core.Models;
namespace RioPulse.Core.Services;

public class CharacterHistoryService
{
    private readonly string _baseStoragePath;

    public CharacterHistoryService(string baseStoragePath)
    {
        _baseStoragePath = baseStoragePath;
        Directory.CreateDirectory(_baseStoragePath);
    }

    public async Task SaveCharacterSnapshot(Character character)
    {
        DateTime timestamp = DateTime.UtcNow;
        string snapshotPath = GetSnapshotPath(character.Name, timestamp);

        CharacterSnapshot snapshot = new()
        {
            Timestamp = timestamp,
            Character = character
        };

        await SaveJsonAsync(snapshot, snapshotPath);
    }

    public async Task SaveExtendedSnapshot(ExtendedCharacterSnapshot snapshot)
    {
        // Générer le chemin pour sauvegarder le snapshot
        string snapshotPath = GetSnapshotPath(snapshot.Character.Name, snapshot.Timestamp);

        // Sauvegarder les données au format JSON
        await SaveJsonAsync(snapshot, snapshotPath);
    }


    public async Task<List<CharacterSnapshot>> GetCharacterHistory(string characterName)
    {
        string characterPath = Path.Combine(_baseStoragePath, characterName);
        if (!Directory.Exists(characterPath))
            return new List<CharacterSnapshot>();

        List<CharacterSnapshot> snapshots = new();
        foreach (string file in Directory.GetFiles(characterPath, "*.json"))
        {
            CharacterSnapshot? snapshot = await LoadJsonAsync<CharacterSnapshot>(file);
            if (snapshot != null)
                snapshots.Add(snapshot);
        }

        return snapshots.OrderByDescending(s => s.Timestamp).ToList();
    }

    private string GetSnapshotPath(string characterName, DateTime timestamp)
    {
        string characterPath = Path.Combine(_baseStoragePath, characterName);
        Directory.CreateDirectory(characterPath);
        return Path.Combine(characterPath, $"{timestamp:yyyyMMddHHmmss}.json");
    }

    private async Task SaveJsonAsync<T>(T data, string filePath)
    {
        JsonSerializerOptions options = new()
        {
            WriteIndented = true
        };
        await File.WriteAllTextAsync(filePath, JsonSerializer.Serialize(data, options));
    }

    private async Task<T?> LoadJsonAsync<T>(string filePath)
    {
        if (!File.Exists(filePath)) return default;

        string json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<T>(json);
    }
}