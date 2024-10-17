using Core.Services;
using Microsoft.IdentityModel.Tokens;
using System.Collections.ObjectModel;
using System.Text;
using VersionGit.Models;

namespace VersionGit.ViewModels
{
    public class PushViewModel : BindableBase, IDialogAware
    {
        public static string Title => "Publicar cambios";

        public DelegateCommand CloseDialogCommand { get; private set; }
        public DelegateCommand CancelDialogCommand { get; private set; }
        public DelegateCommand CreateBranch { get; private set; }
        public DialogCloseListener RequestClose { get; }

        public ObservableCollection<ChangedFile> ModifiedFiles { get; set; }
        public ObservableCollection<string> Branches { get; set; }
        public int TotalFiles { get; set; }

        private string _branch = string.Empty;
        public string Branch
        {
            get => _branch;
            set
            {
                SetProperty(ref _branch, value);
                CloseDialogCommand.RaiseCanExecuteChanged();
            }
        }

        private string _tag = string.Empty;
        public string Tag
        {
            get => _tag;
            set
            {
                SetProperty(ref _tag, value);
                CloseDialogCommand.RaiseCanExecuteChanged();
            }
        }

        private string _message = string.Empty;
        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        private bool _selectAll = true;
        public bool SelectAll
        {
            get => _selectAll;
            set
            {
                SetProperty(ref _selectAll, value);
                foreach (var item in ModifiedFiles)
                {
                    item.Selected = SelectAll;
                }
            }
        }

        public PushViewModel(IDialogService dialogService)
        {
            var gitStatus = GitInner.RunGitCommand("status", "--porcelain", Settings.Instance.EtiquetasDir);
            ModifiedFiles = new(gitStatus.Message
                .ToLower()
                .Split(SettingsService.Instance.Extension, StringSplitOptions.RemoveEmptyEntries)
                .Select(e =>
                {
                    var str = e.Trim() + SettingsService.Instance.Extension;
                    var st = str.Split(' ', 2);
                    return new ChangedFile(st[1], (st[0] == "??") ? 'A' : st[0].ToUpper().First());
                }));

            TotalFiles = ModifiedFiles.Count;

            var gitBranch = GitInner.RunGitCommand("branch", "--format=%(refname:short)\\n", Settings.Instance.EtiquetasDir);
            Branches = new(gitBranch.Message.Split("\\n", StringSplitOptions.RemoveEmptyEntries));

            CreateBranch = new DelegateCommand(() =>
                dialogService.Show("CreateBranch", result =>
                    {
                        if (result.Result != ButtonResult.OK)
                        {
                            return;
                        }

                        if (result.Parameters["BranchName"] is string newBranch)
                        {
                            Branches.Add(newBranch);
                        }
                    }
                )
            );

            CancelDialogCommand = new DelegateCommand(RequestClose.Invoke);
            CloseDialogCommand = new DelegateCommand(Push, () => !(Tag.IsNullOrEmpty() || Branch.IsNullOrEmpty()));
        }

        public Boolean CanCloseDialog() => true;

        public void OnDialogClosed() { }

        public void OnDialogOpened(IDialogParameters parameters) { }

        private void Push()
        {
            var files = ModifiedFiles
                .Where(f => f.Selected)
                .Aggregate(new StringBuilder(), (sb, f) => sb.Append(f.Name + ' '))
                .ToString();
            var msg = Message.IsNullOrEmpty() ? Tag : Message;

            GitInner.RunGitCommand("add", files, Settings.Instance.EtiquetasDir);
            GitInner.RunGitCommand("commit", $"-m {msg}", Settings.Instance.EtiquetasDir);
            GitInner.RunGitCommand("tag", $"-a {Tag} -m {msg}", Settings.Instance.EtiquetasDir);
            GitInner.RunGitCommand("push", $"origin -u {Branch}", Settings.Instance.EtiquetasDir);
            GitInner.RunGitCommand("push", "origin --tags", Settings.Instance.EtiquetasDir);
        }
    }
}
