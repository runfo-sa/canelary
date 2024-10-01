using Core.Services.SettingsModel;

namespace Main.Models
{
    /// <summary>
    /// Vincula un <see cref="Core.Services.SettingsModel.Module"/> con un comando.
    /// </summary>
    public record struct ModuleAction(Module Module, DelegateCommand<string> Command);
}
