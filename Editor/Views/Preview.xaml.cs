using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Core.Controls;

using Editor.ViewModels;

namespace Editor.Views;

public partial class Preview : UserControl
{
    public Preview()
    {
        InitializeComponent();
        DataContextChanged += Preview_DataContextChanged;
        PreviewZoomBorder.MouseWheel += (_, _) => SaveZoomState();
        PreviewZoomBorder.MouseLeftButtonUp += (_, _) => SaveZoomState();
        PreviewZoomBorder.PreviewMouseRightButtonDown += (_, _) => SaveZoomState();
    }

    private void Preview_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is PreviewViewModel oldViewModel)
        {
            oldViewModel.ApplyZoomState -= OnApplyZoomState;
        }

        if (e.NewValue is PreviewViewModel newViewModel)
        {
            newViewModel.ApplyZoomState += OnApplyZoomState;
        }
    }

    private void OnApplyZoomState(ZoomState? state)
    {
        PreviewZoomBorder.ApplyState(state ?? ZoomState.Identity);
    }

    private void SaveZoomState()
    {
        if (DataContext is PreviewViewModel viewModel)
        {
            viewModel.SaveZoomState(PreviewZoomBorder.GetState());
        }
    }
}
