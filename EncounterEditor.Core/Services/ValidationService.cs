using EncounterEditor.Core.Models;
using EncounterEditor.Core.Utilities;

namespace EncounterEditor.Core.Services;

public sealed class ValidationService : IValidationService
{
    private const double OverlapThreshold = 0.15;

    public IReadOnlyList<ValidationIssue> Validate(EncounterProject project)
    {
        var issues = new List<ValidationIssue>();
        ValidateDuplicateIds(project, issues);
        ValidateZones(project, issues);
        ValidateSpawns(project, issues);
        ValidatePatrolRoutes(project, issues);
        return issues;
    }

    private static void ValidateDuplicateIds(EncounterProject project, ICollection<ValidationIssue> issues)
    {
        var duplicateGroups = project.EnumerateObjects()
            .GroupBy(item => item.Id, StringComparer.OrdinalIgnoreCase)
            .Where(group => !string.IsNullOrWhiteSpace(group.Key) && group.Count() > 1);

        foreach (var group in duplicateGroups)
        {
            foreach (var item in group)
            {
                issues.Add(new ValidationIssue
                {
                    Severity = ValidationSeverity.Error,
                    Message = $"Duplicate id '{group.Key}' detected.",
                    ObjectId = item.Id,
                    ObjectType = item.ObjectType,
                    SuggestedFix = "Regenerate or rename the object's id."
                });
            }
        }
    }

    private static void ValidateZones(EncounterProject project, ICollection<ValidationIssue> issues)
    {
        foreach (var zone in project.Zones)
        {
            if (string.IsNullOrWhiteSpace(zone.Name))
            {
                issues.Add(CreateIssue(zone, ValidationSeverity.Error, "Zone is missing a name.", "Provide a clear authored name for the zone."));
            }

            if (zone.Faction == EncounterFaction.None)
            {
                issues.Add(CreateIssue(zone, ValidationSeverity.Warning, "Zone faction is not assigned.", "Pick a faction so encounter ownership is explicit."));
            }

            var spawnsInZone = project.SpawnPoints.Count(spawn =>
                (!string.IsNullOrWhiteSpace(spawn.LinkedZoneId) && string.Equals(spawn.LinkedZoneId, zone.Id, StringComparison.OrdinalIgnoreCase)) ||
                GeometryHelper.IsPointInsideZone(spawn.Position, zone));

            if (spawnsInZone == 0)
            {
                issues.Add(CreateIssue(zone, ValidationSeverity.Warning, "Zone has no spawn points.", "Add at least one spawn point inside or linked to this zone."));
            }
        }

        for (var i = 0; i < project.Zones.Count; i++)
        {
            for (var j = i + 1; j < project.Zones.Count; j++)
            {
                var overlapRatio = GeometryHelper.CalculateZoneOverlapRatio(project.Zones[i], project.Zones[j]);
                if (overlapRatio <= OverlapThreshold)
                {
                    continue;
                }

                issues.Add(CreateIssue(
                    project.Zones[i],
                    ValidationSeverity.Warning,
                    $"Zone overlaps '{project.Zones[j].DisplayName}' by {overlapRatio:P0}.",
                    "Reduce the overlap or split the play space into cleaner authored areas."));
            }
        }
    }

    private static void ValidateSpawns(EncounterProject project, ICollection<ValidationIssue> issues)
    {
        foreach (var spawn in project.SpawnPoints)
        {
            if (string.IsNullOrWhiteSpace(spawn.Archetype))
            {
                issues.Add(CreateIssue(spawn, ValidationSeverity.Error, "Spawn point is missing an archetype.", "Assign the enemy or encounter archetype."));
            }

            var insideAnyZone = project.Zones.Any(zone => GeometryHelper.IsPointInsideZone(spawn.Position, zone));
            if (!insideAnyZone)
            {
                issues.Add(CreateIssue(spawn, ValidationSeverity.Warning, "Spawn point is outside every zone.", "Move it inside a zone or author a matching encounter zone."));
            }
        }
    }

    private static void ValidatePatrolRoutes(EncounterProject project, ICollection<ValidationIssue> issues)
    {
        foreach (var route in project.PatrolRoutes)
        {
            if (route.Points.Count < 2)
            {
                issues.Add(CreateIssue(route, ValidationSeverity.Error, "Patrol route needs at least two points.", "Add more points or remove the route."));
            }

            if (string.IsNullOrWhiteSpace(route.DisplayName))
            {
                issues.Add(CreateIssue(route, ValidationSeverity.Warning, "Patrol route is missing a display name.", "Name the route so designers can identify it quickly."));
            }
        }
    }

    private static ValidationIssue CreateIssue(EncounterObjectBase item, ValidationSeverity severity, string message, string fix)
    {
        return new ValidationIssue
        {
            Severity = severity,
            Message = message,
            ObjectId = item.Id,
            ObjectType = item.ObjectType,
            SuggestedFix = fix
        };
    }
}

