namespace Core.Services
{
    public static class BackendServiceProvider
    {
        public static IBackend Backend { get; private set; } = null!;

        public static void Set(IBackend backend)
        {
            if (backend.GetType().Name == SettingsService.Instance.Backend)
            {
                Backend = backend;
            }

            if (Backend == null)
            {
                throw new NullReferenceException("No hay backend definido!");
            }
        }
    }
}
