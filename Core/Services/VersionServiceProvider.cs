using YamlDotNet.Core.Tokens;

namespace Core.Services;

public static class VersionServiceProvider
{
    private static IVersion? _version;
    private static readonly Lock _lock = new();

    public static IVersion Version
    {
        get
        {
            if (_version == null)
            {
                throw new NoServiceException("No hay servicio de versionado definido!");
            }
            return _version;
        }
    }

    public static void Set(IVersion version)
    {
        if (version.GetType().Name == SettingsService.Instance.Version)
        {
            lock (_lock)
            {
                _version = version;
            }
        }
    }

    public static void SetView(string versionName, string region, Type viewType, IRegionManager regionManager)
    {
        if (versionName == SettingsService.Instance.Version)
        {
            regionManager.RegisterViewWithRegion(region, viewType);
        }
    }
}