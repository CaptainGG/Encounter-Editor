namespace EncounterEditor.Core.Models;

public sealed class ValidationIssue
{
    public ValidationSeverity Severity { get; set; } = ValidationSeverity.Warning;

    public string Message { get; set; } = string.Empty;

    public string ObjectId { get; set; } = string.Empty;

    public EncounterObjectType ObjectType { get; set; }

    public string SuggestedFix { get; set; } = string.Empty;
}

