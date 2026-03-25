using EncounterEditor.Core.Commands;

namespace EncounterEditor.Core.Services;

public interface ICommandHistoryService
{
    event EventHandler? Changed;

    bool CanUndo { get; }

    bool CanRedo { get; }

    string UndoDescription { get; }

    string RedoDescription { get; }

    void Execute(IUndoableCommand command);

    void Undo();

    void Redo();

    void Clear();
}

