using Core.Events;

namespace Publish.ViewModels
{
    public class PublishViewModel(IEventAggregator eventAggregator) : BindableBase
    {
        public DelegateCommand PublishCommand { get; private set; } =
            new(() => eventAggregator
                .GetEvent<PublishEvent>()
                .Publish());
    }
}
