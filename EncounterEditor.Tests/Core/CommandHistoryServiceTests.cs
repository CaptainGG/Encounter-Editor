using EncounterEditor.Core.Commands;
using EncounterEditor.Core.Models;
using EncounterEditor.Core.Services;
using Xunit;

namespace EncounterEditor.Tests.Core;

public sealed class CommandHistoryServiceTests
{
    [Fact]
    public void UndoRedo_RestoresAddMoveDeleteAndPropertyChanges()
    {
        var history = new CommandHistoryService();
        var project = ProjectFactory.Create();
        var spawn = new SpawnPoint
        {
            Id = "spawn_002",
            DisplayName = "Bridge Ambush",
            Archetype = "Heavy Guard",
            LinkedZoneId = "zone_001",
            Position = new PointD(240, 220)
        };

        history.Execute(new AddEntityCommand<SpawnPoint>(project.SpawnPoints, spawn, "Add spawn"));
        history.Execute(new PropertyChangeCommand<EncounterZone, string>(
            project.Zones[0],
            project.Zones[0].Name,
            "South Gate Retuned",
            (zone, value) => zone.Name = value,
            "Rename zone"));
        history.Execute(new PropertyChangeCommand<SpawnPoint, PointD>(
            spawn,
            spawn.Position,
            new PointD(320, 320),
            (target, value) => target.Position = value,
            "Move spawn"));
        history.Execute(new RemoveEntityCommand<SpawnPoint>(project.SpawnPoints, spawn, "Delete spawn"));

        Assert.Single(project.SpawnPoints);
        Assert.Equal("South Gate Retuned", project.Zones[0].Name);

        history.Undo();
        history.Undo();
        history.Undo();
        history.Undo();

        Assert.Single(project.SpawnPoints);
        Assert.Equal("South Gate", project.Zones[0].Name);
        Assert.Equal(new PointD(200, 180), project.SpawnPoints[0].Position);

        history.Redo();
        history.Redo();
        history.Redo();
        history.Redo();

        Assert.Single(project.SpawnPoints);
        Assert.Equal("South Gate Retuned", project.Zones[0].Name);
        Assert.DoesNotContain(project.SpawnPoints, item => item.Id == "spawn_002");
    }
}
