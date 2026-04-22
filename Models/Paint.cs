using System.Text.Json.Serialization;

public enum PaintCategory
{
    Base,
    Layer,
    Contrast,
    Dry,
    Technical,
    Shade
}

public class Paint
{
    public string Name { get; set; } = "";

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PaintCategory Category { get; set; }

    public int RackNumber { get; set; }
    public int Row { get; set; }
    public int Column { get; set; }
    public bool IsOwned { get; set; }

    [JsonIgnore]
    public string Location => $"Rack {RackNumber}  Row {Row,2}  Col {Column}";

    [JsonIgnore]
    public string CategoryCssColor => Category switch
    {
        PaintCategory.Base      => "#e74c3c",
        PaintCategory.Layer     => "#5dade2",
        PaintCategory.Contrast  => "#a569bd",
        PaintCategory.Dry       => "#f4d03f",
        PaintCategory.Technical => "#48c9b0",
        PaintCategory.Shade     => "#52be80",
        _                       => "#aaaaaa"
    };

    [JsonIgnore]
    public string CategoryTextColor => Category switch
    {
        PaintCategory.Dry => "#1a1a1a",
        _                 => "#ffffff"
    };
}
