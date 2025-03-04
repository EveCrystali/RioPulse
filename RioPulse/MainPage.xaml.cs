using RioPulse.Core.Models;
using RioPulse.Core.Services;
namespace RioPulse;

public partial class MainPage : ContentPage
{
    private readonly CharacterHistoryService _historyService;
    private readonly RaiderIoService _raiderIoService;

    public MainPage(CharacterHistoryService historyService, RaiderIoService raiderIoService)
    {
        InitializeComponent();
        _historyService = historyService;
        _raiderIoService = raiderIoService;
    }

    private async void SearchButton_Clicked(object sender, EventArgs e)
    {
        await UpdateCharacterDataAsync(RegionEntry.Text, RealmEntry.Text, CharacterNameEntry.Text);
    }

    private async Task UpdateCharacterDataAsync(string region, string realm, string characterName)
    {
        try
        {
            // Retrieving data via the API
            Character? character = await _raiderIoService.GetCharacterDataAsync(region, realm, characterName);

            // Saving the snapshot
            await _historyService.SaveCharacterSnapshot(character);

            // Retrieving history for display/analysis
            List<CharacterSnapshot> history = await _historyService.GetCharacterHistory(characterName);

            // TODO: Use the history to update the UI
            UpdateUI(character, history);

        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not update data: {ex.Message}", "OK");
        }
    }

    private void UpdateUI(Character character, List<CharacterSnapshot> history)
    {
        CharacterNameLabel.Text = character.Name;
        CharacterRealmLabel.Text = $"Realm: {character.Realm}";
        CharacterRegionLabel.Text = $"Region: {character.Region}";
        if (character != null && character.MythicPlusScoresBySeason[0] != null && character.MythicPlusScoresBySeason[0].Scores != null && character.MythicPlusScoresBySeason[0].Scores.Count > 0)
        {
            // Assuming you want the score from the first season
            double score = character.MythicPlusScoresBySeason[0].Scores["all"];
            CharacterScoreLabel.Text = $"Score: {score}";
        }
        else
        {
            CharacterScoreLabel.Text = "Score: No Mythic+ data found.";
        }

        // CharacterHistoryCollectionView.ItemsSource = history;

        CharacterNameLabel.IsVisible = true;
        CharacterRealmLabel.IsVisible = true;
        CharacterRegionLabel.IsVisible = true;
        CharacterScoreLabel.IsVisible = true;
        // CharacterHistoryCollectionView.IsVisible = true;
    }



}