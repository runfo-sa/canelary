namespace Core.Services;

public static class BackendServiceProvider
{
    private static IBackend? _backend;

    public static IBackend Backend
    {
        get
        {
            if (_backend == null)
            {
                throw new NoServiceException("No hay servicio de backend definido!");
            }
            return _backend;
        }
        private set => _backend = value;
    }

    public static void Set(IBackend backend)
    {
        if (backend.GetType().Name == SettingsService.Instance.Backend)
        {
            Backend = backend;
        }
    }
}