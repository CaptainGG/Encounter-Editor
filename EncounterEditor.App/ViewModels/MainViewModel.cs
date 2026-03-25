using System.IO;
using System.Collections.ObjectModel;
using EncounterEditor.App.Infrastructure;
using EncounterEditor.Core.Commands;
using EncounterEditor.Core.Models;
using EncounterEditor.Core.Services;

namespace EncounterEditor.App.ViewModels;

public sealed class MainViewModel : ViewModelBase
{
    private readonly IProjectSerializer _serializer;
    private readonly IValidationService _validationService;
    private readonly ICommandHistoryService _commandHistory;
    private readonly ICanvasViewportService _viewportService;
    private readonly ISelectionService _selectionService;
    private readonly IFileDialogService _fileDialogService;

    private EncounterProject _project = new();
    private object? _selectedObject;
    private string? _currentFilePath;
    private string _statusText = "Create a zone, drop a few spawns, and validate the encounter as you go.";
    private bool _hasUnsavedChanges;

    public MainViewModel(
        IProjectSerializer serializer,
        IValidationService validationService,
        ICommandHistoryService commandHistory,
        ICanvasViewportService viewportService,
        ISelectionService selectionService,
        IFileDialogService fileDialogService)
    {
        _serializer = serializer;
        _validationService = validationService;
        _commandHistory = commandHistory;
        _viewportService = viewportService;
        _selectionService = selectionService;
        _fileDialogService = fileDialogService;

        NewProjectCommand = new RelayCommand(CreateNewProject);
        OpenProjectCommand = new RelayCommand(OpenProject);
        SaveProjectCommand = new RelayCommand(SaveProject);
        SaveProjectAsCommand = new RelayCommand(SaveProjectAs);
        AddZoneCommand = new RelayCommand(AddZone);
        AddSpawnCommand = new RelayCommand(AddSpawnPoint, () => Project.Zones.Count > 0);
        AddPatrolRouteCommand = new RelayCommand(AddPatrolRoute, () => Project.Zones.Count > 0);
        DeleteSelectionCommand = new RelayCommand(DeleteSelection, () => SelectedObject is not null);
        UndoCommand = new RelayCommand(Undo, () => _commandHistory.CanUndo);
        RedoCommand = new RelayCommand(Redo, () => _commandHistory.CanRedo);
        ResetViewportCommand = new RelayCommand(() =>
        {
            _viewportService.Reset();
            NotifyViewportChanged();
            StatusText = "Viewport reset.";
        });
        SelectIssueCommand = new RelayCommand<ValidationIssue>(SelectIssue);

        _commandHistory.Changed += (_, _) =>
        {
            OnPropertyChanged(nameof(CanUndo));
            OnPropertyChanged(nameof(CanRedo));
            OnPropertyChanged(nameof(UndoDescription));
            OnPropertyChanged(nameof(RedoDescription));
            RaiseCommandStates();
        };

        _selectionService.SelectionChanged += (_, _) => RefreshSelection();
        _viewportService.Changed += (_, _) => NotifyViewportChanged();

        CreateNewProject();
    }

    public ObservableCollection<EncounterZoneViewModel> Zones { get; } = new();

    public ObservableCollection<SpawnPointViewModel> SpawnPoints { get; } = new();

    public ObservableCollection<PatrolRouteViewModel> PatrolRoutes { get; } = new();

    public ObservableCollection<ValidationIssue> ValidationIssues { get; } = new();

    public RelayCommand NewProjectCommand { get; }

    public RelayCommand OpenProjectCommand { get; }

    public RelayCommand SaveProjectCommand { get; }

    public RelayCommand SaveProjectAsCommand { get; }

    public RelayCommand AddZoneCommand { get; }

    public RelayCommand AddSpawnCommand { get; }

    public RelayCommand AddPatrolRouteCommand { get; }

    public RelayCommand DeleteSelectionCommand { get; }

    public RelayCommand UndoCommand { get; }

    public RelayCommand RedoCommand { get; }

    public RelayCommand ResetViewportCommand { get; }

    public RelayCommand<ValidationIssue> SelectIssueCommand { get; }

    public EncounterProject Project => _project;

    public object? SelectedObject
    {
        get => _selectedObject;
        private set
        {
            if (SetProperty(ref _selectedObject, value))
            {
                RaiseCommandStates();
            }
        }
    }

    public string ProjectName
    {
        get => _project.ProjectName;
        set => UpdateValue(_project, _project.ProjectName, value, (project, newValue) => project.ProjectName = newValue, "Update project name");
    }

    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    public bool HasUnsavedChanges
    {
        get => _hasUnsavedChanges;
        private set
        {
            if (SetProperty(ref _hasUnsavedChanges, value))
            {
                OnPropertyChanged(nameof(Title));
            }
        }
    }

    public string Title
    {
        get
        {
            var fileSegment = string.IsNullOrWhiteSpace(_currentFilePath) ? "unsaved.encounter.json" : Path.GetFileName(_currentFilePath);
            var dirtyMarker = HasUnsavedChanges ? " *" : string.Empty;
            return $"Encounter Editor  |  {Project.ProjectName}  |  {fileSegment}{dirtyMarker}";
        }
    }

    public double MapWidth => _project.Map.Width;

    public double MapHeight => _project.Map.Height;

    public double GridSize => _project.Map.GridSize;

    public double Zoom => _viewportService.Zoom;

    public double PanX => _viewportService.PanOffset.X;

    public double PanY => _viewportService.PanOffset.Y;

    public string ZoomLabel => $"{Zoom * 100:0}%";

    public bool CanUndo => _commandHistory.CanUndo;

    public bool CanRedo => _commandHistory.CanRedo;

    public string UndoDescription => _commandHistory.UndoDescription;

    public string RedoDescription => _commandHistory.RedoDescription;

    public string ValidationSummary => ValidationIssues.Count == 0
        ? "No validation issues."
        : $"{ValidationIssues.Count} validation issue(s)";

    public void SelectObject(SceneItemViewModel item)
    {
        _selectionService.SetSelection(item.Id, item.ObjectType);
        StatusText = $"Selected {item.DisplayName}.";
    }

    public void ClearSelection()
    {
        _selectionService.Clear();
        StatusText = "Selection cleared.";
    }

    public void PanBy(double deltaX, double deltaY) => _viewportService.PanBy(deltaX, deltaY);

    public void ZoomAt(double delta, PointD focusScreenPoint) => _viewportService.ZoomAt(delta, focusScreenPoint);

    public void CommitMove(SceneItemViewModel item, double deltaX, double deltaY)
    {
        if (Math.Abs(deltaX) < double.Epsilon && Math.Abs(deltaY) < double.Epsilon)
        {
            return;
        }

        switch (item)
        {
            case EncounterZoneViewModel zone:
                var newZonePosition = _viewportService.Snap(new PointD(zone.Model.X + deltaX, zone.Model.Y + deltaY), GridSize);
                ExecuteCommand(
                    new PropertyChangeCommand<EncounterZone, PointD>(
                        zone.Model,
                        new PointD(zone.Model.X, zone.Model.Y),
                        newZonePosition,
                        (target, value) =>
                        {
                            target.X = value.X;
                            target.Y = value.Y;
                        },
                        "Move zone"),
                    "Zone moved.",
                    selectItem: zone);
                break;

            case SpawnPointViewModel spawn:
                var newSpawnPosition = _viewportService.Snap(new PointD(spawn.Model.Position.X + deltaX, spawn.Model.Position.Y + deltaY), GridSize);
                ExecuteCommand(
                    new PropertyChangeCommand<SpawnPoint, PointD>(
                        spawn.Model,
                        spawn.Model.Position,
                        newSpawnPosition,
                        (target, value) => target.Position = value,
                        "Move spawn point"),
                    "Spawn point moved.",
                    selectItem: spawn);
                break;

            case PatrolRouteViewModel route:
                var newPoints = route.Model.Points
                    .Select(point => _viewportService.Snap(new PointD(point.X + deltaX, point.Y + deltaY), GridSize))
                    .ToList();
                ExecuteCommand(
                    new PropertyChangeCommand<PatrolRoute, List<PointD>>(
                        route.Model,
                        route.Model.Points.ToList(),
                        newPoints,
                        (target, value) => target.Points = value,
                        "Move patrol route"),
                    "Patrol route moved.",
                    selectItem: route);
                break;
        }
    }

    public void UpdateValue<TObject, TValue>(
        TObject target,
        TValue oldValue,
        TValue newValue,
        Action<TObject, TValue> applyValue,
        string description)
    {
        if (EqualityComparer<TValue>.Default.Equals(oldValue, newValue))
        {
            return;
        }

        ExecuteCommand(
            new PropertyChangeCommand<TObject, TValue>(target, oldValue, newValue, applyValue, description),
            $"{description}.",
            reselectCurrent: true);
    }

    private void CreateNewProject()
    {
        _project = new EncounterProject
        {
            ProjectName = "New Encounter",
            Map = new MapSettings
            {
                Width = 2400,
                Height = 1400,
                GridSize = 40
            }
        };

        _currentFilePath = null;
        _selectionService.Clear();
        _commandHistory.Clear();
        _viewportService.Reset();
        HasUnsavedChanges = false;
        RefreshAll("Created a new encounter project.");
    }

    private void OpenProject()
    {
        var filePath = _fileDialogService.OpenProject();
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return;
        }

        _project = _serializer.Load(filePath);
        _currentFilePath = filePath;
        _selectionService.Clear();
        _commandHistory.Clear();
        _viewportService.Reset();
        HasUnsavedChanges = false;
        RefreshAll($"Loaded {Path.GetFileName(filePath)}.");
    }

    private void SaveProject()
    {
        if (string.IsNullOrWhiteSpace(_currentFilePath))
        {
            SaveProjectAs();
            return;
        }

        _serializer.Save(_currentFilePath, _project);
        HasUnsavedChanges = false;
        StatusText = $"Saved {Path.GetFileName(_currentFilePath)}.";
        OnPropertyChanged(nameof(Title));
    }

    private void SaveProjectAs()
    {
        var filePath = _fileDialogService.SaveProject(_currentFilePath);
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return;
        }

        _currentFilePath = filePath;
        SaveProject();
    }

    private void AddZone()
    {
        var zone = new EncounterZone
        {
            Id = CreateId("zone"),
            DisplayName = $"Zone {Project.Zones.Count + 1}",
            Name = $"Zone {Project.Zones.Count + 1}",
            X = 160 + Project.Zones.Count * 60,
            Y = 160 + Project.Zones.Count * 40
        };

        ExecuteCommand(
            new AddEntityCommand<EncounterZone>(_project.Zones, zone, "Add zone"),
            $"Added {zone.DisplayName}.",
            selectId: zone.Id,
            selectType: zone.ObjectType);
    }

    private void AddSpawnPoint()
    {
        var selectedZone = SelectedObject as EncounterZoneViewModel;
        var anchorX = selectedZone?.Model.X + (selectedZone?.Model.Width ?? 0) / 2 ?? 320;
        var anchorY = selectedZone?.Model.Y + (selectedZone?.Model.Height ?? 0) / 2 ?? 240;
        var spawn = new SpawnPoint
        {
            Id = CreateId("spawn"),
            DisplayName = $"Spawn {Project.SpawnPoints.Count + 1}",
            LinkedZoneId = selectedZone?.Id ?? string.Empty,
            Position = _viewportService.Snap(new PointD(anchorX, anchorY), GridSize)
        };

        ExecuteCommand(
            new AddEntityCommand<SpawnPoint>(_project.SpawnPoints, spawn, "Add spawn point"),
            $"Added {spawn.DisplayName}.",
            selectId: spawn.Id,
            selectType: spawn.ObjectType);
    }

    private void AddPatrolRoute()
    {
        var selectedZone = SelectedObject as EncounterZoneViewModel;
        var originX = selectedZone?.Model.X + 40 ?? 260;
        var originY = selectedZone?.Model.Y + 40 ?? 260;
        var route = new PatrolRoute
        {
            Id = CreateId("route"),
            DisplayName = $"Patrol {Project.PatrolRoutes.Count + 1}",
            Points = new List<PointD>
            {
                _viewportService.Snap(new PointD(originX, originY), GridSize),
                _viewportService.Snap(new PointD(originX + 160, originY), GridSize),
                _viewportService.Snap(new PointD(originX + 160, originY + 160), GridSize)
            }
        };

        ExecuteCommand(
            new AddEntityCommand<PatrolRoute>(_project.PatrolRoutes, route, "Add patrol route"),
            $"Added {route.DisplayName}.",
            selectId: route.Id,
            selectType: route.ObjectType);
    }

    private void DeleteSelection()
    {
        switch (SelectedObject)
        {
            case EncounterZoneViewModel zone:
                ExecuteCommand(new RemoveEntityCommand<EncounterZone>(_project.Zones, zone.Model, "Delete zone"), $"Deleted {zone.DisplayName}.", clearSelection: true);
                break;
            case SpawnPointViewModel spawn:
                ExecuteCommand(new RemoveEntityCommand<SpawnPoint>(_project.SpawnPoints, spawn.Model, "Delete spawn point"), $"Deleted {spawn.DisplayName}.", clearSelection: true);
                break;
            case PatrolRouteViewModel route:
                ExecuteCommand(new RemoveEntityCommand<PatrolRoute>(_project.PatrolRoutes, route.Model, "Delete patrol route"), $"Deleted {route.DisplayName}.", clearSelection: true);
                break;
        }
    }

    private void Undo()
    {
        _commandHistory.Undo();
        HasUnsavedChanges = true;
        RefreshAll("Undo complete.");
    }

    private void Redo()
    {
        _commandHistory.Redo();
        HasUnsavedChanges = true;
        RefreshAll("Redo complete.");
    }

    private void SelectIssue(ValidationIssue? issue)
    {
        if (issue is null)
        {
            return;
        }

        _selectionService.SetSelection(issue.ObjectId, issue.ObjectType);
        StatusText = issue.Message;
    }

    private void ExecuteCommand(
        IUndoableCommand command,
        string status,
        bool clearSelection = false,
        bool reselectCurrent = false,
        string? selectId = null,
        EncounterObjectType? selectType = null,
        SceneItemViewModel? selectItem = null)
    {
        var selectedId = selectId ?? selectItem?.Id;
        var selectedType = selectType ?? selectItem?.ObjectType;

        _commandHistory.Execute(command);
        HasUnsavedChanges = true;

        if (clearSelection)
        {
            _selectionService.Clear();
        }
        else if (!string.IsNullOrWhiteSpace(selectedId) && selectedType.HasValue)
        {
            _selectionService.SetSelection(selectedId, selectedType.Value);
        }
        else if (reselectCurrent && _selectionService.Current is not null)
        {
            _selectionService.SetSelection(_selectionService.Current.ObjectId, _selectionService.Current.ObjectType);
        }

        RefreshAll(status);
    }

    private void RefreshAll(string? statusOverride = null)
    {
        BuildSceneCollections();
        RefreshSelection();
        Revalidate();
        RaiseCommandStates();
        NotifyViewportChanged();
        OnPropertyChanged(nameof(ProjectName));
        OnPropertyChanged(nameof(MapWidth));
        OnPropertyChanged(nameof(MapHeight));
        OnPropertyChanged(nameof(GridSize));
        OnPropertyChanged(nameof(Title));

        if (!string.IsNullOrWhiteSpace(statusOverride))
        {
            StatusText = statusOverride;
        }
    }

    private void BuildSceneCollections()
    {
        RebuildCollection(Zones, _project.Zones, zone => new EncounterZoneViewModel(this, zone));
        RebuildCollection(SpawnPoints, _project.SpawnPoints, spawn => new SpawnPointViewModel(this, spawn));
        RebuildCollection(PatrolRoutes, _project.PatrolRoutes, route => new PatrolRouteViewModel(this, route));
    }

    private void RefreshSelection()
    {
        var current = _selectionService.Current;

        foreach (var zone in Zones)
        {
            zone.IsSelected = current?.ObjectId == zone.Id;
        }

        foreach (var spawn in SpawnPoints)
        {
            spawn.IsSelected = current?.ObjectId == spawn.Id;
        }

        foreach (var route in PatrolRoutes)
        {
            route.IsSelected = current?.ObjectId == route.Id;
        }

        SelectedObject = current is null
            ? null
            : Zones.Cast<SceneItemViewModel>()
                .Concat(SpawnPoints)
                .Concat(PatrolRoutes)
                .FirstOrDefault(item => item.Id == current.ObjectId);
    }

    private void Revalidate()
    {
        ValidationIssues.Clear();
        foreach (var issue in _validationService.Validate(_project))
        {
            ValidationIssues.Add(issue);
        }

        OnPropertyChanged(nameof(ValidationSummary));
    }

    private void NotifyViewportChanged()
    {
        OnPropertyChanged(nameof(Zoom));
        OnPropertyChanged(nameof(PanX));
        OnPropertyChanged(nameof(PanY));
        OnPropertyChanged(nameof(ZoomLabel));
    }

    private void RaiseCommandStates()
    {
        NewProjectCommand.NotifyCanExecuteChanged();
        OpenProjectCommand.NotifyCanExecuteChanged();
        SaveProjectCommand.NotifyCanExecuteChanged();
        SaveProjectAsCommand.NotifyCanExecuteChanged();
        AddZoneCommand.NotifyCanExecuteChanged();
        AddSpawnCommand.NotifyCanExecuteChanged();
        AddPatrolRouteCommand.NotifyCanExecuteChanged();
        DeleteSelectionCommand.NotifyCanExecuteChanged();
        UndoCommand.NotifyCanExecuteChanged();
        RedoCommand.NotifyCanExecuteChanged();
        ResetViewportCommand.NotifyCanExecuteChanged();
    }

    private string CreateId(string prefix)
    {
        var index = 1;
        var existingIds = _project.EnumerateObjects().Select(item => item.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);

        while (existingIds.Contains($"{prefix}_{index:000}"))
        {
            index++;
        }

        return $"{prefix}_{index:000}";
    }

    private static void RebuildCollection<TModel, TViewModel>(
        ObservableCollection<TViewModel> target,
        IEnumerable<TModel> source,
        Func<TModel, TViewModel> factory)
    {
        target.Clear();
        foreach (var item in source)
        {
            target.Add(factory(item));
        }
    }
}
