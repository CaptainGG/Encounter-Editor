using EncounterEditor.Core.Models;

namespace EncounterEditor.Core.Services;

public sealed class SelectionState
{
    public string ObjectId { get; init; } = string.Empty;

    public EncounterObjectType ObjectType { get; init; }
}

