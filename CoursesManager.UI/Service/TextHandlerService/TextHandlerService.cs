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
    public class TextHandlerService : ITextHandlerService
    {
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

        public string ExtractBodyContent(string html)
        {
            Match match = Regex.Match(html, @"<body>(.*?)</body>", RegexOptions.Singleline);
            return match.Success ? match.Groups[1].Value : string.Empty;
        }

        public string ReplaceBodyContent(string html, string newBodyContent)
        {
            return Regex.Replace(html, @"<body>(.*?)</body>", $"<body>{newBodyContent}</body>", RegexOptions.Singleline);
        }
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
