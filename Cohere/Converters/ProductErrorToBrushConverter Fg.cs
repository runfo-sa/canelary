using Core.Models;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Cohere.Converters
{
    [ValueConversion(typeof(ProductError), typeof(Brush))]
    public class ProductErrorToForegroundBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var error = (ProductError)value;
            return error switch
            {
                ProductError.None => Application.Current.FindResource(AdonisUI.Brushes.ForegroundBrush),
                ProductError.Incomplete => "#ff3232",
                ProductError.Incoherent => "#ff7532",
                _ => throw new NotImplementedException()
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}