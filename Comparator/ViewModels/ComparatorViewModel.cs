using Comparator.Models;
using Comparator.Services;
using Comparator.Views;

using MaterialDesignThemes.Wpf;

namespace Comparator.ViewModels;

public delegate void ComparatorDelegate();

/// <summary>
/// <para><b>View Model</b> para la ventana de Comparación de etiquetas.</para>
/// </summary>
public class ComparatorViewModel : BindableBase
{
    private readonly ICommandService _commandService;

    private bool _textMode = true;

    /// <summary>
    /// Indica si la ventana esta comparando codigo o no.
    /// </summary>
    public bool TextMode
    {
        get => _textMode;
        set => SetProperty(ref _textMode, value);
    }

    private bool _imageMode = false;

    /// <summary>
    /// Indica si la ventana esta comparando imagenes o no.
    /// </summary>
    public bool ImageMode
    {
        get => _imageMode;
        set => SetProperty(ref _imageMode, value);
    }

    private string _dialogIdentifier = $"Comparator_SelectDialog_{DateTime.Now}";

    public string DialogIdentifier
    {
        get => _dialogIdentifier;
        set => SetProperty(ref _dialogIdentifier, value);
    }

    public DelegateCommand ChangeFilesCommand { get; private set; }

    public DelegateCommand RefreshCommand { get; private set; }

    public ComparatorViewModel(IContainerRegistry containerRegistry, IContainerProvider container)
    {
        containerRegistry.RegisterScoped<ICommandService, CommandsService>();
        _commandService = container.Resolve<ICommandService>();

        ChangeFilesCommand = new(async () => await ChangeFiles());
        RefreshCommand = new(() => _commandService.Refresh.Execute(null));
    }

    private async Task ChangeFiles()
    {
        await DialogHost.Show(new SelectLabelsDialog(DialogIdentifier), DialogIdentifier, delegate (object sender, DialogClosingEventArgs args)
        {
            if (args.Parameter is SelectionResult result)
            {
                _commandService.ChangeFiles.Execute(result);
            }
        });
    }
}