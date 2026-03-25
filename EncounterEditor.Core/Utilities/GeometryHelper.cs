using EncounterEditor.Core.Models;

namespace EncounterEditor.Core.Utilities;

public static class GeometryHelper
{
    public static bool IsPointInsideZone(PointD point, EncounterZone zone)
    {
        return point.X >= zone.X &&
               point.X <= zone.X + zone.Width &&
               point.Y >= zone.Y &&
               point.Y <= zone.Y + zone.Height;
    }

    public static double CalculateZoneOverlapRatio(EncounterZone first, EncounterZone second)
    {
        var overlapWidth = Math.Max(0, Math.Min(first.X + first.Width, second.X + second.Width) - Math.Max(first.X, second.X));
        var overlapHeight = Math.Max(0, Math.Min(first.Y + first.Height, second.Y + second.Height) - Math.Max(first.Y, second.Y));

        if (overlapWidth <= 0 || overlapHeight <= 0)
        {
            return 0;
        }

        var overlapArea = overlapWidth * overlapHeight;
        var smallerZoneArea = Math.Min(first.Width * first.Height, second.Width * second.Height);
        return smallerZoneArea <= 0 ? 0 : overlapArea / smallerZoneArea;
    }

    public static PointD Snap(PointD point, double gridSize)
    {
        if (gridSize <= 0)
        {
            return point;
        }

        return new PointD(
            Math.Round(point.X / gridSize) * gridSize,
            Math.Round(point.Y / gridSize) * gridSize);
    }
}

