namespace VersionGit.Models
{
    public class ChangedFile(string name, char type) : BindableBase
    {
        public string Name => name;
        public char Type => type;

        private bool _selected = true;
        public bool Selected
        {
            get => _selected;
            set => SetProperty(ref _selected, value);
        }
    }
}
