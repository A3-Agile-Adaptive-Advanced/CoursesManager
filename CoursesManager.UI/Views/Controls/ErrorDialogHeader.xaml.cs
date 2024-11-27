using System.Windows;
using System.Windows.Controls;

namespace CoursesManager.UI.Views.Controls
{
    /// <summary>
    /// Interaction logic for ErrorDialogHeader.xaml
    /// </summary>
    public partial class ErrorDialogHeader : UserControl
    {
        public ErrorDialogHeader()
        {
            InitializeComponent();
        }

        public string HeaderTitle
        {
            get => (string)GetValue(HeaderTitleProperty);
            set => SetValue(HeaderTitleProperty, value);
        }

        public static readonly DependencyProperty HeaderTitleProperty = DependencyProperty.Register(
            nameof(HeaderTitle),
            typeof(string),
            typeof(ErrorDialogHeader),
            new PropertyMetadata(string.Empty));
    }
}