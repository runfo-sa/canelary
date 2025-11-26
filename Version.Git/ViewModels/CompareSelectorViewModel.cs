using System.Collections.ObjectModel;

using Core.Events;
using Core.FileTree;
using Core.Services;
using Core.Services.VersionModel;

namespace VersionGit.ViewModels;

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

    public DelegateCommand<bool?> VersionChanged { get; private set; }

    public CompareSelectorViewModel(IEventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator;
        _eventAggregator
         .GetEvent<SendFilesEvent>()
         .Subscribe(SendFiles);

        var files = VersionServiceProvider.Version.ListFiles();
        LeftFiles = [.. files];
        RightFiles = [.. files];

        var versions = VersionServiceProvider.Version.ListVersions();
        LeftVersion = [.. versions];
        RightVersion = [.. versions];

        VersionChanged = new(ChangeFile);
    }

    private void ChangeFile(bool? isLeftFile)
    {
        if (isLeftFile is true)
        {
            var files = VersionServiceProvider.Version.ListFiles(LeftVer!);
            LeftFiles.Clear();
            foreach (var file in files)
            {
                LeftFiles.Add(file);
            }
        }
        else
        {
            var files = VersionServiceProvider.Version.ListFiles(RightVer!);
            RightFiles.Clear();
            foreach (var file in files)
            {
                RightFiles.Add(file);
            }
        }
    }

    private void SendFiles(string id)
    {
        _eventAggregator.GetEvent<RecvFilesEvent>().Publish(new ComparasionFiles(LeftFile!, RightFile!, id));
    }
}