using System.Text.RegularExpressions;
using System.Windows.Documents;
using System.Windows.Media;

namespace CoursesManager.UI.Service.TextHandlerService
{
    public interface ITextHandlerService
    {
        public FlowDocument UpdateVisibleTextWithErrorFormatting(string inputText, List<string> flaggedWords);

        public string ExtractBodyContent(string html);

        public string ReplaceBodyContent(string html, string newBodyContent);

        public string RetrieveTextFromDocument(FlowDocument document);

    }
}
