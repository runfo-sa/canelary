using AdonisUI.Controls;
using Core.Services;
using Editor.Views;
using Editor.Views.Dialogs;

namespace Editor
{
    [Module(ModuleName = "Editor#True#FileEdit#1", OnDemand = true)]
    public class EditorModule(IRegionManager regionManager) : IModule
    {
        private readonly IRegionManager _regionManager = regionManager;
        private IContainerProvider? _container;

        public void OnInitialized(IContainerProvider containerProvider)
        {
            _container = containerProvider;
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterInstance(new ModuleLoaderService("Editor", CreateWindow));
            containerRegistry.RegisterDialog<ResizeLabelDialog>();

            _regionManager.RegisterViewWithRegion("Editor#MenuRegion", typeof(Menu));
            _regionManager.RegisterViewWithRegion("Editor#ToolbarRegion", typeof(Toolbar));
            _regionManager.RegisterViewWithRegion("Editor#TreeRegion", typeof(Tree));
            _regionManager.RegisterViewWithRegion("Editor#TextEditorRegion", typeof(TextEditor));
            _regionManager.RegisterViewWithRegion("Editor#PreviewRegion", typeof(Preview));
        }

        private void CreateWindow(string name)
        {
            new AdonisWindow
            {
                Title = $"Visual Ternera - Editor",
                Content = _container?.Resolve<Views.Editor>()
            }.Show();
        }
    }
}
