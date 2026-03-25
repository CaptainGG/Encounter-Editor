namespace EncounterEditor.Core.Models;

public sealed class EncounterProject
{
    public int Version { get; set; } = 1;

    public string ProjectName { get; set; } = "Forest Ambush";

    public MapSettings Map { get; set; } = new();

    public List<EncounterZone> Zones { get; set; } = new();

    public List<SpawnPoint> SpawnPoints { get; set; } = new();

    public List<PatrolRoute> PatrolRoutes { get; set; } = new();

    public IEnumerable<EncounterObjectBase> EnumerateObjects()
    {
        foreach (var zone in Zones)
        {
            yield return zone;
        }

        foreach (var spawn in SpawnPoints)
        {
            yield return spawn;
        }

        foreach (var route in PatrolRoutes)
        {
            yield return route;
        }
    }
}
