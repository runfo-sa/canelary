using System.Windows.Media;

using AvalonEditB.Rendering;

using DiffPlex.DiffBuilder.Model;

namespace Comparator.Helpers;

/// <summary>
/// Resalta las lineas indicadas con un color diferente en base al tipo de cambio ocurrido.
/// </summary>
public class Highlighter(List<DiffPiece> lines) : IBackgroundRenderer
{
    public KnownLayer Layer => KnownLayer.Background;
    private static Color deletedColor = Color.FromArgb(50, 232, 155, 180);
    private static Color modifiedColor = Color.FromArgb(50, 232, 194, 155);
    private static Color insertedColor = Color.FromArgb(50, 181, 232, 155);

    public void Draw(TextView textView, DrawingContext drawingContext)
    {
        foreach (var line in lines)
        {
            if (line.Position is not null)
            {
                var visualLine = textView.GetVisualLine(line.Position!.Value);

                if (visualLine != null)
                {
                    foreach (var rc in BackgroundGeometryBuilder.GetRectsFromVisualSegment(textView, visualLine, 0, 10000))
                    {
                        if (line.Type == ChangeType.Deleted)
                        {
                            drawingContext.DrawRectangle(
                                new SolidColorBrush(deletedColor), null,
                                new System.Windows.Rect(0, rc.Top, textView.ActualWidth, rc.Height)
                            );
                        }
                        else if (line.Type == ChangeType.Inserted)
                        {
                            drawingContext.DrawRectangle(
                                new SolidColorBrush(insertedColor), null,
                                new System.Windows.Rect(0, rc.Top, textView.ActualWidth, rc.Height)
                            );
                        }
                        else if (line.Type == ChangeType.Modified)
                        {
                            drawingContext.DrawRectangle(
                                new SolidColorBrush(modifiedColor), null,
                                new System.Windows.Rect(0, rc.Top, textView.ActualWidth, rc.Height)
                            );
                        }
                    }
                }
            }
        }
    }
}