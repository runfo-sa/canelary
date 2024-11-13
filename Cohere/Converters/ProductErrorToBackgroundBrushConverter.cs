using Core.Models;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Cohere.Converters
{
    [ValueConversion(typeof(ProductError), typeof(Brush))]
    public class ProductErrorToBackgroundBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var error = (ProductError)value;
            return error switch
            {
                ProductError.None => Brushes.Transparent,
                ProductError.Incomplete => "#b92d2d",
                ProductError.Incoherent => "#b9702d",
                _ => throw new NotImplementedException()
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}