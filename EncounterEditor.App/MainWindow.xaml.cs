using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using EncounterEditor.App.Infrastructure;
using EncounterEditor.App.ViewModels;
using EncounterEditor.Core.Models;
using EncounterEditor.Core.Services;

namespace EncounterEditor.App;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private DragState? _dragState;
    private bool _isPanning;
    private Point _panStartScreenPoint;

    public MainWindow()
    {
        InitializeComponent();

        _viewModel = new MainViewModel(
            new JsonProjectSerializer(),
            new ValidationService(),
            new CommandHistoryService(),
            new CanvasViewportService(),
            new SelectionService(),
            new FileDialogService());

        DataContext = _viewModel;
        _viewModel.PropertyChanged += ViewModelOnPropertyChanged;
        Loaded += (_, _) => ApplySceneTransform();
    }

    private void Zone_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element && element.DataContext is EncounterZoneViewModel zone)
        {
            BeginDrag(zone, element, e);
        }
    }

    private void Spawn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element && element.DataContext is SpawnPointViewModel spawn)
        {
            BeginDrag(spawn, element, e);
        }
    }

    private void Route_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element && element.DataContext is PatrolRouteViewModel route)
        {
            BeginDrag(route, element, e);
        }
    }

    private void SceneBackground_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_dragState is null)
        {
            _viewModel.ClearSelection();
        }
    }

    private void ValidationIssues_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is ListView listView && listView.SelectedItem is ValidationIssue issue)
        {
            _viewModel.SelectIssueCommand.Execute(issue);
        }
    }

    private void Outline_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is ListBox listBox && listBox.SelectedItem is SceneItemViewModel item)
        {
            _viewModel.SelectObject(item);
        }
    }

    private void ViewportHost_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Middle)
        {
            return;
        }

        _isPanning = true;
        _panStartScreenPoint = e.GetPosition(this);
        ViewportHost.CaptureMouse();
        Cursor = Cursors.SizeAll;
    }

    private void ViewportHost_MouseMove(object sender, MouseEventArgs e)
    {
        if (_isPanning)
        {
            var currentPoint = e.GetPosition(this);
            var panDelta = currentPoint - _panStartScreenPoint;
            _panStartScreenPoint = currentPoint;
            _viewModel.PanBy(panDelta.X, panDelta.Y);
            return;
        }

        if (_dragState is null || e.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        var dragPoint = e.GetPosition(SceneSurface);
        var dragDelta = dragPoint - _dragState.StartCanvasPoint;
        var worldDeltaX = dragDelta.X / _viewModel.Zoom;
        var worldDeltaY = dragDelta.Y / _viewModel.Zoom;
        _dragState.Visual.RenderTransform = new TranslateTransform(worldDeltaX, worldDeltaY);
    }

    private void ViewportHost_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (_isPanning && e.ChangedButton == MouseButton.Middle)
        {
            _isPanning = false;
            ViewportHost.ReleaseMouseCapture();
            Cursor = Cursors.Arrow;
            return;
        }

        if (_dragState is null || e.ChangedButton != MouseButton.Left)
        {
            return;
        }

        var current = e.GetPosition(SceneSurface);
        var delta = current - _dragState.StartCanvasPoint;
        var worldDeltaX = delta.X / _viewModel.Zoom;
        var worldDeltaY = delta.Y / _viewModel.Zoom;

        _dragState.Visual.RenderTransform = Transform.Identity;
        _dragState.Visual.ReleaseMouseCapture();
        _viewModel.CommitMove(_dragState.Item, worldDeltaX, worldDeltaY);
        _dragState = null;
    }

    private void ViewportHost_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        _viewModel.ZoomAt(e.Delta, new PointD(e.GetPosition(ViewportHost).X, e.GetPosition(ViewportHost).Y));
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.N)
        {
            _viewModel.NewProjectCommand.Execute(null);
            e.Handled = true;
        }
        else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.O)
        {
            _viewModel.OpenProjectCommand.Execute(null);
            e.Handled = true;
        }
        else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.S)
        {
            _viewModel.SaveProjectCommand.Execute(null);
            e.Handled = true;
        }
        else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Z)
        {
            _viewModel.UndoCommand.Execute(null);
            e.Handled = true;
        }
        else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Y)
        {
            _viewModel.RedoCommand.Execute(null);
            e.Handled = true;
        }
        else if (e.Key == Key.Delete)
        {
            _viewModel.DeleteSelectionCommand.Execute(null);
            e.Handled = true;
        }
    }

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(MainViewModel.Zoom) or nameof(MainViewModel.PanX) or nameof(MainViewModel.PanY))
        {
            ApplySceneTransform();
        }
    }

    private void ApplySceneTransform()
    {
        SceneSurface.RenderTransform = new TransformGroup
        {
            Children =
            {
                new ScaleTransform(_viewModel.Zoom, _viewModel.Zoom),
                new TranslateTransform(_viewModel.PanX, _viewModel.PanY)
            }
        };
    }

    private void BeginDrag(SceneItemViewModel item, FrameworkElement element, MouseButtonEventArgs e)
    {
        _viewModel.SelectObject(item);
        _dragState = new DragState(item, element, e.GetPosition(SceneSurface));
        element.CaptureMouse();
        e.Handled = true;
    }

    private sealed record DragState(SceneItemViewModel Item, FrameworkElement Visual, Point StartCanvasPoint);
}
