using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using VersionGit.Models;

namespace Version.Git.Converters
{
    [ValueConversion(typeof(ChangedFile), typeof(Brush))]
    public class TypeToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = (char)value;
            return status switch
            {
                'D' => "#DD5746",
                'M' => "#FFC470",
                'A' => "#4793AF",
                _ => throw new NotImplementedException()
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}