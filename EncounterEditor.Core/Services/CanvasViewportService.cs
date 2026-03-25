using EncounterEditor.Core.Models;
using EncounterEditor.Core.Utilities;

namespace EncounterEditor.Core.Services;

public sealed class CanvasViewportService : ICanvasViewportService
{
    private const double MinZoom = 0.35;
    private const double MaxZoom = 2.5;

    public event EventHandler? Changed;

    public double Zoom { get; private set; } = 1;

    public PointD PanOffset { get; private set; } = new(40, 40);

    public PointD ScreenToWorld(PointD screenPoint)
    {
        return new PointD(
            (screenPoint.X - PanOffset.X) / Zoom,
            (screenPoint.Y - PanOffset.Y) / Zoom);
    }

    public PointD WorldToScreen(PointD worldPoint)
    {
        return new PointD(
            worldPoint.X * Zoom + PanOffset.X,
            worldPoint.Y * Zoom + PanOffset.Y);
    }

    public PointD Snap(PointD point, double gridSize) => GeometryHelper.Snap(point, gridSize);

    public void SetPan(PointD panOffset)
    {
        PanOffset = panOffset;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void PanBy(double deltaX, double deltaY)
    {
        PanOffset = new PointD(PanOffset.X + deltaX, PanOffset.Y + deltaY);
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void ZoomAt(double zoomDelta, PointD focusScreenPoint)
    {
        var zoomFactor = zoomDelta > 0 ? 1.1 : 0.9;
        var oldWorldFocus = ScreenToWorld(focusScreenPoint);
        Zoom = Math.Clamp(Zoom * zoomFactor, MinZoom, MaxZoom);
        PanOffset = new PointD(
            focusScreenPoint.X - oldWorldFocus.X * Zoom,
            focusScreenPoint.Y - oldWorldFocus.Y * Zoom);

        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void Reset()
    {
        Zoom = 1;
        PanOffset = new PointD(40, 40);
        Changed?.Invoke(this, EventArgs.Empty);
    }
}
