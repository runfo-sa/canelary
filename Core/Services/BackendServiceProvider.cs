namespace Core.Services;

public static class BackendServiceProvider
{
    private static IBackend? _backend;
    private static readonly Lock _lock = new();

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
    }

    public static void Set(IBackend backend)
    {
        if (backend.GetType().Name == SettingsService.Instance.Backend)
        {
            lock (_lock)
            {
                _backend = backend;
            }
        }
    }
}