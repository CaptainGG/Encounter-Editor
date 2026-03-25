namespace EncounterEditor.Core.Models;

public sealed class EncounterZone : EncounterObjectBase
{
    public override EncounterObjectType ObjectType => EncounterObjectType.Zone;

    public string Name { get; set; } = "New Zone";

    public EncounterDifficulty Difficulty { get; set; } = EncounterDifficulty.Medium;

    public EncounterFaction Faction { get; set; } = EncounterFaction.Guard;

    public int RecommendedPlayerLevel { get; set; } = 8;

    public double X { get; set; } = 120;

    public double Y { get; set; } = 120;

    public double Width { get; set; } = 320;

    public double Height { get; set; } = 200;
}

