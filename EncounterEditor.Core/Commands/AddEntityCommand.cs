namespace EncounterEditor.Core.Commands;

public sealed class AddEntityCommand<T> : IUndoableCommand
{
    private readonly IList<T> _collection;
    private readonly T _entity;

    public AddEntityCommand(IList<T> collection, T entity, string description)
    {
        _collection = collection;
        _entity = entity;
        Description = description;
    }

    public string Description { get; }

    public void Execute() => _collection.Add(_entity);

    public void Undo() => _collection.Remove(_entity);
}

