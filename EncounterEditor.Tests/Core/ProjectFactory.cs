using EncounterEditor.Core.Models;

namespace EncounterEditor.Tests.Core;

internal static class ProjectFactory
{
    public static EncounterProject Create()
    {
        return new EncounterProject
        {
            ProjectName = "Factory Project",
            Map = new MapSettings
            {
                Width = 1200,
                Height = 800,
                GridSize = 40
            },
            Zones = new List<EncounterZone>
            {
                new EncounterZone
                {
                    Id = "zone_001",
                    DisplayName = "South Gate",
                    Name = "South Gate",
                    Difficulty = EncounterDifficulty.Medium,
                    Faction = EncounterFaction.Guard,
                    RecommendedPlayerLevel = 6,
                    X = 80,
                    Y = 80,
                    Width = 320,
                    Height = 240
                }
            },
            SpawnPoints = new List<SpawnPoint>
            {
                new SpawnPoint
                {
                    Id = "spawn_001",
                    DisplayName = "Outer Pair",
                    Archetype = "Rifle Guard",
                    Count = 2,
                    DelaySeconds = 1,
                    FacingDegrees = 180,
                    LinkedZoneId = "zone_001",
                    Position = new PointD(200, 180)
                }
            },
            PatrolRoutes = new List<PatrolRoute>
            {
                new PatrolRoute
                {
                    Id = "route_001",
                    DisplayName = "Gate Sweep",
                    Speed = 1.5,
                    Loop = true,
                    Points = new List<PointD>
                    {
                        new PointD(120, 120),
                        new PointD(280, 120),
                        new PointD(280, 240)
                    }
                }
            }
        };
    }
}
