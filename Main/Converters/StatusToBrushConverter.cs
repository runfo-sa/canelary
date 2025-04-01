using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

using Core.Database.ServiceDbModels;

namespace Main.Converters;

[ValueConversion(typeof(ClientStatus), typeof(Brush))]
public class StatusToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var status = (ClientStatus)value;
        return status switch
        {
            ClientStatus.DesactualizadaSobrantes => "#DD5746",
            ClientStatus.MultipleInstalaciones => "#8B322C",
            ClientStatus.Desactualizada => "#DD5746",
            ClientStatus.NoInstalado => "#8B322C",
            ClientStatus.Sobrantes => "#FFC470",
            ClientStatus.Okay => "#4793AF",
            _ => throw new NotImplementedException()
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}