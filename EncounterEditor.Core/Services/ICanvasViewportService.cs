using EncounterEditor.Core.Models;

namespace EncounterEditor.Core.Services;

public interface ICanvasViewportService
{
    event EventHandler? Changed;

    double Zoom { get; }

    PointD PanOffset { get; }

    PointD ScreenToWorld(PointD screenPoint);

    PointD WorldToScreen(PointD worldPoint);

    PointD Snap(PointD point, double gridSize);

    void SetPan(PointD panOffset);

    void PanBy(double deltaX, double deltaY);

    void ZoomAt(double zoomDelta, PointD focusScreenPoint);

    void Reset();
}

