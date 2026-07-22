using System.Windows;
using System.Windows.Media;

using AvalonEditB;
using AvalonEditB.Document;
using AvalonEditB.Rendering;

namespace Editor.Services;

public class TextMarkerService(TextEditor textEditor) : IBackgroundRenderer, IVisualLineTransformer
{
    private readonly TextEditor _textEditor = textEditor;
    private readonly TextSegmentCollection<TextMarker> markers = new(textEditor.Document);
    private static readonly Dictionary<Color, SolidColorBrush> _brushCache = [];
    private static readonly Dictionary<Color, Pen> _penCache = [];

    private static SolidColorBrush GetBrush(Color color)
    {
        if (!_brushCache.TryGetValue(color, out var brush))
        {
            brush = new SolidColorBrush(color);
            brush.Freeze();
            _brushCache[color] = brush;
        }
        return brush;
    }

    private static Pen GetPen(Color color)
    {
        if (!_penCache.TryGetValue(color, out var pen))
        {
            pen = new Pen(GetBrush(color), 1);
            pen.Freeze();
            _penCache[color] = pen;
        }
        return pen;
    }

    public sealed class TextMarker : TextSegment
    {
        public TextMarker(int startOffset, int length)
        {
            StartOffset = startOffset;
            Length = length;
        }

        public Color? BackgroundColor { get; set; }
        public Color MarkerColor { get; set; }
        public string? ToolTip { get; set; }
    }

    public void Draw(TextView textView, DrawingContext drawingContext)
    {
        if (markers == null || !textView.VisualLinesValid)
        {
            return;
        }
        var visualLines = textView.VisualLines;
        if (visualLines.Count == 0)
        {
            return;
        }
        int viewStart = visualLines[0].FirstDocumentLine.Offset;
        int viewEnd = visualLines[^1].LastDocumentLine.EndOffset;
        foreach (TextMarker marker in markers.FindOverlappingSegments(viewStart, viewEnd - viewStart))
        {
            if (marker.BackgroundColor != null)
            {
                var geoBuilder = new BackgroundGeometryBuilder { AlignToWholePixels = true, CornerRadius = 3 };
                geoBuilder.AddSegment(textView, marker);
                Geometry geometry = geoBuilder.CreateGeometry();
                if (geometry != null)
                {
                    drawingContext.DrawGeometry(GetBrush(marker.BackgroundColor.Value), null, geometry);
                }
            }
            foreach (Rect r in BackgroundGeometryBuilder.GetRectsForSegment(textView, marker))
            {
                Point startPoint = r.BottomLeft;
                Point endPoint = r.BottomRight;

                var usedPen = GetPen(marker.MarkerColor);
                const double offset = 2.5;

                int count = Math.Max((int)((endPoint.X - startPoint.X) / offset) + 1, 4);

                var geometry = new StreamGeometry();

                using (StreamGeometryContext ctx = geometry.Open())
                {
                    ctx.BeginFigure(startPoint, false, false);
                    ctx.PolyLineTo(CreatePoints(startPoint, offset, count).ToArray(), true, false);
                }

                geometry.Freeze();

                drawingContext.DrawGeometry(Brushes.Transparent, usedPen, geometry);
            }
        }
    }

    public KnownLayer Layer
    {
        get { return KnownLayer.Selection; }
    }

    public void Transform(ITextRunConstructionContext context, IList<VisualLineElement> elements)
    { }

    private static IEnumerable<Point> CreatePoints(Point start, double offset, int count)
    {
        for (int i = 0; i < count; i++)
        {
            yield return new Point(start.X + (i * offset), start.Y - ((i + 1) % 2 == 0 ? offset : 0));
        }
    }

    public void Clear()
    {
        markers.Clear();
    }

    private void Redraw(ISegment segment)
    {
        _textEditor.TextArea.TextView.Redraw(segment);
    }

    public void Create(int offset, int length, string message)
    {
        var m = new TextMarker(offset, length);
        markers.Add(m);
        m.MarkerColor = Colors.Wheat;
        m.ToolTip = message;
        Redraw(m);
    }

    public IEnumerable<TextMarker> GetMarkersAtOffset(int offset)
    {
        return markers == null ? [] : markers.FindSegmentsContaining(offset);
    }
}