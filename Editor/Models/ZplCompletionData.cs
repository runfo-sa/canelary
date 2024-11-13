using Editor.Views;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using System.Windows.Media;

namespace Editor.Models
{
    public class ZplCompletionData(ZplCommand command) : ICompletionData
    {
        private readonly ZplCommand _command = command;

        public ImageSource? Image => null;

        public String Text { get; private set; } = command.Name;

        public Object Content => Text;

        public Object Description => new DocumentationPage(_command);

        public Double Priority => 1.0;

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            var offset = _command.Command.Length - _command.Command.LastIndexOf('|') - 1;
            textArea.Document.Replace(completionSegment, _command.Usage[1..]);
            textArea.Caret.Location = new TextLocation(textArea.Caret.Location.Line, textArea.Caret.Location.Column - offset);
        }
    }
}