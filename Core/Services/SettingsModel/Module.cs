using Material.Icons;

namespace Core.Services.SettingsModel
{
    public record struct Module(string Name, MaterialIconKind Icon, string Description);
}
