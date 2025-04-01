using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

using Core.Events;
using Core.Models;

using Dragablz;

using Main.Models;

using Material.Icons;
using Material.Icons.WPF;

namespace Main.Controls;

public partial class SplitView : UserControl
{
    public IEnumerable ItemsSource
    {
        get => (IEnumerable)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(IEnumerable),
            typeof(SplitView),
            new PropertyMetadata(new PropertyChangedCallback(OnItemsSourcePropertyChanged))
        );

    private static readonly Lazy<IEventAggregator> _lazyEventAggregator =
          new(() => ContainerLocator.Container.Resolve<IEventAggregator>());

    private static IEventAggregator EventAggregator => _lazyEventAggregator.Value;

    private bool _isPaneOpen = true;
    private readonly List<(ModuleAction, Button)> _mods = [];
    private readonly Storyboard _closeAnim;
    private readonly Storyboard _openAnim;
    private readonly ObservableCollection<ModuleTab> _contentControls = [];

    public SplitView()
    {
        InitializeComponent();

        Tabs.ItemsSource = _contentControls;
        Tabs.ClosingItemCallback += TabControl_ClosingItemHandler;

        CompactPane.Width = new GridLength(300, GridUnitType.Pixel);
        DisplayPaneBtn.Content = new MaterialIcon() { Kind = Material.Icons.MaterialIconKind.ChevronLeft };
        _closeAnim = SlideAnim(300, 84, "CompactPane");
        _openAnim = SlideAnim(84, 300, "CompactPane");

        EventAggregator
            .GetEvent<SendModuleEvent>()
            .Subscribe(c =>
            {
                if (Tabs.Items.IsEmpty)
                {
                    Tabs.Visibility = Visibility.Visible;
                    BackgroundLogo.Visibility = Visibility.Collapsed;
                }

                _contentControls.Add(c);
                Tabs.SelectedIndex = Tabs.Items.Count - 1;
            }, ThreadOption.UIThread, true);
    }

    private static void OnItemsSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is SplitView control)
        {
            control.OnItemsSourceChanged((IEnumerable)e.NewValue);
        }
    }

    private void OnItemsSourceChanged(IEnumerable list)
    {
        foreach (var item in list)
        {
            var mod = (ModuleAction)item;
            var btn = new Button()
            {
                Style = Application.Current.FindResource("MaterialDesignRaisedSecondaryDarkButton") as Style,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Command = mod.Command,
                CommandParameter = mod.Module.Name,
                ToolTip = mod.Module.Description,
                Height = 48
            };

            UpdateBtn(btn, _isPaneOpen, mod.Module.Icon, mod.Module.Name);
            _mods.Add((mod, btn));

            var sep = new Separator() { Background = null, Height = 24 };
            SideBar.Children.Add(btn);
            SideBar.Children.Add(sep);
        }
    }

    private static void UpdateBtn(Button btn, bool showLabel, MaterialIconKind ic, string name)
    {
        var grid = new Grid();
        var c1 = new ColumnDefinition() { Width = new GridLength(25, GridUnitType.Star) };
        grid.ColumnDefinitions.Add(c1);

        var icon = new MaterialIcon() { Kind = ic };
        Grid.SetColumn(icon, 0);
        grid.Children.Add(icon);

        if (showLabel)
        {
            var c2 = new ColumnDefinition() { Width = new GridLength(75, GridUnitType.Star) };
            grid.ColumnDefinitions.Add(c2);
            var label = new TextBlock()
            {
                FontSize = 26,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Text = name
            };
            Grid.SetColumn(label, 1);
            grid.Children.Add(label);
        }

        btn.Content = grid;
    }

    private void DisplayPaneBtn_Click(object? sender, RoutedEventArgs e)
    {
        _isPaneOpen = !_isPaneOpen;
        if (!_isPaneOpen)
        {
            DisplayPaneBtn.Content = new MaterialIcon() { Kind = MaterialIconKind.ChevronRight };
            Title.Visibility = Visibility.Collapsed;
            VersionLabel.Visibility = Visibility.Hidden;
            _closeAnim.Begin(CompactPane);
        }
        else
        {
            DisplayPaneBtn.Content = new MaterialIcon() { Kind = MaterialIconKind.ChevronLeft };
            Title.Visibility = Visibility.Visible;
            VersionLabel.Visibility = Visibility.Visible;
            _openAnim.Begin(CompactPane);
        }

        foreach (var (mod, btn) in _mods)
        {
            UpdateBtn(btn, _isPaneOpen, mod.Module.Icon, mod.Module.Name);
        }
    }

    private static Storyboard SlideAnim(double from, double to, string name)
    {
        var anim = new GridLengthAnimation
        {
            From = new GridLength(from, GridUnitType.Pixel),
            To = new GridLength(to, GridUnitType.Pixel),
            Duration = new Duration(TimeSpan.FromMilliseconds(200))
        };

        Storyboard.SetTargetName(anim, name);
        Storyboard.SetTargetProperty(anim, new PropertyPath(ColumnDefinition.WidthProperty));
        var stb = new Storyboard();
        stb.Children.Add(anim);

        return stb;
    }

    private void TabControl_ClosingItemHandler(ItemActionCallbackArgs<TabablzControl> args)
    {
        if (Tabs.Items.Count <= 1)
        {
            Tabs.Visibility = Visibility.Hidden;
            BackgroundLogo.Visibility = Visibility.Visible;
        }
    }
}