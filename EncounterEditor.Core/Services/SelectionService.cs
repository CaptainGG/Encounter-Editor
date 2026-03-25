using EncounterEditor.Core.Models;

namespace EncounterEditor.Core.Services;

public sealed class SelectionService : ISelectionService
{
    public event EventHandler? SelectionChanged;

    public SelectionState? Current { get; private set; }

    public void SetSelection(string objectId, EncounterObjectType objectType)
    {
        Current = new SelectionState
        {
            ObjectId = objectId,
            ObjectType = objectType
        };

        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Clear()
    {
        Current = null;
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }
}

