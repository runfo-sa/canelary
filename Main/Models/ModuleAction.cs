using Core.Models;

namespace Main.Models
{
    public record struct ModuleAction(ModuleMetadata Metadata, DelegateCommand<string> Command);
}
