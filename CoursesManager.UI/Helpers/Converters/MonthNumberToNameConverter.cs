using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CoursesManager.UI.Helpers.Converters
{
    public class MonthNumberToNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return DependencyProperty.UnsetValue;

            var dutchCulture = new CultureInfo("nl-NL");

            // Extract the month number:
            // - If 'value' is DateTime, use date.Month
            // - Otherwise, try parsing 'value' as an integer
            // - If parsing fails, set month to 0
            int month = value is DateTime date
                ? date.Month
                : int.TryParse(value.ToString(), out var parsedMonth) ? parsedMonth : 0;

            if (month < 1 || month > 12)
                return DependencyProperty.UnsetValue;

            var monthName = dutchCulture.DateTimeFormat.GetMonthName(month);

            // If the month name is invalid or empty, return unset;
            // otherwise, capitalize the first letter and combine with the rest
            return string.IsNullOrEmpty(monthName)
                ? DependencyProperty.UnsetValue
                : char.ToUpper(monthName[0]) + monthName[1..];
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
