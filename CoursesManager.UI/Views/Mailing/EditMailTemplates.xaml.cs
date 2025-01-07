using CoursesManager.UI.Enums;
using CoursesManager.UI.Service.PlaceholderService;
using CoursesManager.UI.ViewModels.Mailing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

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
            PlaceholderService placeholderService = new();

            var placeholders = placeholderService.GetValidPlaceholders();
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

                    TextPointer endOfCurrentBlock = currentCaret.GetLineStartPosition(1, out int linesMoved);
                    if (linesMoved == 0 || endOfCurrentBlock == null)
                    {
                        endOfCurrentBlock = richTextBox.Document.ContentEnd.GetPositionAtOffset(-1);
                        while (endOfCurrentBlock != null &&
                               (endOfCurrentBlock.GetPointerContext(LogicalDirection.Forward) ==
                                TextPointerContext.None ||
                                endOfCurrentBlock.GetPointerContext(LogicalDirection.Forward) ==
                                TextPointerContext.ElementEnd))
                        {
                            endOfCurrentBlock = endOfCurrentBlock.GetPositionAtOffset(1, LogicalDirection.Forward);
                        }
                    }

                    if (endOfCurrentBlock != null)
                    {
                        richTextBox.CaretPosition = endOfCurrentBlock;

                        var newLine = Environment.NewLine;

                        selection.Text = $"<p></p>{newLine}";
                    }
                    else
                    {
                        richTextBox.CaretPosition = richTextBox.Document.ContentEnd.GetPositionAtOffset(-1);
                        var newLine = Environment.NewLine;

                        selection.Text = $"{newLine}<p></p>";
                    }
                    var newPosition = selection.Start.GetPositionAtOffset(3, LogicalDirection.Forward);
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

                if (leadingText.EndsWith("<p>") && trailingText.StartsWith("</p>"))
                {
                    TextPointer start = currentCaret.GetPositionAtOffset(-3);
                    TextPointer end = currentCaret.GetPositionAtOffset(4);

                    richTextBox.Selection.Select(start, end);
                    richTextBox.Selection.Text = "";

                    var nextLineStart = end.GetLineStartPosition(1);
                    if (nextLineStart != null)
                    {
                        richTextBox.Selection.Select(start, nextLineStart);
                        richTextBox.Selection.Text = "";
                    }

                    richTextBox.CaretPosition = start.GetPositionAtOffset(-6);


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
            try
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
            catch (Exception ex)
            {
                viewModel.ShowErrorMessage("Er is een overwachte fout opgetreden, probeer opnieuw", ToastType.Warning);
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
