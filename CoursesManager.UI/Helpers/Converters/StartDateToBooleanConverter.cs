using System.Globalization;
using System.Windows.Data;

namespace CoursesManager.UI.Helpers.Converters
{
    public class StartDateToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime startDate)
            {
                return DateTime.Now.Date < startDate.Date && DateTime.Now.Date >= startDate.Date.AddDays(-7 * 3);
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
