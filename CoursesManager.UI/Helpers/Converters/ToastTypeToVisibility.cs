using System.Globalization;
using System.Windows.Data;
using System.Windows;
using CoursesManager.UI.Enums;

namespace CoursesManager.UI.Helpers.Converters;

// This method is needed to be able to use the Toasttype to control the visibility.
// This makes it possible to use 1 variable to control the visibility in the viewmodel to display 3 (or more) different message types.
public class ToastTypeToVisibility : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ToastType toastType && parameter is string iconType)
        {

            switch (iconType)
            {
                case "Warning":
                    return toastType == ToastType.Warning ? Visibility.Visible : Visibility.Collapsed;
                case "Error":
                    return toastType == ToastType.Error ? Visibility.Visible : Visibility.Collapsed;
                case "Confirmation":
                    return toastType == ToastType.Confirmation ? Visibility.Visible : Visibility.Collapsed;
                default:
                    return Visibility.Collapsed;
            }
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            return visibility == Visibility.Visible;
        }
        return false;
    }
}