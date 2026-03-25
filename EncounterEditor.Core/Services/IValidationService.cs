using EncounterEditor.Core.Models;

namespace EncounterEditor.Core.Services;

public interface IValidationService
{
    IReadOnlyList<ValidationIssue> Validate(EncounterProject project);
}

