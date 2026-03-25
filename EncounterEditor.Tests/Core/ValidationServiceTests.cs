using EncounterEditor.Core.Models;
using EncounterEditor.Core.Services;
using Xunit;

namespace EncounterEditor.Tests.Core;

public sealed class ValidationServiceTests
{
    [Fact]
    public void Validate_CatchesConfiguredRules()
    {
        var project = new EncounterProject
        {
            Zones = new List<EncounterZone>
            {
                new EncounterZone
                {
                    Id = "duplicate",
                    DisplayName = "Overlap A",
                    Name = string.Empty,
                    Faction = EncounterFaction.None,
                    X = 100,
                    Y = 100,
                    Width = 300,
                    Height = 300
                },
                new EncounterZone
                {
                    Id = "duplicate",
                    DisplayName = "Overlap B",
                    Name = "Overlap B",
                    Faction = EncounterFaction.Guard,
                    X = 180,
                    Y = 180,
                    Width = 300,
                    Height = 300
                }
            },
            SpawnPoints = new List<SpawnPoint>
            {
                new SpawnPoint
                {
                    Id = "spawn_001",
                    DisplayName = "Loose Spawn",
                    Archetype = string.Empty,
                    Position = new PointD(900, 900)
                }
            },
            PatrolRoutes = new List<PatrolRoute>
            {
                new PatrolRoute
                {
                    Id = "route_001",
                    DisplayName = string.Empty,
                    Points = new List<PointD> { new PointD(32, 32) }
                }
            }
        };

        var issues = new ValidationService().Validate(project);

        Assert.Contains(issues, issue => issue.Message.Contains("Duplicate id", StringComparison.Ordinal));
        Assert.Contains(issues, issue => issue.Message.Contains("missing a name", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(issues, issue => issue.Message.Contains("faction is not assigned", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(issues, issue => issue.Message.Contains("outside every zone", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(issues, issue => issue.Message.Contains("no spawn points", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(issues, issue => issue.Message.Contains("overlaps", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(issues, issue => issue.Message.Contains("needs at least two points", StringComparison.OrdinalIgnoreCase));
    }
}
