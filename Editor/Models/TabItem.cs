using Core.Services;
using ICSharpCode.AvalonEdit.Document;
using Microsoft.Win32;
using System.ComponentModel;
using System.IO;

namespace Editor.Models
{
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
                    File.WriteAllText(Path, Content.Text);
                    Header = Header[..(Header.Length - 1)];
                }
                else
                {
                    string extension = SettingsService.Instance.EtiquetasExtension;
                    SaveFileDialog dialog = new()
                    {
                        Filter = $"ZPL File (*.{extension})|*.{extension}|Todos los archivos (*.*)|*.*"
                    };

                    if (dialog.ShowDialog() == false)
                    {
                        return false;
                    }

                    File.WriteAllText(dialog.FileName, Content.Text);
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
}
