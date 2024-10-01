using Core.Services;

namespace Publish.ViewModels
{
    public class PublishViewModel : BindableBase
    {
        public DelegateCommand PublishCommand { get; private set; }

        public PublishViewModel()
        {
            PublishCommand = new(VersionServiceProvider.Version.Publish);
        }
    }
}
