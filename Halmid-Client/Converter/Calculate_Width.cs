using System;
using System.Windows.Data;

namespace Halmid_Client.Converter
{
    public class Calculate_Width : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double data = double.Parse(value.ToString()) - 80;
            return data;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
