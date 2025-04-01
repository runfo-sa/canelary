using System.ComponentModel;
using System.Text;

using AvalonEditB.Document;

using Core.Services;

using Microsoft.Win32;

namespace Editor.Models;

/// <summary>
/// Clase que modela el contendio de una pestaña en el editor de texto.
/// Contiene toda la informacion necesaria para renderizar un editor de texto.
/// </summary>
public class TabItem : BindableBase
{
    public delegate void TabItemHandler(TabItem item);

    public event TabItemHandler? WasModified;

    public TextDocument Content { get; private set; }

    private string? _path;

    public string? Path
    {
        get => _path;
        set => SetProperty(ref _path, value);
    }

    private string _header = string.Empty;

    public string Header
    {
        get => _header;
        private set => SetProperty(ref _header, value);
    }

    private bool _hasUnsavedChanges = false;

    public bool HasUnsavedChanges
    {
        get => _hasUnsavedChanges;
        private set => SetProperty(ref _hasUnsavedChanges, value);
    }

    private List<LintingInfo> _lintingData = [];

    public List<LintingInfo> LintingData
    {
        get => _lintingData;
        set => SetProperty(ref _lintingData, value);
    }

    public TabItem(string header, string content, string? path = null)
    {
        Path = path;
        Header = header;
        Content = new TextDocument(content);
        Content.TextChanged += SetUnsavedChanges;
        Content.UndoStack.PropertyChanged += ResetChanges;
    }

    public void SetAsUnsaved()
    {
        SetUnsavedChanges(null, EventArgs.Empty);
    }

    private void SetUnsavedChanges(object? sender, EventArgs e)
    {
        Header += '*';
        HasUnsavedChanges = true;
        Content.TextChanged -= SetUnsavedChanges;
        WasModified?.Invoke(this);
    }

    private void ResetChanges(object? sender, PropertyChangedEventArgs e)
    {
        if (Content.UndoStack.IsOriginalFile && HasUnsavedChanges)
        {
            HasUnsavedChanges = false;
            Header = Header[..(Header.Length - 1)];
            Content.TextChanged += SetUnsavedChanges;
            WasModified?.Invoke(this);
        }
    }

    public bool SaveItem()
    {
        if (HasUnsavedChanges)
        {
            if (Path is not null)
            {
                if (!VersionServiceProvider.Version.SaveFile(Path, Content.Text))
                {
                    return false;
                }
                Header = Header[..(Header.Length - 1)];
            }
            else
            {
                var filters = new StringBuilder();

                foreach (var ext in SettingsService.Instance.Extension)
                {
                    filters.Append($"ZPL File (*.{ext})|*.{ext}|");
                }
                filters.Append("Todos los archivos (*.*)|*.*");

                SaveFileDialog dialog = new()
                {
                    Filter = filters.ToString()
                };

                if (dialog.ShowDialog() == false)
                {
                    return false;
                }

                if (!VersionServiceProvider.Version.SaveFile(dialog.FileName, Content.Text))
                {
                    return false;
                }
                Path = dialog.FileName;
                Header = dialog.SafeFileName;
            }

            HasUnsavedChanges = false;
            Content.TextChanged += SetUnsavedChanges;
            Content.UndoStack.MarkAsOriginalFile();
        }

        return true;
    }

    public void ClearLinting()
    {
        LintingData = [];
    }
}