using EncounterEditor.Core.Models;

namespace EncounterEditor.Core.Services;

public interface ISelectionService
{
    event EventHandler? SelectionChanged;

    SelectionState? Current { get; }

    void SetSelection(string objectId, EncounterObjectType objectType);

    void Clear();
}

