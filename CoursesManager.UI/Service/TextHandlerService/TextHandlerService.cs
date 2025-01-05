using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace CoursesManager.UI.Service.TextHandlerService
{
    /// <summary>
    /// Service for handling text-related operations such as formatting, extraction, and replacement in documents and HTML.
    /// </summary>
    public class TextHandlerService : ITextHandlerService
    {
        /// <summary>
        /// Updates the visible text in a document by highlighting flagged words with specific formatting.
        /// </summary>
        /// <param name="inputText">The original text to be processed.</param>
        /// <param name="flaggedWords">The list of words to highlight.</param>
        /// <returns>A FlowDocument containing the formatted text.</returns>
        public FlowDocument UpdateVisibleTextWithErrorFormatting(string inputText, List<string> flaggedWords)
        {
            FlowDocument newDocument = new FlowDocument();
            Paragraph paragraph = new Paragraph();

            string pattern = @"(<[^>]*>)|([ ,.!?;:\""\r\n]+)|(\[[^\]]*\])";
            string[] words = Regex.Split(inputText, pattern);
            foreach (string word in words)
            {
                Run run = new Run(word);
                if (flaggedWords.Contains(word))
                {
                    run.FontWeight = FontWeights.Bold;
                    run.Foreground = Brushes.Red;
                }

                paragraph.Inlines.Add(run);

            }

            newDocument.Blocks.Add(paragraph);
            return newDocument;
        }

        /// <summary>
        /// Extracts the content inside the <body> tag of an HTML string.
        /// </summary>
        /// <param name="html">The HTML string to extract content from.</param>
        /// <returns>The content inside the <body> tag, or an empty string if no content is found.</returns>
        public string ExtractBodyContent(string html)
        {
            Match match = Regex.Match(html, @"<body>(.*?)</body>", RegexOptions.Singleline);
            return match.Success ? match.Groups[1].Value : string.Empty;
        }
        /// <summary>
        /// Replaces the content inside the <body> tag of an HTML string with new content.
        /// </summary>
        /// <param name="html">The original HTML string.</param>
        /// <param name="newBodyContent">The new content to place inside the <body> tag.</param>
        /// <returns>The updated HTML string with the replaced content.</returns>
        public string ReplaceBodyContent(string html, string newBodyContent)
        {
            return Regex.Replace(html, @"<body>(.*?)</body>", $"<body>{newBodyContent}</body>", RegexOptions.Singleline);
        }
        /// <summary>
        /// Retrieves plain text from a FlowDocument.
        /// </summary>
        /// <param name="document">The FlowDocument to extract text from.</param>
        /// <returns>The extracted plain text, or an empty string if the document is null.</returns>
        public string RetrieveTextFromDocument(FlowDocument document)
        {

            if (document == null)
            {
                return string.Empty;
            }

            string plainText = new TextRange(document.ContentStart, document.ContentEnd).Text;
            return plainText.TrimEnd();
        }

    }
}
