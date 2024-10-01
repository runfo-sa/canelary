using Core.Services;

namespace PreviewLabelary
{
    [Module(ModuleName = "LabelaryPreview#False", OnDemand = false)]
    public class LabelaryModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            if (SettingsService.Instance.Preview == "Labelary")
            {
                PreviewServiceProvider.Set(content => new Labelary(content));
                // Como cambiar el regex de la variable:
                // HighlightingManager.Instance
                //  .GetDefinition("ZPL")
                //  .MainRuleSet
                //  .Rules
                //  .First(r => r.Color.Name == "Variable")
            }
        }

        public void RegisterTypes(IContainerRegistry containerRegistry) { }
    }
}
