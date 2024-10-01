using Core.Services;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Forms;
using VersionGit.Models;

namespace VersionGit.ViewModels
{
    public class PublishViewModel : BindableBase
    {
        public ListCollectionView Versions { get; set; }
        public ObservableCollection<string> Files { get; set; }
        public DelegateCommand SelectionChangedCommand { get; private set; }
        public DelegateCommand OpenFolderCommand { get; private set; }

        private string _folder = string.Empty;
        public string FolderPath
        {
            get => _folder;
            set => SetProperty(ref _folder, value);
        }

        public PublishViewModel()
        {
            var (files, versions) = VersionServiceProvider.Version.FetchFileVer();
            Versions = new(versions.ToList());
            Files = new(files.Select(f => f.Name).ToList());

            SelectionChangedCommand = new(() =>
            {
                var files = Git.LoadGitFiles((string)Versions.CurrentItem);
                Files.Clear();
                foreach (var f in files)
                {
                    Files.Add(f.Name);
                }
            });

            OpenFolderCommand = new(() =>
            {
                FolderBrowserDialog dialog = new();

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    FolderPath = dialog.SelectedPath;
                    ((Git)VersionServiceProvider.Version).FolderPath = FolderPath;
                }
            });
        }
    }
}
