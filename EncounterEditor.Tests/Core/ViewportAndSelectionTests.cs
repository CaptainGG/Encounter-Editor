using EncounterEditor.Core.Models;
using EncounterEditor.Core.Services;
using Xunit;

namespace EncounterEditor.Tests.Core;

public sealed class ViewportAndSelectionTests
{
    [Fact]
    public void ViewportTransformsAndSnappingStayPredictable()
    {
        var viewport = new CanvasViewportService();

        viewport.PanBy(60, 30);
        viewport.ZoomAt(120, new PointD(300, 200));

        var world = viewport.ScreenToWorld(new PointD(420, 260));
        var screen = viewport.WorldToScreen(world);
        var snapped = viewport.Snap(new PointD(113, 187), 40);

        Assert.Equal(420, screen.X, 4);
        Assert.Equal(260, screen.Y, 4);
        Assert.Equal(new PointD(120, 200), snapped);
    }

    [Fact]
    public void SelectionCanBeClearedAfterDeleteWorkflow()
    {
        var selection = new SelectionService();

        selection.SetSelection("spawn_002", EncounterObjectType.SpawnPoint);
        Assert.NotNull(selection.Current);

        selection.Clear();

        Assert.Null(selection.Current);
    }
}
