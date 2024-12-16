using CoursesManager.MVVM.Data;
using CoursesManager.MVVM.Navigation;
using CoursesManager.UI.Mailing;
using System.Windows.Input;
using CoursesManager.UI.Models;
using CoursesManager.UI.Repositories.TemplateRepository;
using CoursesManager.UI.Helpers;
using CoursesManager.MVVM.Commands;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using System.Windows.Controls;
using System.Windows;

namespace CoursesManager.UI.ViewModels.Mailing
{
    public class EditMailTemplatesViewModel : ViewModelWithNavigation
    {

        private readonly IMailProvider _mailProvider;
        private readonly TemplateRepository _templateRepository = new ();
        private readonly HtmlParser _htmlParser = new();

        private FlowDocument _visibleText;
        public FlowDocument VisibleText
        {
            get => _visibleText;
            set => SetProperty(ref _visibleText, value);
        }

        public ICommand BoldCommand { get; }
        public ICommand ItalicCommand { get; }
        public ICommand UnderlineCommand { get; }
        public ICommand EnterKeyCommand { get; }

        public ICommand ShowMailCommand { get; }


        public EditMailTemplatesViewModel(INavigationService navigationService) : base(navigationService)
        {
            _navigationService = navigationService;
            _mailProvider = new MailProvider();



            BoldCommand = new RelayCommand(BoldText);
            ItalicCommand = new RelayCommand(ItalicText);
            UnderlineCommand = new RelayCommand(UnderlineText);

            VisibleText = new FlowDocument(new Paragraph(new Run(GetTemplateText("CertificateMail"))));
            ShowMailCommand = new RelayCommand<string>(SwitchHtmls, s => s != null);

        }

        public string GetTemplateText(string templateName)
        {
            string templateText = string.Empty;
            Template originalTemplate = _templateRepository.GetTemplateByName(templateName);
            Match match = Regex.Match(originalTemplate.HtmlString, @"<body.*?>(.*?)</body>", RegexOptions.Singleline);
            string bodyContent = match.Groups[1].Value;
            templateText = _htmlParser.ConvertFromHtml(bodyContent);

            return templateText;
        }

        private void SwitchHtmls(string Html)
        {
            VisibleText = new FlowDocument(new Paragraph(new Run(GetTemplateText(Html))));
        }

        private void BoldText()
        {
            var selection = GetRichTextBoxSelection();
            if (selection != null)
            {
                var currentFontWeight = selection.GetPropertyValue(TextElement.FontWeightProperty);

                if (currentFontWeight is FontWeight fontWeight && fontWeight == FontWeights.Bold)
                {
                    selection.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);
                }
                else
                {
                    selection.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
                }
            }
        }

        private void ItalicText()
        {
            var selection = GetRichTextBoxSelection();
            if (selection != null)
            {
                var currentFontStyle = selection.GetPropertyValue(TextElement.FontStyleProperty);

                if (currentFontStyle is FontStyle fontStyle && fontStyle == FontStyles.Italic)
                {
                    selection.ApplyPropertyValue(TextElement.FontStyleProperty, FontStyles.Normal);
                }
                else
                {
                    selection.ApplyPropertyValue(TextElement.FontStyleProperty, FontStyles.Italic);
                }
            }
        }

        private void UnderlineText()
        {
            var selection = GetRichTextBoxSelection();
            if (selection != null)
            {
                var currentTextDecorations = selection.GetPropertyValue(Inline.TextDecorationsProperty);

                if (currentTextDecorations is TextDecorationCollection textDecorations && textDecorations.Contains(TextDecorations.Underline[0]))
                {
                    selection.ApplyPropertyValue(Inline.TextDecorationsProperty, null);
                }
                else
                {
                    selection.ApplyPropertyValue(Inline.TextDecorationsProperty, TextDecorations.Underline);
                }
            }
        }

        // A helper method to get the current selection in the RichTextBox
        private TextSelection GetRichTextBoxSelection()
        {
            var richTextBox = Application.Current.Windows[0].FindName("richTextBox") as RichTextBox;
            return richTextBox?.Selection;
        }
    }
}
