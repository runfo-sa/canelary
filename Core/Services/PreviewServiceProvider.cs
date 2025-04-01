namespace Core.Services;

public static class PreviewServiceProvider
{
    private static Func<string, IPreview>? _callPreview;

    /// <summary>
    /// Devuelve una instancia del servicio <see cref="IPreview"/> <br/>
    /// </summary>
    /// <exception cref="NullReferenceException"></exception>
    public static IPreview ProvideService(string content)
    {
        return (_callPreview != null) ? _callPreview.Invoke(content) : throw new NoServiceException("No hay ningun preview engine definido!");
    }

    public static void Set(Func<string, IPreview> func)
    {
        _callPreview = func;
    }
}