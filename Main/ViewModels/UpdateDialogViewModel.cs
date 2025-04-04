using AutoRenovatioNS;
using AutoRenovatioNS.Models;

namespace Main.ViewModels;

public class UpdateDialogViewModel(AutoRenovatio updater, DefaultUpdate info, string oldVersion) : BindableBase
{
    private string _oldVersion = oldVersion;

    public string OldVersion
    {
        get => _oldVersion;
        set => SetProperty(ref _oldVersion, value);
    }

    private string _version = info.Version.ToString();

    public string Version
    {
        get => _version;
        set => SetProperty(ref _version, value);
    }

    private string? _changelog = info.Changelog;

    public string? Changelog
    {
        get => _changelog;
        set => SetProperty(ref _changelog, value);
    }

    private bool _enableButton = true;

    public bool EnableButton
    {
        get => _enableButton;
        set => SetProperty(ref _enableButton, value);
    }

    private readonly DefaultUpdate _info = info;
    private readonly AutoRenovatio _updater = updater;

    public AsyncDelegateCommand UpdateCommand => new(Update);

    private async Task Update()
    {
        EnableButton = false;
        //await _updater.DownloadUpdateAsync(_info);
    }
}