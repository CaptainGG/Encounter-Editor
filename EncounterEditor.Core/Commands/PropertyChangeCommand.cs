namespace EncounterEditor.Core.Commands;

public sealed class PropertyChangeCommand<TObject, TValue> : IUndoableCommand
{
    private readonly TObject _target;
    private readonly TValue _oldValue;
    private readonly TValue _newValue;
    private readonly Action<TObject, TValue> _applyValue;

    public PropertyChangeCommand(
        TObject target,
        TValue oldValue,
        TValue newValue,
        Action<TObject, TValue> applyValue,
        string description)
    {
        _target = target;
        _oldValue = oldValue;
        _newValue = newValue;
        _applyValue = applyValue;
        Description = description;
    }

    public string Description { get; }

    public void Execute() => _applyValue(_target, _newValue);

    public void Undo() => _applyValue(_target, _oldValue);
}

