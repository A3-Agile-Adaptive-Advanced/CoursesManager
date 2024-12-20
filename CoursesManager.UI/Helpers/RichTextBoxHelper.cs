using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows;

namespace CoursesManager.UI.Helpers
{
    public static class RichTextBoxHelper
    {
        public static readonly DependencyProperty DocumentProperty =
            DependencyProperty.RegisterAttached(
                "Document",
                typeof(FlowDocument),
                typeof(RichTextBoxHelper),
                new FrameworkPropertyMetadata(null, OnDocumentChanged));

        public static FlowDocument GetDocument(DependencyObject obj)
        {
            return (FlowDocument)obj.GetValue(DocumentProperty);
        }

        public static void SetDocument(DependencyObject obj, FlowDocument value)
        {
            obj.SetValue(DocumentProperty, value);
        }

        private static void OnDocumentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RichTextBox richTextBox)
            {
                var newDocument = e.NewValue as FlowDocument;
                richTextBox.Document = newDocument ?? new FlowDocument();

                richTextBox.TextChanged -= RichTextBox_TextChanged;
                richTextBox.TextChanged += RichTextBox_TextChanged;
            }
        }

        private static void RichTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is RichTextBox richTextBox)
            {
                var document = richTextBox.Document;
                var viewModelDocument = GetDocument(richTextBox);

                if (document != viewModelDocument)
                {
                    SetDocument(richTextBox, document);
                }
            }
        }
    }

}
