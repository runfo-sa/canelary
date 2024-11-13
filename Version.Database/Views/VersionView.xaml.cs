using Microsoft.EntityFrameworkCore;
using System.Windows.Controls;
using VersionDatabase.Db;

namespace VersionDatabase.Views
{
    public partial class VersionView : UserControl
    {
        public VersionView()
        {
            InitializeComponent();
            var context = new DatabaseDbContext().Database.GetDbConnection();
            DbName.Text = $"{context.DataSource}/{context.Database}";
        }
    }
}