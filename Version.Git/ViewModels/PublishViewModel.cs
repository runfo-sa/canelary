using Core.Events;
using Core.Services;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Data;
using VersionGit.Models;

namespace VersionGit.ViewModels
{
    public class PublishViewModel : BindableBase
    {
        public ListCollectionView Versions { get; set; }
        public ObservableCollection<string> Files { get; set; }
        public DelegateCommand SelectionChangedCommand { get; private set; }
        public DelegateCommand OpenFolderCommand { get; private set; }
        public DelegateCommand PushCommand { get; private set; }

        private Visibility _enablePush;

        public Visibility EnablePush
        {
            get => _enablePush;
            set => SetProperty(ref _enablePush, value);
        }

        private string _folder = string.Empty;
        private readonly IDialogService _dialogService;

        public string FolderPath
        {
            get => _folder;
            set => SetProperty(ref _folder, value);
        }

        public PublishViewModel(IEventAggregator eventAggregator, IDialogService dialogService)
        {
            _dialogService = dialogService;
            var files = VersionServiceProvider.Version.ListFiles();
            var versions = VersionServiceProvider.Version.ListVersions();
            Versions = new(versions.ToList());
            Files = new(files.Select(f => f.Name).ToList());
            EnablePush = (((string)Versions.CurrentItem) == "Local" && GitInner.EnableGit) ? Visibility.Visible : Visibility.Hidden;

            SelectionChangedCommand = new(() =>
            {
                var files = VersionServiceProvider.Version.ListFiles((string)Versions.CurrentItem);
                Files.Clear();
                foreach (var f in files)
                {
                    Files.Add(f.Name);
                }
                EnablePush = (((string)Versions.CurrentItem) == "Local" && GitInner.EnableGit) ? Visibility.Visible : Visibility.Hidden;
            });

            OpenFolderCommand = new(() =>
            {
                OpenFolderDialog dialog = new();

                if (dialog.ShowDialog() is true)
                {
                    FolderPath = dialog.FolderName;
                    ((Git)VersionServiceProvider.Version).FolderPath = FolderPath;
                }
            });

            PushCommand = new(Push);

            eventAggregator
                .GetEvent<PublishEvent>()
                .Subscribe(async () => { await Pull(); });
        }

        private async Task Pull()
        {
            var version = (string)Versions.CurrentItem;
            if (version == "Local")
            {
                foreach (string filename in Directory.EnumerateFiles(Settings.Instance.EtiquetasDir))
                {
                    using FileStream src = File.Open(filename, FileMode.Open);
                    using FileStream dest = File.Create(Path.Combine(FolderPath, Path.GetFileName(filename)));
                    await src.CopyToAsync(dest);
                }
            }
            else
            {
                GitInner.RunGitCommand("clone", $"{Settings.Instance.GitRepo} \"{FolderPath}\"", FolderPath);
            }
        }

        private void Push()
        {
            _dialogService.Show("PushView");
        }
    }
}