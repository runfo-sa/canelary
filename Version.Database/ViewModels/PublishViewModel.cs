using Core.Events;
using Core.Services;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using Version.Database.Models;
using VersionDatabase.Db;

namespace VersionDatabase.ViewModels
{
    public class PublishViewModel : BindableBase
    {
        public ObservableCollection<LabelVersions> Labels { get; set; }
        public DelegateCommand OpenFolderCommand { get; private set; }

        private string? _folder;

        public string? FolderPath
        {
            get => _folder;
            set => SetProperty(ref _folder, value);
        }

        public PublishViewModel(IEventAggregator eventAggregator)
        {
            using var context = new DatabaseDbContext();
            var labels = context.DefinicionEtiquetas.Select(e => new LabelVersions(e));
            Labels = [.. labels];

            OpenFolderCommand = new(() =>
            {
                OpenFolderDialog dialog = new();

                if (dialog.ShowDialog() is true)
                {
                    FolderPath = dialog.FolderName;
                }
            });

            eventAggregator
                .GetEvent<PublishEvent>()
                .Subscribe(async () => await Publish());
        }

        private async Task Publish()
        {
            using var context = new DatabaseDbContext();
            var toUpdate = Labels
                .Where(l => l.SelectedVersion != l.CurrentVersion)
                .Select(l =>
                {
                    l.Etiqueta.Version = l.SelectedVersion;
                    return l.Etiqueta;
                });

            context.DefinicionEtiquetas.UpdateRange(toUpdate);
            await context.SaveChangesAsync();

            var labels = context.DefinicionEtiquetas.Select(e => new LabelVersions(e));
            Labels.Clear();
            foreach (var item in labels)
            {
                Labels.Add(item);
            }

            if (!FolderPath.IsNullOrEmpty())
            {
                await Compile(FolderPath!);
            }
        }

        private static async Task Compile(string folder)
        {
            var files = (IEnumerable<VirtualFile>)VersionServiceProvider.Version.ListFiles();
            foreach (var file in files)
            {
                var content = file.Read();
                var filename = Path.Combine(folder, file.Name);
                await File.WriteAllTextAsync(filename, content);
            }
        }
    }
}