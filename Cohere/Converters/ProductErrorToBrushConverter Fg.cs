using Core.Database.Model;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Cohere.Converters
{
    [ValueConversion(typeof(ProductoError), typeof(Brush))]
    public class ProductErrorToForegroundBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var error = (ProductoError)value;
            return error switch
            {
                ProductoError.Ninguno => Application.Current.FindResource(AdonisUI.Brushes.ForegroundBrush),
                ProductoError.Incompleto => "#ff3232",
                ProductoError.Incoherente => "#ff7532",
                _ => throw new NotImplementedException()
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
