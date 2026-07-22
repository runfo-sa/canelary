namespace Editor.Services;

public interface IEditorPreviewMediator
{
    CompositeCommand GeneratePreview { get; }
    CompositeCommand SendErrors { get; }
    CompositeCommand GenerateLinter { get; }
    CompositeCommand SendData { get; }
    CompositeCommand TabSelected { get; }
}