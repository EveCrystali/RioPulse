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