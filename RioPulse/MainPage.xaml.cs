using RioPulse.Core.Models;
using RioPulse.Core.Services;
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
            await _historyService.SaveCharacterSnapshot(characterAndGuildData?.Character); // Save character only

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
        if (characterAndGuildData == null)
        {
            // Handle null data
            CharacterScoreLabel.Text = "No data found.";
            return;
        }

        CharacterNameLabel.Text = characterAndGuildData.Character.Name;
        CharacterRealmLabel.Text = $"Realm: {characterAndGuildData.Character.Realm}";
        CharacterRegionLabel.Text = $"Region: {characterAndGuildData.Character.Region}";

        // Display Mythic+ Score
        if (characterAndGuildData.Character.MythicPlusScoresBySeason != null && characterAndGuildData.Character.MythicPlusScoresBySeason[0].Scores["all"] != null && characterAndGuildData.Character.MythicPlusScoresBySeason[0].Scores["all"] > 0)
        {
            float score = characterAndGuildData.Character.MythicPlusScoresBySeason[0].Scores["all"];
            CharacterScoreLabel.Text = $"Score: {score}";
        }
        else
        {
            CharacterScoreLabel.Text = "Score: No Mythic+ data found.";
        }

        // Display Guildmates' scores (assuming guildmates are sorted by score)
        TableView tableView = new TableView();
        foreach (Character guildmate in characterAndGuildData.GuildMembers)
        {
            if (guildmate.MythicPlusScoresBySeason != null && guildmate.MythicPlusScoresBySeason[0].Scores["all"] != null && guildmate.MythicPlusScoresBySeason[0].Scores["all"] > 0)
            {
                double guildmateScore = guildmate.MythicPlusScoresBySeason[0].Scores["all"];
                tableView.Root.Add(new TableSection { new TextCell { Text = $"{guildmate.Name}", Detail = $"Score: {guildmateScore}" } });
            }
        }

        // ...other code...
        Character[] sortedGuildmates = characterAndGuildData.GuildMembers
            .OrderByDescending(g => g.MythicPlusScoresBySeason?[0]?.Scores["all"] ?? 0) // handles nulls
            .ToArray();

        foreach (Character guildmate in sortedGuildmates)
        {
            // ...rest of your foreach loop...
        }
        // ...rest of your UpdateUI function...


        // Add the TableView to your UI
        // You'll need to replace this with the appropriate layout and add tableView to your existing layout
        var layout = new VerticalStackLayout { tableView };
        Content = layout;

        CharacterNameLabel.IsVisible = true;
        CharacterRealmLabel.IsVisible = true;
        CharacterRegionLabel.IsVisible = true;
        CharacterScoreLabel.IsVisible = true;
    }




}