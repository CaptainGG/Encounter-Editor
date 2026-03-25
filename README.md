# Encounter Editor

`Encounter Editor` is a small WPF desktop tool built to demonstrate editor-facing, UX-sensitive tools programming. The project focuses on helping a level designer place and tune encounter content inside a fast authoring loop with validation, undo/redo, and clean JSON persistence.

## Why This Fits The Role

- It is a desktop editor, not a generic business CRUD app.
- The workflow is built around content creators: create, inspect, move, validate, and save without leaving the editor.
- The architecture separates domain logic from UI so validation, serialization, command history, and viewport behavior are testable.
- The interaction design prioritizes responsive feedback and usability over feature breadth.

## Solution Layout

- `EncounterEditor.App`: WPF shell, canvas interactions, inspector, outline, and validation panel.
- `EncounterEditor.Core`: domain models, serializer, validation rules, viewport math, selection, and undoable commands.
- `EncounterEditor.Tests`: automated coverage for serialization, validation, command history, and viewport behavior.

## Architecture Snapshot

```text
MainWindow
  -> MainViewModel
     -> CommandHistoryService
     -> ValidationService
     -> JsonProjectSerializer
     -> CanvasViewportService
     -> SelectionService
     -> EncounterProject
```

## Features

- Single-screen editor layout with:
  - asset palette and project outline
  - top-down grid canvas
  - property inspector
  - validation list and status panel
- Supported authored objects:
  - encounter zones
  - spawn points
  - patrol routes
- Interaction support:
  - selection
  - drag repositioning with grid snapping
  - mouse-wheel zoom
  - middle-mouse panning
  - keyboard shortcuts for save, undo/redo, delete, new, and open
- Validation rules:
  - duplicate ids
  - missing zone name
  - missing faction
  - zone with no spawn points
  - spawn outside any zone
  - overlapping zones
  - patrol route with fewer than two points

## Demo Script

1. Launch the editor and open [`Samples/forest-ambush.encounter.json`](/C:/Users/hkakroo/Documents/Tools/Samples/forest-ambush.encounter.json).
2. Select `Bridge Crossing` from the outline and add a new spawn point.
3. Drag the new spawn outside the zone and show the validation warning appear.
4. Drag it back onto the grid inside the zone and confirm the warning clears.
5. Rename the zone or patrol route in the inspector.
6. Save the project, then use undo/redo to show authored changes moving cleanly through history.

## Running

From the workspace root:

```powershell
dotnet restore EncounterEditor.App\EncounterEditor.App.csproj
dotnet build EncounterEditor.App\EncounterEditor.App.csproj --no-restore
dotnet test EncounterEditor.Tests\EncounterEditor.Tests.csproj
dotnet run --project EncounterEditor.App\EncounterEditor.App.csproj --no-build
```

## Notes

- The local environment used for implementation only had .NET 7 installed, so the project targets `net7.0` / `net7.0-windows`.
- `CommunityToolkit.Mvvm` was intentionally not added because the environment had no preconfigured package restore path at scaffold time; the project uses a lightweight in-repo MVVM base instead.
- The code favors clarity and interview readability over maximum abstraction.
