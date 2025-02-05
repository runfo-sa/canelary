using Core.Services;

namespace Publish
{
    [Module(ModuleName = "Publicar", OnDemand = true)]
    public class PublishModule : IModule
    {
        private IContainerProvider? _container;

        public void OnInitialized(IContainerProvider containerProvider)
        {
            _container = containerProvider;
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterInstance(new ModuleLoaderService("Publicar", CreateWindow));
        }

        private void CreateWindow(string name)
        {
            /*new AdonisWindow
            {
                Title = $"Canelary - Publicar",
                Content = _container?.Resolve<Views.Publish>(),
                Height = 768,
                Width = 1024,
            }.Show();*/
        }
    }
}