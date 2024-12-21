using CoursesManager.UI.Dialogs.ViewModels;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace CoursesManager.UI.Dialogs.Windows
{
    /// <summary>
    /// Interaction logic for TemplatePreviewDialogWindow.xaml
    /// </summary>
    public partial class TemplatePreviewDialogWindow : Window
    {
        public TemplatePreviewDialogWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is TemplatePreviewDialogViewModel viewModel)
            {
                webBrowser.NavigateToString(viewModel.Message);
            }
        }
    }
}
