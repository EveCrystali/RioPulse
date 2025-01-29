using System.Text.Json;
namespace RioPulse.Services;

public class RaiderIoService
{
    private readonly HttpClient _httpClient;

    public RaiderIoService()
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri("https://raider.io/api/v1/");
    }

    public async Task<CharacterData> GetCharacterDataAsync(string realm, string name, string region)
    {
        HttpResponseMessage response = await _httpClient.GetAsync($"characters/profile?region={region}&realm={realm}&name={name}&fields=mythic_plus_scores");
        response.EnsureSuccessStatusCode();

        string json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<CharacterData>(json);
    }
}

public class CharacterData
{
    public string Name { get; set; }
    public string Realm { get; set; }
    public MythicPlusScores MythicPlusScores { get; set; }
}

public class MythicPlusScores
{
    public double All { get; set; }
}