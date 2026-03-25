namespace EncounterEditor.Core.Models;

public sealed class MapSettings
{
    public double Width { get; set; } = 2400;

    public double Height { get; set; } = 1400;

    public double GridSize { get; set; } = 40;

    public string BackgroundImagePath { get; set; } = string.Empty;
}

