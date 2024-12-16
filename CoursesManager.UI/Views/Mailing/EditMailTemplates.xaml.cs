using CoursesManager.MVVM.Data;
using CoursesManager.UI.ViewModels.Courses;
using CoursesManager.UI.ViewModels.Mailing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CoursesManager.UI.Views.Mailing
{
    /// <summary>
    /// Interaction logic for EditMailTemplates.xaml
    /// </summary>
    public partial class EditMailTemplates : UserControl
    {
        public EditMailTemplates()
        {
            InitializeComponent();
        }


        private void BoldButton_Click(object sender, RoutedEventArgs e)
        {
            var selection = richTextBox.Selection;
            if (selection != null)
            {
                var viewModel = DataContext as EditMailTemplatesViewModel;
                formatText(selection, "b");
            }
        }

        private void ItalicButton_Click(object sender, RoutedEventArgs e)
        {
            var selection = richTextBox.Selection;
            if (selection != null)
            {
                var viewModel = DataContext as EditMailTemplatesViewModel;
                formatText(selection, "i");
            }
        }

        private void UnderlineButton_Click(object sender, RoutedEventArgs e)
        {
            var selection = richTextBox.Selection;
            if (selection != null)
            {
                var viewModel = DataContext as EditMailTemplatesViewModel;
                formatText(selection, "u");
            }
        }

        

        private void LinkButton_Click(object sender, RoutedEventArgs e)
        {
            var selection = richTextBox.Selection;
            if (selection != null)
            {
                var viewModel = DataContext as EditMailTemplatesViewModel;
                formatText(selection, "li");
            }
        }

        private void TitleButton_Click(object sender, RoutedEventArgs e)
        {
            var selection = richTextBox.Selection;
            if (selection != null)
            {
                var viewModel = DataContext as EditMailTemplatesViewModel;
                formatText(selection, "h1");
            }
        }

        private void AccentuateButton_Click(object sender, RoutedEventArgs e)
        {
            var selection = richTextBox.Selection;
            if (selection != null)
            {
                var viewModel = DataContext as EditMailTemplatesViewModel;
                formatText(selection, "strong");
            }
        }

        private void RichTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                e.Handled = true;

                var richTextBox = sender as RichTextBox;
                var selection = richTextBox?.Selection;

                if (selection != null)
                {
                    var currentCaret = richTextBox.CaretPosition;
                    var newLine = Environment.NewLine;

                    selection.Text = $"{newLine}<p></p>";

                    var newPosition = selection.Start.GetPositionAtOffset(7, LogicalDirection.Forward);
                    if (newPosition != null)
                    {
                        richTextBox.CaretPosition = newPosition;
                    }
                }
            }
        }

        private void formatText(TextSelection selection, string formatType)
        {
            if (selection != null)
            {
                string selectedText = selection.Text.Trim();

                if (!string.IsNullOrWhiteSpace(selectedText))
                {
                    string wrappedText = $"<{formatType}>{selectedText}</{formatType}>";
                    selection.Text = wrappedText;
                }
            }
        }


    }
}
