using System.Globalization;
using System.Windows.Data;

using Core.Database.ServiceDbModels;

namespace Main.Converters;

[ValueConversion(typeof(ClientStatus), typeof(string))]
public class StatusToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var status = (ClientStatus)value;
        return status switch
        {
            ClientStatus.DesactualizadaSobrantes => "Desactualizado y Sobrantes",
            ClientStatus.MultipleInstalaciones => "Multiples Instalaciones",
            ClientStatus.Desactualizada => "Desactualizado",
            ClientStatus.NoInstalado => "PiQuatro No Instalado",
            ClientStatus.Sobrantes => "Archivos Sobrantes",
            ClientStatus.Okay => "Okay",
            _ => throw new NotImplementedException()
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}