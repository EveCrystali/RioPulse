using RioPulse.Core.Models;
using RioPulse.Core.Services;
namespace RioPulse;

public partial class  MainPage : ContentPage
{
    private readonly CharacterHistoryService _historyService;
    private readonly HttpClient _httpClient;
    private readonly RaiderIoService _raiderIoService;

    public MainPage()
    {
        InitializeComponent();
        _historyService = new CharacterHistoryService("./data/characters");
        _httpClient = new HttpClient();
        _raiderIoService = new RaiderIoService(_httpClient);
    }

    private async Task UpdateCharacterData(string region, string realm, string characterName)
    {
        try
        {
            // Récupération des données via l'API
            var character = await _raiderIoService.GetCharacterDataAsync(region, realm, characterName);

            // Sauvegarde du snapshot
            await _historyService.SaveCharacterSnapshot(character);

            // Récupération de l'historique pour affichage/analyse
            List<CharacterSnapshot> history = await _historyService.GetCharacterHistory(characterName);

            // TODO: Utiliser l'historique pour mettre à jour l'interface
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erreur", $"Impossible de mettre à jour les données : {ex.Message}", "OK");
        }
    }
}