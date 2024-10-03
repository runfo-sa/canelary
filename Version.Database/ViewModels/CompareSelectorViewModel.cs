using Core.Events;
using Core.FileTree;
using Core.Services;
using Core.Services.VersionModel;
using System.Collections.ObjectModel;
using Version.Database.Models;
using VersionDatabase.Db;

namespace VersionDatabase.ViewModels
{
    public class CompareSelectorViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;

        public ObservableCollection<IFile> LeftFiles { get; set; }
        public ObservableCollection<IFile> RightFiles { get; set; }
        public ObservableCollection<string> LeftVersion { get; set; }
        public ObservableCollection<string> RightVersion { get; set; }

        public IFile? LeftFile { get; set; } = null!;
        public IFile? RightFile { get; set; } = null!;
        public string? LeftVer { get; set; } = null!;
        public string? RightVer { get; set; } = null!;

        public DelegateCommand<bool?> FileChanged { get; private set; }
        public DelegateCommand<bool?> VersionChanged { get; private set; }

        public CompareSelectorViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator
             .GetEvent<SendFilesEvent>()
             .Subscribe(SendFiles);

            var files = VersionServiceProvider.Version.ListFiles();
            LeftFiles = new(files);
            RightFiles = new(files);

            var versions = VersionServiceProvider.Version.ListVersions();
            LeftVersion = new(versions);
            RightVersion = new(versions);

            FileChanged = new(ChangeVersion);
            VersionChanged = new(ChangeFile);
        }

        private void ChangeFile(bool? isLeftFile)
        {
            using var context = new DatabaseDbContext();

            if (isLeftFile is true)
            {
                var leftFile = (VirtualFile?)LeftFile;
                if (int.TryParse(LeftVer, out var leftVer) && leftFile is not null)
                {
                    LeftFile = context.Etiquetas
                        .Where(e => e.IdEtiqueta == leftFile.Id && e.Version == leftVer)
                        .Select(n => new VirtualFile(n.Nombre, n.IdEtiqueta) { Version = leftVer })
                        .First();
                }
            }
            else
            {
                var rightFile = (VirtualFile?)RightFile;
                if (int.TryParse(RightVer, out var rightVer) && rightFile is not null)
                {
                    RightFile = context.Etiquetas
                        .Where(e => e.IdEtiqueta == rightFile.Id && e.Version == rightVer)
                        .Select(n => new VirtualFile(n.Nombre, n.IdEtiqueta) { Version = rightVer })
                        .First();
                }
            }
        }

        private void ChangeVersion(bool? isLeftVersion)
        {
            if (isLeftVersion is true)
            {
                var versions = VersionServiceProvider.Version.ListVersions(LeftFile);
                LeftVersion.Clear();
                foreach (var item in versions)
                {
                    LeftVersion.Add(item);
                }
            }
            else
            {
                var versions = VersionServiceProvider.Version.ListVersions(RightFile);
                RightVersion.Clear();
                foreach (var item in versions)
                {
                    RightVersion.Add(item);
                }
            }
        }

        private void SendFiles()
        {
            _eventAggregator.GetEvent<RecvFilesEvent>().Publish(new ComparasionFiles(LeftFile!, RightFile!));
        }
    }
}
