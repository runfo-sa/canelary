using System.Collections.ObjectModel;
using Version.Database.Models;
using VersionDatabase.Db;

namespace VersionDatabase.ViewModels
{
    public class PublishViewModel : BindableBase
    {
        public ObservableCollection<LabelVersions> Labels { get; set; }

        public PublishViewModel()
        {
            using var context = new DatabaseDbContext();
            var labels = context.DefinicionEtiquetas.Select(e => new LabelVersions(e));
            Labels = [.. labels];
        }
    }
}
