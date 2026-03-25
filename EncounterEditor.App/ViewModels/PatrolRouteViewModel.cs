using System.Windows;
using System.Windows.Media;
using EncounterEditor.Core.Models;

namespace EncounterEditor.App.ViewModels;

public sealed class PatrolRouteViewModel : SceneItemViewModel
{
    private static readonly SolidColorBrush BaseStroke = new(Color.FromRgb(203, 166, 247));
    private static readonly SolidColorBrush SelectedStroke = new(Color.FromRgb(247, 185, 85));
    private readonly PatrolRoute _model;

    public PatrolRouteViewModel(MainViewModel owner, PatrolRoute model) : base(owner)
    {
        _model = model;
    }

    public PatrolRoute Model => _model;

    public override string Id => _model.Id;

    public override string DisplayName
    {
        get => _model.DisplayName;
        set => Owner.UpdateValue(_model, _model.DisplayName, value, (route, newValue) => route.DisplayName = newValue, "Update patrol route name");
    }

    public double Speed
    {
        get => _model.Speed;
        set => Owner.UpdateValue(_model, _model.Speed, value, (route, newValue) => route.Speed = Math.Max(0.25, newValue), "Update patrol speed");
    }

    public bool Loop
    {
        get => _model.Loop;
        set => Owner.UpdateValue(_model, _model.Loop, value, (route, newValue) => route.Loop = newValue, "Update patrol loop");
    }

    public string PointsSummary => string.Join("  |  ", _model.Points.Select(point => $"({point.X:0}, {point.Y:0})"));

    public PointCollection PolylinePoints => new(_model.Points.Select(point => new Point(point.X, point.Y)));

    public double LabelX => _model.Points.Count > 0 ? _model.Points[0].X + 8 : 0;

    public double LabelY => _model.Points.Count > 0 ? _model.Points[0].Y - 20 : 0;

    public override EncounterObjectType ObjectType => EncounterObjectType.PatrolRoute;

    public override Brush PrimaryBrush => Brushes.Transparent;

    public override Brush SecondaryBrush => IsSelected ? SelectedStroke : BaseStroke;
}
