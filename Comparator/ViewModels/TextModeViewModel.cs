using System.ComponentModel;

using AvalonEditB.Document;

using Comparator.Models;
using Comparator.Services;

using Core.FileTree;
using Core.Models;

using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;

namespace Comparator.ViewModels;

[RegionMemberLifetime(KeepAlive = true)]
public class TextModeViewModel : BindableBase
{
    private TextDocument _leftText = null!;

    /// <summary>
    /// Codigo a comparar del lado izquierdo, considerado como el 'codigo viejo'.
    /// </summary>
    public TextDocument LeftText
    {
        get => _leftText;
        set => SetProperty(ref _leftText, value);
    }

    private TextDocument _rightText = null!;

    /// <summary>
    /// Codigo a comparar del lado derecho, considerado como el 'codigo nuevo'.
    /// </summary>
    public TextDocument RightText
    {
        get => _rightText;
        set => SetProperty(ref _rightText, value);
    }

    private string _leftFilename = string.Empty;

    /// <summary>
    /// Nombre del archivo del lado izquierdo.
    /// </summary>
    public string LeftFilename
    {
        get => _leftFilename;
        set => SetProperty(ref _leftFilename, value);
    }

    private string _rightFilename = string.Empty;

    /// <summary>
    /// Nombre del archivo del lado derecho.
    /// </summary>
    public string RightFilename
    {
        get => _rightFilename;
        set => SetProperty(ref _rightFilename, value);
    }

    public DelegateCommand CalculateDiffCommand { get; private set; }

    private SideBySideDiffModel? _diff = null;
    private readonly ICommandService _commandService;

    public TextModeViewModel(ICommandService commandService, IFile leftFile, IFile rightFile)
    {
        LeftText = new TextDocument(leftFile.Read());
        RightText = new TextDocument(rightFile.Read());
        LeftFilename = leftFile.Name;
        RightFilename = rightFile.Name;
        _commandService = commandService;

        CalculateDiffCommand = new(() =>
        {
            if (_diff != null)
            {
                return;
            }

            var leftText = LeftText.Text;
            var rightText = RightText.Text;

            var worker = new BackgroundWorker();
            worker.DoWork += BackgroundWork;
            worker.RunWorkerCompleted += BackgroundDone;
            worker.RunWorkerAsync((leftText, rightText));
        });

        _commandService.ChangeFiles.RegisterCommand(new DelegateCommand<SelectionResult>(ChangeFiles));
        _commandService.Refresh.RegisterCommand(new DelegateCommand(()
            => ChangeFiles(new SelectionResult(leftFile, rightFile, new LabelDpi(), new LabelSize()))));
    }

    private void ChangeFiles(SelectionResult result)
    {
        _diff = null;
        LeftText = new TextDocument(result.LeftFile.Read());
        RightText = new TextDocument(result.RightFile.Read());
        LeftFilename = result.LeftFile.Name;
        RightFilename = result.RightFile.Name;
        CalculateDiffCommand.Execute();
    }

    private static void BackgroundWork(object? sender, DoWorkEventArgs e)
    {
        var tuple = ((string, string)?)e.Argument;
        e.Result = SideBySideDiffBuilder.Diff(tuple?.Item1, tuple?.Item2);
    }

    private void BackgroundDone(object? sender, RunWorkerCompletedEventArgs e)
    {
        _diff = (SideBySideDiffModel?)e.Result;
        _commandService.GenerateDiff.Execute(_diff);
    }
}