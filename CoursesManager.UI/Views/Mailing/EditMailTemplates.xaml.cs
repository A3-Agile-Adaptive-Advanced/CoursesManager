using CoursesManager.MVVM.Data;
using CoursesManager.UI.ViewModels.Courses;
using CoursesManager.UI.ViewModels.Mailing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private EditMailTemplatesViewModel? viewModel { get; set; }
        private System.Windows.Documents.TextSelection selection { get; set; }
        public EditMailTemplates()
        {
            InitializeComponent();
            var placeholders = new List<string>
        {

            "[Cursus naam]",
            "[Cursus code]",
            "[Cursus beschrijving]",
            "[Cursus categorie]",
            "[Cursus startdatum]",
            "[Cursus einddatum]",
            "[Cursus locatie naam]",
            "[Cursus locatie land]",
            "[Cursus locatie postcode]",
            "[Cursus locatie stad]",
            "[Cursus locatie straat]",
            "[Cursus locatie huisnummer]",
            "[Cursus locatie toevoeging]",

            "[Betaal link]",

            "[Cursist naam]",
            "[Cursist email]",
            "[Cursist telefoonnummer]",
            "[Cursist geboortedatum]",
            "[Cursist adres land]",
            "[Cursist adres postcode]",
            "[Cursist adres stad]",
            "[Cursist adres straat]",
            "[Cursist adres huisnummer]",
            "[Cursist adres toevoeging]"
        };
            SuggestionsList.ItemsSource = placeholders;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            viewModel = DataContext as EditMailTemplatesViewModel;
        }


        private void BoldButton_Click(object sender, RoutedEventArgs e)
        {
            selection = richTextBox.Selection;
            if (selection != null)
            {
                formatText(selection, "b");
            }
            _ = selection;
        }

        private void ItalicButton_Click(object sender, RoutedEventArgs e)
        {
            selection = richTextBox.Selection;
            if (selection != null)
            {
                formatText(selection, "i");
            }
            _ = selection;
        }

        private void UnderlineButton_Click(object sender, RoutedEventArgs e)
        {
            selection = richTextBox.Selection;
            if (selection != null)
            {
                formatText(selection, "u");
            }
            _ = selection;
        }



        private void LinkButton_Click(object sender, RoutedEventArgs e)
        {
            selection = richTextBox.Selection;
            if (selection != null)
            {
                formatText(selection, "li");
            }
            _ = selection;
        }

        private void TitleButton_Click(object sender, RoutedEventArgs e)
        {
            selection = richTextBox.Selection;
            if (selection != null)
            {
                formatText(selection, "h1");
            }
            _ = selection;
        }

        private void AccentuateButton_Click(object sender, RoutedEventArgs e)
        {
            selection = richTextBox.Selection;
            if (selection != null)
            {
                formatText(selection, "strong");
            }
            _ = selection;
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
                    string trailingText = currentCaret.GetTextInRun(LogicalDirection.Forward);
                    string firstFourCharacters = trailingText.Length >= 4 ? trailingText.Substring(0, 4) : trailingText;
                    if (firstFourCharacters == "</p>")
                    {
                        richTextBox.CaretPosition = currentCaret.GetPositionAtOffset(4);
                    }

                    var newLine = Environment.NewLine;

                    selection.Text = $"{newLine}<p></p>";

                    var newPosition = selection.Start.GetPositionAtOffset(7, LogicalDirection.Forward);
                    if (newPosition != null)
                    {
                        richTextBox.CaretPosition = newPosition;
                    }
                }
            }
            else if (e.Key == Key.Back)
            {
                var currentCaret = richTextBox.CaretPosition;
                string leadingText = currentCaret.GetTextInRun(LogicalDirection.Backward);
                string trailingText = currentCaret.GetTextInRun(LogicalDirection.Forward);

                if (leadingText == "<p>" && trailingText.StartsWith("</p>"))
                {
                    richTextBox.Selection.Select(currentCaret.GetPositionAtOffset(-3), currentCaret.GetPositionAtOffset(4));
                    richTextBox.Selection.Text = "";

                    richTextBox.CaretPosition = currentCaret.GetPositionAtOffset(-8);

                    e.Handled = true;
                }
            }
            else if (e.Key == Key.OemOpenBrackets)
            {
                var caretPosition = richTextBox.CaretPosition;
                richTextBox.Selection.Text = "[";

                richTextBox.CaretPosition = richTextBox.Selection.End;
                SuggestionsPopup.IsOpen = true;
                e.Handled = true;
            }
            else if (SuggestionsPopup.IsOpen)
            {
                if (e.Key == Key.Escape)
                {
                    SuggestionsPopup.IsOpen = false;
                    e.Handled = true;
                }
                else if (e.Key != Key.Up || e.Key != Key.Down || e.Key != Key.Enter)
                {
                    SuggestionsPopup.IsOpen = false;
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
        private void SuggestionsList_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                SuggestionsPopup.IsOpen = false;
                e.Handled = true;
            }
            else if (e.Key != Key.Up && e.Key != Key.Down && e.Key != Key.Enter)
            {
                SuggestionsPopup.IsOpen = false;
            }
        }

        private void InsertSelectedText(string selectedText)
        {
            if (!string.IsNullOrEmpty(selectedText))
            {
                    richTextBox.Selection.Text = selectedText;
                    TextPointer selectionStart = richTextBox.Selection.Start;
                    TextPointer previousPosition = selectionStart.GetPositionAtOffset(-1, LogicalDirection.Backward);

                    if (previousPosition != null)
                    {
                        previousPosition.DeleteTextInRun(1);
                    }

                richTextBox.CaretPosition = richTextBox.Selection.End;

                SuggestionsPopup.IsOpen = false;
                SuggestionsList.SelectedItem = null;
                richTextBox.Focus();
            }
        }

        private void SuggestionsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SuggestionsPopup.IsOpen && SuggestionsList.SelectedItem is string selectedText)
            {
                InsertSelectedText(selectedText);
            }
        }

    }
}
