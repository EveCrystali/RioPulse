using RioPulse.Core.Models;
using RioPulse.Core.Services;
using RioPulse.UI.ViewModels;
namespace RioPulse;

public partial class MainPage : ContentPage
{
    private readonly CharacterHistoryService _historyService;
    private readonly RaiderIoService _raiderIoService;

    private readonly CharacterOrchestrator _orchestrator;
    public MainPage(CharacterHistoryService historyService, RaiderIoService raiderIoService)
    {
        InitializeComponent();
        _historyService = historyService;
        _raiderIoService = raiderIoService;
        _orchestrator = new CharacterOrchestrator(_raiderIoService, _historyService);
    }

    private async void SearchButton_Clicked(object sender, EventArgs e)
    {
        await UpdateCharacterDataAsync(RegionEntry.Text, RealmEntry.Text, CharacterNameEntry.Text);
    }

    private async Task UpdateCharacterDataAsync(string region, string realm, string characterName)
    {
        try
        {
            //Get Character and Guild data
            ExtendedCharacterSnapshot? characterAndGuildData = await _orchestrator.UpdateCharacterAndGuildSnapshotAsync(region, realm, characterName);

            // Save the snapshot
            await _historyService.SaveCharacterSnapshot(characterAndGuildData?.Character); 

            // Update UI - you'll need to modify this considerably
            UpdateUI(characterAndGuildData);

        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not update data: {ex.Message}", "OK");
        }
    }


    private void UpdateUI(ExtendedCharacterSnapshot? characterAndGuildData)
    {
        CharacterDataFrame.IsVisible = characterAndGuildData != null;
        GuildMembersFrame.IsVisible = characterAndGuildData != null && characterAndGuildData.GuildMembers?.Count > 0;

        if (characterAndGuildData != null)
        {
            // Mise à jour des informations du personnage
            CharacterNameLabel.Text = characterAndGuildData.Character.Name;

            double score = characterAndGuildData.Character.MythicPlusScoresBySeason?[0]?.Scores["all"] ?? 0;
            CharacterScoreLabel.Text = $"🏆 Score: {score:F1}";

            // Mise à jour des membres de la guilde
            List<GuildMemberDisplay>? guildMembers = characterAndGuildData.GuildMembers?
                .Where(g => g.MythicPlusScoresBySeason?.Count > 0 && g.MythicPlusScoresBySeason[0].Scores.ContainsKey("all"))
                .Select(g => new GuildMemberDisplay
                {
                    Name = g.Name,
                    Score = g.MythicPlusScoresBySeason[0].Scores["all"],
                })
                .OrderByDescending(g => g.Score)
                .ToList();

            GuildMembersCollection.ItemsSource = guildMembers;
        }
        else
        {
            CharacterScoreLabel.Text = "No data available.";
        }
    }
}
