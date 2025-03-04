using System.Text.Json;
using Microsoft.Extensions.Configuration;
using RioPulse.Core.Models;
namespace RioPulse.Core.Services;

public class RaiderIoService
{
    private readonly HttpClient _httpClient;

    public RaiderIoService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(configuration["RaiderIoApi:BaseAddress"]);
    }

    public async Task<Character?> GetCharacterDataAsync(string region, string realm, string name)
    {
        try
        {
            // HttpResponseMessage response = await _httpClient.GetAsync($"characters/profile?region={region}&realm={realm}&name={name}&fields=mythic_plus_scores");
            HttpResponseMessage response = await _httpClient.GetAsync($"characters/profile?region={region}&realm={realm}&name={name}&fields=mythic_plus_scores_by_season:season-tww-1" );
            // HttpResponseMessage response = await _httpClient.GetAsync($"characters/profile?region={region}&realm={realm}&name={name}&fields=mythic_plus_scores_by_season:current" );


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

    public static async Task<Character?> LoadCharacterDataAsync(string filePath)
    {
        if (!File.Exists(filePath)) return null;

        string json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<Character>(json);
    }
}