namespace Core.Services
{
    public static class VersionServiceProvider
    {
        public static IVersion Version { get; private set; } = null!;

        public static void Set(IVersion version)
        {
            if (version.GetType().Name == SettingsService.Instance.VersionSystem)
            {
                Version = version;
            }

            if (Version == null)
            {
                throw new NoServiceException("No hay servicio de versionado definido!");
            }
        }
    }
}
