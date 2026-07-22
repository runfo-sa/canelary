using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using AvalonEditB.CodeCompletion;
using AvalonEditB.Document;
using AvalonEditB.Rendering;

using Editor.Models;
using Editor.Services;

using TextEditor = AvalonEditB.TextEditor;

namespace Editor.Controls;

public class LinterTextEditor : TextEditor
{
    private readonly TextMarkerService _markerService;
    private ToolTip? _toolTip;
    private CompletionWindow? _completionWindow;

    public static readonly DependencyProperty LinterDataProperty =
        DependencyProperty.Register(
            name: "LinterData",
            propertyType: typeof(List<LintingInfo>),
            ownerType: typeof(LinterTextEditor),
            typeMetadata: new PropertyMetadata(OnLinterDataChanged));

    public List<LintingInfo> LinterData
    {
        get { return (List<LintingInfo>)GetValue(LinterDataProperty); }
        set { SetValue(LinterDataProperty, value); }
    }

    public LinterTextEditor()
    {
        _markerService = new TextMarkerService(this);

        TextView textView = TextArea.TextView;
        textView.BackgroundRenderers.Add(_markerService);
        textView.LineTransformers.Add(_markerService);
        textView.Services.AddService(typeof(TextMarkerService), _markerService);
        textView.MouseHover += ShowTooltip;
        textView.MouseHoverStopped += HideTooltip;
        textView.VisualLinesChanged += VisualLinesChanged;

        TextArea.TextEntered += LoadIntellisense;
        Unloaded += OnUnloaded;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        TextView textView = TextArea.TextView;
        textView.MouseHover -= ShowTooltip;
        textView.MouseHoverStopped -= HideTooltip;
        textView.VisualLinesChanged -= VisualLinesChanged;
        TextArea.TextEntered -= LoadIntellisense;
        Unloaded -= OnUnloaded;

        _completionWindow?.Close();
        _completionWindow = null;
    }

    private void ShowTooltip(object sender, MouseEventArgs e)
    {
        var pos = TextArea.TextView.GetPositionFloor(e.GetPosition(TextArea.TextView) + TextArea.TextView.ScrollOffset);
        if (pos.HasValue)
        {
            TextLocation logicalPosition = pos.Value.Location;
            int offset = Document.GetOffset(logicalPosition);

            var line = TextArea.Document.GetLineByOffset(offset);
            var lineText = TextArea.Document.GetText(line.Offset, line.Length);
            if (line.Length == 0 || offset > (line.Offset + line.Length - 1))
            {
                e.Handled = true;
                return;
            }

            var markersAtOffset = _markerService.GetMarkersAtOffset(offset);
            var markerWithToolTip = markersAtOffset.FirstOrDefault(marker => marker.ToolTip != null);

            if (_toolTip is null)
            {
                _toolTip = new ToolTip();
                _toolTip.Closed += ToolTipClosed;
                _toolTip.PlacementTarget = this;
                _toolTip.HasDropShadow = true;

                if (markerWithToolTip is not null)
                {
                    _toolTip.Content = new TextBlock
                    {
                        Text = markerWithToolTip.ToolTip,
                        TextWrapping = TextWrapping.Wrap,
                        Foreground = Foreground,
                        Background = Background
                    };
                }
                else
                {
                    var cursor = offset - line.Offset;
                    var startCmd = int.Max(lineText.LastIndexOf('^', cursor), 0);
                    var _endCmd = lineText.IndexOf('^', cursor);
                    var endCmd = (_endCmd == -1) ? line.Length : _endCmd;

                    var command = ZplCompletionList.List
                        .FirstOrDefault(i => lineText[startCmd..endCmd].Contains(i.Key))
                        .Value;

                    if (command != null)
                    {
                        _toolTip.Content = command.Description;
                    }
                    else
                    {
                        _toolTip = null;
                        e.Handled = true;
                        return;
                    }
                }

                _toolTip.IsOpen = true;
                e.Handled = true;
            }
        }
    }

    private void ToolTipClosed(object sender, RoutedEventArgs e)
    {
        _toolTip = null;
    }

    private void HideTooltip(object sender, MouseEventArgs e)
    {
        if (_toolTip is not null)
        {
            _toolTip.IsOpen = false;
            e.Handled = true;
        }
    }

    private void VisualLinesChanged(object? sender, EventArgs e)
    {
        if (_toolTip is not null)
        {
            _toolTip.IsOpen = false;
        }
    }

    private static void OnLinterDataChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        var editor = (LinterTextEditor)sender;
        if (editor is not null && editor.LinterData is not null)
        {
            editor._markerService.Clear();
            foreach (LintingInfo l in editor.LinterData)
            {
                editor._markerService.Create(l.Offset, l.Length, l.Message);
            }
        }
    }

    private void OnCompletionWindowClosed(object? sender, EventArgs e)
    {
        if (_completionWindow != null)
        {
            _completionWindow.Closed -= OnCompletionWindowClosed;
            _completionWindow = null;
        }
    }

    private void LoadIntellisense(object sender, TextCompositionEventArgs e)
    {
        if (e.Text == "^" || e.Text == "~")
        {
            _completionWindow?.Close();
            _completionWindow = new CompletionWindow(TextArea);
            var data = _completionWindow.CompletionList.CompletionData;

            ZplCompletionList.List
                .Where(i => i.Key.StartsWith(e.Text))
                .ToList()
                .ForEach(i =>
                {
                    data.Add(i.Value);
                });

            _completionWindow.WindowStyle = WindowStyle.None;
            _completionWindow.AllowsTransparency = true;
            _completionWindow.Background = Background;
            _completionWindow.Foreground = Foreground;
            _completionWindow.Width = 256.0;
            _completionWindow.CloseAutomatically = false;

            _completionWindow.Show();
            _completionWindow.Closed += OnCompletionWindowClosed;
        }
        else if (_completionWindow != null)
        {
            var data = _completionWindow.CompletionList.CompletionData;
            data.Clear();

            ZplCompletionList.List
                .Where(i => i.Key.StartsWith(e.Text))
                .ToList()
                .ForEach(i =>
                {
                    data.Add(i.Value);
                });
        }
    }
}