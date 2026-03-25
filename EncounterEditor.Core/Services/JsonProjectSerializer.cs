using System.Text.Json;
using EncounterEditor.Core.Models;

namespace EncounterEditor.Core.Services;

public sealed class JsonProjectSerializer : IProjectSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public EncounterProject Load(string filePath)
    {
        var json = File.ReadAllText(filePath);
        return JsonSerializer.Deserialize<EncounterProject>(json, SerializerOptions)
               ?? new EncounterProject();
    }

    public void Save(string filePath, EncounterProject project)
    {
        var json = JsonSerializer.Serialize(project, SerializerOptions);
        File.WriteAllText(filePath, json);
    }
}

