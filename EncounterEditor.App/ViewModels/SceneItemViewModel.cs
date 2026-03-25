using System.Windows.Media;
using EncounterEditor.App.Infrastructure;
using EncounterEditor.Core.Models;

namespace EncounterEditor.App.ViewModels;

public abstract class SceneItemViewModel : ViewModelBase
{
    private bool _isSelected;

    protected SceneItemViewModel(MainViewModel owner)
    {
        Owner = owner;
    }

    protected MainViewModel Owner { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    public abstract string Id { get; }

    public abstract string DisplayName { get; set; }

    public abstract EncounterObjectType ObjectType { get; }

    public abstract Brush PrimaryBrush { get; }

    public abstract Brush SecondaryBrush { get; }
}

