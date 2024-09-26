namespace Core.Services
{
    public static class VersionServiceProvider
    {
        private static IVersion? _version;
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
            private set => _version = value;
        }

        public static void Set(IVersion version)
        {
            if (version.GetType().Name == SettingsService.Instance.VersionSystem)
            {
                Version = version;
            }
        }

        public static void SetView(string versionName, Type viewType, IRegionManager regionManager)
        {
            if (versionName == SettingsService.Instance.VersionSystem)
            {
                regionManager.RegisterViewWithRegion("Main#VersionRegion", viewType);
            }
        }
    }
}
