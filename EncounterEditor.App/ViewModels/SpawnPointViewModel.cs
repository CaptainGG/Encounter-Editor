using System.Windows.Media;
using EncounterEditor.Core.Models;

namespace EncounterEditor.App.ViewModels;

public sealed class SpawnPointViewModel : SceneItemViewModel
{
    private static readonly SolidColorBrush BaseFill = new(Color.FromRgb(96, 165, 250));
    private static readonly SolidColorBrush BaseStroke = new(Color.FromRgb(226, 232, 240));
    private static readonly SolidColorBrush SelectedStroke = new(Color.FromRgb(247, 185, 85));
    private readonly SpawnPoint _model;

    public SpawnPointViewModel(MainViewModel owner, SpawnPoint model) : base(owner)
    {
        _model = model;
    }

    public SpawnPoint Model => _model;

    public override string Id => _model.Id;

    public override string DisplayName
    {
        get => _model.DisplayName;
        set => Owner.UpdateValue(_model, _model.DisplayName, value, (spawn, newValue) => spawn.DisplayName = newValue, "Update spawn display name");
    }

    public string Archetype
    {
        get => _model.Archetype;
        set => Owner.UpdateValue(_model, _model.Archetype, value, (spawn, newValue) => spawn.Archetype = newValue, "Update spawn archetype");
    }

    public int Count
    {
        get => _model.Count;
        set => Owner.UpdateValue(_model, _model.Count, value, (spawn, newValue) => spawn.Count = Math.Max(1, newValue), "Update spawn count");
    }

    public double DelaySeconds
    {
        get => _model.DelaySeconds;
        set => Owner.UpdateValue(_model, _model.DelaySeconds, value, (spawn, newValue) => spawn.DelaySeconds = Math.Max(0, newValue), "Update spawn delay");
    }

    public double FacingDegrees
    {
        get => _model.FacingDegrees;
        set => Owner.UpdateValue(_model, _model.FacingDegrees, value, (spawn, newValue) => spawn.FacingDegrees = newValue, "Update spawn facing");
    }

    public string LinkedZoneId
    {
        get => _model.LinkedZoneId;
        set => Owner.UpdateValue(_model, _model.LinkedZoneId, value, (spawn, newValue) => spawn.LinkedZoneId = newValue, "Update spawn link");
    }

    public double X => _model.Position.X;

    public double Y => _model.Position.Y;

    public double CanvasX => _model.Position.X - 9;

    public double CanvasY => _model.Position.Y - 9;

    public override EncounterObjectType ObjectType => EncounterObjectType.SpawnPoint;

    public override Brush PrimaryBrush => BaseFill;

    public override Brush SecondaryBrush => IsSelected ? SelectedStroke : BaseStroke;

    public string Summary => $"{Archetype} x{Count}";
}

