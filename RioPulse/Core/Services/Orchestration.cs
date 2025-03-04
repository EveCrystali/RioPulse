using RioPulse.Core.Models;
using System.Threading.Tasks;

namespace RioPulse.Core.Services;

// Cette classe orchestre l'appel à RaiderIoService et CharacterHistoryService.
public class CharacterOrchestrator
{
    private readonly CharacterHistoryService _characterHistoryService;
    private readonly RaiderIoService _raiderIoService;

    public CharacterOrchestrator(RaiderIoService raiderIoService, CharacterHistoryService characterHistoryService)
    {
        _raiderIoService = raiderIoService;
        _characterHistoryService = characterHistoryService;
    }


    public async Task<ExtendedCharacterSnapshot?> UpdateCharacterAndGuildSnapshotAsync(string region, string realm, string name)
    {
        // Récupération des données du personnage principal
        Character? character = await _raiderIoService.GetCharacterDataAsync(region, realm, name);
        if (character == null)
        {
            Console.WriteLine("Aucune donnée n'a été récupérée pour le personnage.");
            return null;
        }

        // Vérifier si le personnage appartient à une guilde
        if (character.Guild == null)
        {
            Console.WriteLine($"Le personnage {character.Name} n'appartient à aucune guilde.");
            return null;
        }

        // Récupérer les détails complets de la guilde, y compris les membres
        Guild? guildDetails = await _raiderIoService.GetGuildDetailsAsync(
                                                                          character.Region,
                                                                          character.Realm,
                                                                          character.Guild.Name);

        if (guildDetails == null || guildDetails.GuildMembers == null)
        {
            Console.WriteLine($"Impossible de récupérer les détails de la guilde {character.Guild.Name}.");
            return null;
        }

        // Liste pour stocker les données enrichies des membres de la guilde
        List<Character> enrichedGuildMembers = new();

        Task<Character?>[] tasks = guildDetails.GuildMembers.Select(guildMember =>
                                                _raiderIoService.GetCharacterDataAsync(guildMember.Character.Region,
                                                                                        guildMember.Character.Realm,
                                                                                        guildMember.Character.Name)).ToArray();

        Character?[] results = await Task.WhenAll(tasks);

        foreach (Character? members in results)
        {
            if (members != null && character.MythicPlusScoresBySeason[0].Scores["all"] > 0)
            {
                enrichedGuildMembers.Add(members);
                _characterHistoryService.SaveCharacterSnapshot(members);
            }
        }

        // Créer un snapshot étendu contenant le personnage principal et ses guild mates enrichis
        ExtendedCharacterSnapshot snapshot = new()
        {
            Timestamp = DateTime.UtcNow,
            Character = character,
            GuildMembers = enrichedGuildMembers
        };

        // Sauvegarder le snapshot via le service d'historique
        await _characterHistoryService.SaveExtendedSnapshot(snapshot);

        Console.WriteLine("Snapshot étendu enregistré avec succès.");

        return snapshot;
    }


    public async Task<Character> EnrichCharacterWithGuildDetailsAsync(Character character)
    {
        if (character.Guild != null)
        {
            // Récupérer les détails complets de la guilde
            Guild? guildDetails = await _raiderIoService.GetGuildDetailsAsync(
                                                                              character.Region,
                                                                              character.Realm,
                                                                              character.Guild.Name);

            if (guildDetails != null)
            {
                // Remplacer l'objet Guild par les détails complets
                character.Guild = guildDetails;
            }
        }

        return character;
    }
}
