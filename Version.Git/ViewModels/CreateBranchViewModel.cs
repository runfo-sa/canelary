using Microsoft.IdentityModel.Tokens;

using VersionGit.Models;

namespace VersionGit.ViewModels;

public class CreateBranchViewModel : BindableBase, IDialogAware
{
    public static string Title => "Crear nueva branch";

    private string _name = string.Empty;

    public string Name
    {
        get => _name;
        set
        {
            SetProperty(ref _name, value);
            CanClose = !(_branches.Contains(Name) || Name.IsNullOrEmpty());
        }
    }

    public DelegateCommand CloseDialogCommand { get; private set; }
    public DelegateCommand CancelDialogCommand { get; private set; }

    public DialogCloseListener RequestClose { get; }

    private readonly IEnumerable<string> _branches;

    private bool _canClose = false;

    public bool CanClose
    {
        get => _canClose;
        set => SetProperty(ref _canClose, value);
    }

    public CreateBranchViewModel()
    {
        var gitBranch = GitInner.RunGitCommand("branch", "--format=%(refname:short)\\n", Settings.Instance.EtiquetasDir);
        _branches = gitBranch.Message.Split("\\n", StringSplitOptions.RemoveEmptyEntries);

        CancelDialogCommand = new DelegateCommand(() => RequestClose.Invoke());

        CloseDialogCommand = new DelegateCommand(() =>
        {
            GitInner.RunGitCommand("branch", Name, Settings.Instance.EtiquetasDir);

            var result = new DialogResult
            {
                Parameters = new DialogParameters { { "BranchName", Name } },
                Result = ButtonResult.OK
            };
            RequestClose.Invoke(result);
        }, () => CanClose)
            .ObservesProperty(() => CanClose);
    }

    public Boolean CanCloseDialog() => true;

    public void OnDialogClosed()
    { }

    public void OnDialogOpened(IDialogParameters parameters)
    { }
}