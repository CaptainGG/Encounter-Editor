using EncounterEditor.Core.Models;
using EncounterEditor.Core.Services;
using Xunit;

namespace EncounterEditor.Tests.Core;

public sealed class JsonProjectSerializerTests
{
    [Fact]
    public void RoundTripPreservesAuthoredData()
    {
        var serializer = new JsonProjectSerializer();
        var project = ProjectFactory.Create();
        var filePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.encounter.json");

        try
        {
            serializer.Save(filePath, project);
            var loaded = serializer.Load(filePath);

            Assert.Equal(project.ProjectName, loaded.ProjectName);
            Assert.Equal(project.Map.GridSize, loaded.Map.GridSize);
            Assert.Single(loaded.Zones);
            Assert.Single(loaded.SpawnPoints);
            Assert.Single(loaded.PatrolRoutes);
            Assert.Equal("South Gate", loaded.Zones[0].Name);
            Assert.Equal("Outer Pair", loaded.SpawnPoints[0].DisplayName);
            Assert.Equal(3, loaded.PatrolRoutes[0].Points.Count);
        }
        finally
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}
