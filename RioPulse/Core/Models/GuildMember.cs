using System.Text.Json.Serialization;

namespace RioPulse.Core.Models;

public class GuildMember
{
    [JsonPropertyName("rank")]
    public int Rank { get; set; }

    [JsonPropertyName("character")]
    public Character Character { get; set; } // Note: Not a Character?
}