namespace EncounterEditor.Core.Commands;

public sealed class RemoveEntityCommand<T> : IUndoableCommand
{
    private readonly IList<T> _collection;
    private readonly T _entity;
    private int _index = -1;

    public RemoveEntityCommand(IList<T> collection, T entity, string description)
    {
        _collection = collection;
        _entity = entity;
        Description = description;
    }

    public string Description { get; }

    public void Execute()
    {
        _index = _collection.IndexOf(_entity);
        if (_index >= 0)
        {
            _collection.RemoveAt(_index);
        }
    }

    public void Undo()
    {
        if (_index < 0 || _index > _collection.Count)
        {
            _collection.Add(_entity);
            return;
        }

        _collection.Insert(_index, _entity);
    }
}

