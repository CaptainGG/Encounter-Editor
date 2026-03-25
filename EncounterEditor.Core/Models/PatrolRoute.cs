namespace EncounterEditor.Core.Models;

public sealed class PatrolRoute : EncounterObjectBase
{
    public override EncounterObjectType ObjectType => EncounterObjectType.PatrolRoute;

    public double Speed { get; set; } = 1.25;

    public bool Loop { get; set; } = true;

    public List<PointD> Points { get; set; } = new()
    {
        new PointD(320, 320),
        new PointD(520, 320),
        new PointD(520, 500)
    };
}
