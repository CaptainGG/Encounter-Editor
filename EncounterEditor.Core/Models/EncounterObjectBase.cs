namespace EncounterEditor.Core.Models;

public abstract class EncounterObjectBase
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    public string DisplayName { get; set; } = string.Empty;

    public abstract EncounterObjectType ObjectType { get; }
}

