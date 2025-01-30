using System.Text.Json.Serialization;
namespace RioPulse.Models;

public class Gear
{
    [JsonPropertyName("item_level_equipped")]
    public string ItemLevelEquipped { get; set; }
    [JsonPropertyName("item_level_total")]
    public string ItemLevelTotal { get; set; }
    [JsonPropertyName("artifact_traits")]
    public string ArtifactTraits { get; set; }

    public string Head { get; set; }
    public string Neck { get; set; }
    public string Shoulders { get; set; }
    public string Back { get; set; }
    public string Chest { get; set; }
    public string Shirt { get; set; }
    public string Tabard { get; set; }
    public string Wrist { get; set; }
    public string Hands { get; set; }
    public string Waist { get; set; }
    public string Legs { get; set; }
    public string Feet { get; set; }
    public string Finger1 { get; set; }
    public string Finger2 { get; set; }
    public string Trinket1 { get; set; }
    public string Trinket2 { get; set; }
    public string MainHand { get; set; }
    public string OffHand { get; set; }
}