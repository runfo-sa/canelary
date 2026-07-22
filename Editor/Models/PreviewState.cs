using Core.Controls;

namespace Editor.Models;

public class PreviewState
{
    public required List<byte[]> RawLabelsData { get; set; }

    public int CurrentLabel { get; set; }

    public double PreviewAngle { get; set; }

    public ZoomState Zoom { get; set; } = ZoomState.Identity;
}
