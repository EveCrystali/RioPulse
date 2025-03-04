using RioPulse.Core.Models;
using RioPulse.Core.Services;
namespace RioPulse.Orchestration;

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


    public async Task UpdateCharacterAndGuildSnapshotAsync(string realm, string name, string region)
    {
        // Récupération des données du personnage principal
        Character? character = await _raiderIoService.GetCharacterDataAsync(region, realm, name);
        if (character == null)
        {
            Console.WriteLine("Aucune donnée n'a été récupérée pour le personnage.");
            return;
        }

        // Vérifier si le personnage appartient à une guilde
        if (character.Guild == null)
        {
            Console.WriteLine($"Le personnage {character.Name} n'appartient à aucune guilde.");
            return;
        }

        // Récupérer les détails complets de la guilde, y compris les membres
        Guild? guildDetails = await _raiderIoService.GetGuildDetailsAsync(
                                                                          character.Region,
                                                                          character.Realm,
                                                                          character.Guild.Name);

        if (guildDetails == null || guildDetails.Members == null)
        {
            Console.WriteLine($"Impossible de récupérer les détails de la guilde {character.Guild.Name}.");
            return;
        }

        // Liste pour stocker les données enrichies des membres de la guilde
        List<Character> enrichedGuildMembers = new();

        // Boucle sur chaque membre de la guilde pour récupérer leurs données détaillées
        foreach (Character guildMember in guildDetails.Members)
        {
            Character? memberData = await _raiderIoService.GetCharacterDataAsync(
                                                                                 guildMember.Realm,
                                                                                 guildMember.Name,
                                                                                 guildMember.Region);

            if (memberData != null)
            {
                enrichedGuildMembers.Add(memberData);
            }
            else
            {
                Console.WriteLine($"Impossible de récupérer les données pour le membre {guildMember.Name}.");
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