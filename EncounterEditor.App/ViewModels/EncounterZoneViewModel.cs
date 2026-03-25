using System.Windows.Media;
using EncounterEditor.Core.Models;

namespace EncounterEditor.App.ViewModels;

public sealed class EncounterZoneViewModel : SceneItemViewModel
{
    private static readonly SolidColorBrush BaseFill = new(Color.FromArgb(70, 73, 199, 177));
    private static readonly SolidColorBrush BaseStroke = new(Color.FromRgb(73, 199, 177));
    private static readonly SolidColorBrush SelectedStroke = new(Color.FromRgb(247, 185, 85));
    private readonly EncounterZone _model;

    public EncounterZoneViewModel(MainViewModel owner, EncounterZone model) : base(owner)
    {
        _model = model;
    }

    public EncounterZone Model => _model;

    public override string Id => _model.Id;

    public override string DisplayName
    {
        get => _model.DisplayName;
        set => Owner.UpdateValue(_model, _model.DisplayName, value, (zone, newValue) => zone.DisplayName = newValue, "Update zone display name");
    }

    public string Name
    {
        get => _model.Name;
        set => Owner.UpdateValue(_model, _model.Name, value, (zone, newValue) => zone.Name = newValue, "Update zone name");
    }

    public EncounterDifficulty Difficulty
    {
        get => _model.Difficulty;
        set => Owner.UpdateValue(_model, _model.Difficulty, value, (zone, newValue) => zone.Difficulty = newValue, "Update zone difficulty");
    }

    public EncounterFaction Faction
    {
        get => _model.Faction;
        set => Owner.UpdateValue(_model, _model.Faction, value, (zone, newValue) => zone.Faction = newValue, "Update zone faction");
    }

    public int RecommendedPlayerLevel
    {
        get => _model.RecommendedPlayerLevel;
        set => Owner.UpdateValue(_model, _model.RecommendedPlayerLevel, value, (zone, newValue) => zone.RecommendedPlayerLevel = Math.Max(1, newValue), "Update zone level");
    }

    public double X
    {
        get => _model.X;
        set => Owner.UpdateValue(_model, _model.X, value, (zone, newValue) => zone.X = newValue, "Move zone");
    }

    public double Y
    {
        get => _model.Y;
        set => Owner.UpdateValue(_model, _model.Y, value, (zone, newValue) => zone.Y = newValue, "Move zone");
    }

    public double Width
    {
        get => _model.Width;
        set => Owner.UpdateValue(_model, _model.Width, value, (zone, newValue) => zone.Width = Math.Max(40, newValue), "Resize zone");
    }

    public double Height
    {
        get => _model.Height;
        set => Owner.UpdateValue(_model, _model.Height, value, (zone, newValue) => zone.Height = Math.Max(40, newValue), "Resize zone");
    }

    public override EncounterObjectType ObjectType => EncounterObjectType.Zone;

    public EncounterDifficulty[] DifficultyOptions { get; } = Enum.GetValues<EncounterDifficulty>();

    public EncounterFaction[] FactionOptions { get; } = Enum.GetValues<EncounterFaction>();

    public override Brush PrimaryBrush => BaseFill;

    public override Brush SecondaryBrush => IsSelected ? SelectedStroke : BaseStroke;

    public string Summary => $"{Difficulty} {Faction}  LVL {RecommendedPlayerLevel}";
}

