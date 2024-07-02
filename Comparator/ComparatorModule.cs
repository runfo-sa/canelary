using AdonisUI.Controls;
using Comparator.Views;
using Core.Services;

namespace Comparator
{
    [Module(ModuleName = "Comparador#Compare", OnDemand = true)]
    public class ComparatorModule : IModule
    {
        private IContainerProvider? _container;

        public void OnInitialized(IContainerProvider containerProvider)
        {
            _container = containerProvider;
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterInstance(new ModuleLoaderService("Comparador", CreateWindow));
            containerRegistry.RegisterDialog<SelectLabelsDialog>();
        }

        private void CreateWindow(string name)
        {
            var view = _container?.Resolve<Views.Comparator>();
            var win = new AdonisWindow
            {
                Title = $"Visual Ternera - Comparador",
                Content = view
            };

            if (!view!.ShouldClose)
            {
                win.Show();
            }
        }
    }
}
