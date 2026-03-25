using System.IO;
using Microsoft.Win32;

namespace EncounterEditor.App.Infrastructure;

public interface IFileDialogService
{
    string? OpenProject();

    string? SaveProject(string? currentFilePath);
}

public sealed class FileDialogService : IFileDialogService
{
    private const string Filter = "Encounter files (*.encounter.json)|*.encounter.json|JSON files (*.json)|*.json";

    public string? OpenProject()
    {
        var dialog = new OpenFileDialog
        {
            Filter = Filter,
            CheckFileExists = true,
            Title = "Open Encounter Project"
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public string? SaveProject(string? currentFilePath)
    {
        var dialog = new SaveFileDialog
        {
            Filter = Filter,
            FileName = string.IsNullOrWhiteSpace(currentFilePath) ? "new-encounter.encounter.json" : Path.GetFileName(currentFilePath),
            InitialDirectory = string.IsNullOrWhiteSpace(currentFilePath) ? null : Path.GetDirectoryName(currentFilePath),
            Title = "Save Encounter Project"
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
