using System.Text.Json.Serialization;

namespace RioPulse.Models;

public class Segments
{
    [JsonPropertyName("all")]
    public Segment All { get; set; }
    [JsonPropertyName("dps")]
    public Segment Dps { get; set; }
    [JsonPropertyName("healer")]
    public Segment Healer { get; set; }
    [JsonPropertyName("tank")]
    public Segment Tank { get; set; }
    [JsonPropertyName("spec_0")]
    public Segment Spec0 { get; set; }
    [JsonPropertyName("spec_1")]
    public Segment Spec1 { get; set; }
    [JsonPropertyName("spec_2")]
    public Segment Spec2 { get; set; }
    [JsonPropertyName("spec_3")]
    public Segment Spec3 { get; set; }
}