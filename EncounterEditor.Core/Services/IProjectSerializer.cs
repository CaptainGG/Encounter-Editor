using EncounterEditor.Core.Models;

namespace EncounterEditor.Core.Services;

public interface IProjectSerializer
{
    EncounterProject Load(string filePath);

    void Save(string filePath, EncounterProject project);
}

