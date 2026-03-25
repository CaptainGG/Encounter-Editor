namespace EncounterEditor.Core.Models;

public sealed class SpawnPoint : EncounterObjectBase
{
    public override EncounterObjectType ObjectType => EncounterObjectType.SpawnPoint;

    public string Archetype { get; set; } = "Rifle Guard";

    public int Count { get; set; } = 3;

    public double DelaySeconds { get; set; } = 0;

    public double FacingDegrees { get; set; } = 180;

    public string LinkedZoneId { get; set; } = string.Empty;

    public PointD Position { get; set; } = new(260, 240);
}

