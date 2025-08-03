using System.Globalization;
using System.Windows.Data;

namespace ArrivalsUI
{
    public class TimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int sec = (int)value;
            return sec >= 60 ? (sec / 60).ToString()+" min" : sec.ToString()+" sec";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
    }
}
