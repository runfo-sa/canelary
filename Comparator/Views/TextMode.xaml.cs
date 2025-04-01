using System.Windows.Controls;

using Comparator.Helpers;
using Comparator.Services;

using DiffPlex.DiffBuilder.Model;

namespace Comparator.Views;

public partial class TextMode : UserControl
{
    public TextMode(ICommandService commandService)
    {
        InitializeComponent();
        leftEditor.Options.AllowScrollBelowDocument = true;
        rightEditor.Options.AllowScrollBelowDocument = true;
        commandService.GenerateDiff.RegisterCommand(new DelegateCommand<SideBySideDiffModel>(RenderDiff));
    }

    private void RenderDiff(SideBySideDiffModel diff)
    {
        leftEditor.TextArea.TextView.BackgroundRenderers.Clear();
        rightEditor.TextArea.TextView.BackgroundRenderers.Clear();

        leftEditor.TextArea.TextView.BackgroundRenderers.Add(new Highlighter(diff.OldText.Lines));
        rightEditor.TextArea.TextView.BackgroundRenderers.Add(new Highlighter(diff.NewText.Lines));
    }

    private void LeftEditor_ScrollChanged(Object sender, ScrollChangedEventArgs e)
    {
        rightEditor.ScrollToVerticalOffset(leftEditor.VerticalOffset);
    }

    private void RightEditor_ScrollChanged(Object sender, ScrollChangedEventArgs e)
    {
        leftEditor.ScrollToVerticalOffset(rightEditor.VerticalOffset);
    }
}