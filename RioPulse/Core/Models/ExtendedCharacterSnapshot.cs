namespace RioPulse.Core.Models;

public class ExtendedCharacterSnapshot
{
    public DateTime Timestamp { get; set; }
    public Character Character { get; set; }
    public List<Character> GuildMembers { get; set; }
}