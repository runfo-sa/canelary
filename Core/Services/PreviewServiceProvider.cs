using Core.Services.SettingsModel;

namespace Core.Services
{
    public static class PreviewServiceProvider
    {
        /// <summary>
        /// Devuelve una instancia del servicio <see cref="IPreviewService"/> <br/>
        /// La elección es en base al enum <see cref="PreviewEngine"/> para el parametro <see cref="SettingsService.PreviewEngine"/>
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public static IPreviewService ProvideService(string content)
        {
            return SettingsService.Instance.PreviewEngine switch
            {
                PreviewEngine.Labelary => new LabelaryService(content),
                _ => throw new NotImplementedException(),
            };
        }
    }
}
