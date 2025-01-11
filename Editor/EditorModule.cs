using Core.Events;
using Core.Models;
using Core.Services;
using Editor.Views;
using Editor.Views.Dialogs;

namespace Editor
{
    [Module(ModuleName = "Editar", OnDemand = true)]
    public class EditorModule(IRegionManager regionManager, IEventAggregator eventAggregator) : IModule
    {
        private readonly IRegionManager _regionManager = regionManager;
        private readonly IEventAggregator _eventAggregator = eventAggregator;
        private IContainerProvider? _container;

        public void OnInitialized(IContainerProvider containerProvider)
        {
            _container = containerProvider;
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterInstance(new ModuleLoaderService("Editar", CreateWindow));
            containerRegistry.RegisterDialog<ResizeLabelDialog>();

            _regionManager.RegisterViewWithRegion("Editor#MenuRegion", typeof(Menu));
            _regionManager.RegisterViewWithRegion("Editor#ToolbarRegion", typeof(Toolbar));
            _regionManager.RegisterViewWithRegion("Editor#TreeRegion", typeof(Tree));
            _regionManager.RegisterViewWithRegion("Editor#TextEditorRegion", typeof(TextEditor));
            _regionManager.RegisterViewWithRegion("Editor#PreviewRegion", typeof(Preview));
        }

        private void CreateWindow(string name)
        {
            _eventAggregator.GetEvent<SendModuleEvent>().Publish(new ModuleTab("Editor", _container?.Resolve<Views.Editor>()!));
        }
    }
}