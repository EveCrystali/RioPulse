using System.Text.Json.Serialization;
namespace RioPulse.Models;

public class Affix
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("description")]
    public string Description { get; set; }
    [JsonPropertyName("icon")]
    public string Icon { get; set; }
    [JsonPropertyName("icon_url")]
    public string IconUrl { get; set; }
    [JsonPropertyName("wowhead_url")]
    public string WowheadUrl { get; set; }
}