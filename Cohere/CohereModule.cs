using AdonisUI.Controls;
using Cohere.Views;
using Core.Services;

namespace Cohere
{
    [Module(ModuleName = "Validacion#True#NotebookCheck#3", OnDemand = true)]
    public class CohereModule(IRegionManager regionManager) : IModule
    {
        private readonly IRegionManager _regionManager = regionManager;
        private IContainerProvider? _container;

        public void OnInitialized(IContainerProvider containerProvider)
        {
            _container = containerProvider;
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterInstance(new ModuleLoaderService("Validacion", CreateWindow));
            containerRegistry.RegisterDialog<CreateRuleDialog>();
            containerRegistry.RegisterDialog<AlterRuleDialog>();
            containerRegistry.RegisterDialog<SelectRuleDialog>();
            containerRegistry.RegisterDialog<GenerateSample>();
            containerRegistry.RegisterDialog<GenerateRecallDialog>();

            _regionManager.RegisterViewWithRegion("Cohere#MenuRegion", typeof(Menu));
            _regionManager.RegisterViewWithRegion("Cohere#TreeRegion", typeof(Tree));
            _regionManager.RegisterViewWithRegion("Cohere#ProductListRegion", typeof(ProductsList));
            _regionManager.RegisterViewWithRegion("Cohere#ProductReportRegion", typeof(ProductReport));
        }

        private void CreateWindow(string name)
        {
            new AdonisWindow
            {
                Title = $"Visual Ternera - Validacion",
                Content = _container?.Resolve<Views.Cohere>()
            }.Show();
        }
    }
}
