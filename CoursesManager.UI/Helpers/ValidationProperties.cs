using System.Windows;

namespace CoursesManager.UI.Helpers
{
    /// <summary>
    /// Static helper class defining attached properties for validation.
    /// These properties enable declarative validation rules to be associated with UI elements.
    /// </summary>
    public static class ValidationProperties
    {
        public static readonly DependencyProperty IsRequiredProperty =
            DependencyProperty.RegisterAttached("IsRequired", typeof(bool),
                typeof(ValidationProperties), new PropertyMetadata(false));

        public static readonly DependencyProperty IsEmailProperty =
            DependencyProperty.RegisterAttached("IsEmail", typeof(bool),
                typeof(ValidationProperties), new PropertyMetadata(false));

        public static readonly DependencyProperty IsPhoneNumberProperty =
            DependencyProperty.RegisterAttached("IsPhoneNumber", typeof(bool),
                typeof(ValidationProperties), new PropertyMetadata(false));

        public static readonly DependencyProperty IsDateProperty =
            DependencyProperty.RegisterAttached("IsDate", typeof(bool),
                typeof(ValidationProperties), new PropertyMetadata(false));

        public static bool GetIsRequired(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsRequiredProperty);
        }

        public static void SetIsRequired(DependencyObject obj, bool value)
        {
            obj.SetValue(IsRequiredProperty, value);
        }

        public static bool GetIsEmail(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsEmailProperty);
        }

        public static void SetIsEmail(DependencyObject obj, bool value)
        {
            obj.SetValue(IsEmailProperty, value);
        }

        public static bool GetIsPhoneNumber(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsPhoneNumberProperty);
        }

        public static void SetIsPhoneNumber(DependencyObject obj, bool value)
        {
            obj.SetValue(IsPhoneNumberProperty, value);
        }

        public static bool GetIsDate(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsDateProperty);
        }

        public static void SetIsDate(DependencyObject obj, bool value)
        {
            obj.SetValue(IsDateProperty, value);
        }
    }
}