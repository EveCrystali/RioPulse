using System.Text.Json;
using RioPulse.Core.Models;
namespace RioPulse.Core.Services;

public class RaiderIoService
{
    private readonly HttpClient _httpClient;

    public RaiderIoService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://raider.io/api/v1/");
    }

    public async Task<Character?> GetCharacterDataAsync(string realm, string name, string region)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"characters/profile?region={region}&realm={realm}&name={name}&fields=mythic_plus_scores");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error during API request : {response.StatusCode}");
                return null;
            }

            string json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Character>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception during API request : {ex.Message}");
            return null;
        }
    }

    public async Task<Guild?> GetGuildDetailsAsync(string region, string realm, string guildName)
    {
        try
        {
            // Construire l'URL avec les paramètres requis
            HttpResponseMessage response = await _httpClient.GetAsync(
                                                                      $"guilds/profile?region={region}&realm={realm}&name={guildName}&fields=members,raid_progression,raid_rankings");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Erreur lors de la requête API : {response.StatusCode}");
                return null;
            }

            // Désérialiser la réponse JSON en un objet Guild
            string json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Guild>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception lors de la requête API : {ex.Message}");
            return null;
        }
    }

    public async Task SaveCharacterDataAsync(Character character, string filePath)
    {
        string json = JsonSerializer.Serialize(character, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        await File.WriteAllTextAsync(filePath, json);
    }

    public async Task<Character?> LoadCharacterDataAsync(string filePath)
    {
        if (!File.Exists(filePath)) return null;

        string json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<Character>(json);
    }
}