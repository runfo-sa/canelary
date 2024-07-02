namespace Editor.Services
{
    public class EditorPreviewMediator : IEditorPreviewMediator
    {
        private readonly CompositeCommand _generatePreview = new();
        public CompositeCommand GeneratePreview => _generatePreview;

        private readonly CompositeCommand _sendErrors = new();
        public CompositeCommand SendErrors => _sendErrors;

        private readonly CompositeCommand _generateLinter = new();
        public CompositeCommand GenerateLinter => _generateLinter;

        private readonly CompositeCommand _sendData = new();
        public CompositeCommand SendData => _sendData;
    }
}
