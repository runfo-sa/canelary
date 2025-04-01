using Editor.Services;

namespace Editor.ViewModels;

public class EditorViewModel : BindableBase
{
    public ICommandService CommandService { get; }

    public EditorViewModel(IContainerRegistry containerRegistry, IContainerProvider container)
    {
        containerRegistry.RegisterScoped<ICommandService, CommandsService>();
        containerRegistry.RegisterScoped<IEditorPreviewMediator, EditorPreviewMediator>();
        CommandService = container.Resolve<ICommandService>();
    }
}