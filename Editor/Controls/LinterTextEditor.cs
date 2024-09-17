using Editor.Models;
using Editor.Services;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Editor.Controls
{
    public class LinterTextEditor : TextEditor
    {
        private readonly TextMarkerService _markerService;
        private ToolTip? _toolTip;

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
        }

        private void ShowTooltip(object sender, MouseEventArgs e)
        {
            var pos = TextArea.TextView.GetPositionFloor(e.GetPosition(TextArea.TextView) + TextArea.TextView.ScrollOffset);
            if (pos.HasValue)
            {
                TextLocation logicalPosition = pos.Value.Location;
                int offset = Document.GetOffset(logicalPosition);

                var markersAtOffset = _markerService.GetMarkersAtOffset(offset);
                var markerWithToolTip = markersAtOffset.FirstOrDefault(marker => marker.ToolTip != null);

                if (markerWithToolTip is not null)
                {
                    if (_toolTip is null)
                    {
                        _toolTip = new ToolTip();
                        _toolTip.Closed += ToolTipClosed;
                        _toolTip.PlacementTarget = this;
                        _toolTip.Content = new TextBlock
                        {
                            Text = markerWithToolTip.ToolTip,
                            TextWrapping = TextWrapping.Wrap
                        };
                        _toolTip.IsOpen = true;
                        e.Handled = true;
                    }
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

        private double _scrollOffset;
        private int _caretLine;

        private void OnScrollOffsetUnload(Object sender, RoutedEventArgs e)
        {
            _scrollOffset = VerticalOffset;
            _caretLine = TextArea.Caret.Line;
            Trace.WriteLine("unload");
        }

        private void OnScrollOffsetLoad(Object sender, DependencyPropertyChangedEventArgs e)
        {
            ScrollToVerticalOffset(_scrollOffset);
            TextArea.Caret.Line = _caretLine;

            if (IsVisible)
                Trace.WriteLine("load");
            else
                Trace.WriteLine("unload");
        }
    }
}
