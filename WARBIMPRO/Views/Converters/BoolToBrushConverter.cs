using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace WARBIMPRO.Views.Converters
{
    /// <summary>
    /// Convierte bool → Brush (o Color/string).
    /// ConverterParameter = "ValorSiTrue|ValorSiFalse"
    /// Ejemplo: ConverterParameter="#4F8EF7|Transparent"
    /// </summary>
    public class BoolToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b && parameter is string param)
            {
                var parts = param.Split('|');
                var chosen = b ? parts[0] : (parts.Length > 1 ? parts[1] : "Transparent");

                try
                {
                    var converter = new BrushConverter();
                    return converter.ConvertFromString(chosen) ?? Brushes.Transparent;
                }
                catch
                {
                    return Brushes.Transparent;
                }
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}