using CoursesManager.MVVM.Commands;
using CoursesManager.MVVM.Data;
using CoursesManager.MVVM.Dialogs;
using CoursesManager.MVVM.Messages;
using CoursesManager.MVVM.Navigation;
using CoursesManager.UI.Dialogs.ResultTypes;
using CoursesManager.UI.Dialogs.ViewModels;
using CoursesManager.UI.Mailing;
using CoursesManager.UI.Models;
using CoursesManager.UI.Repositories.TemplateRepository;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using System.Windows.Input;

namespace CoursesManager.UI.ViewModels.Mailing
{
    public class EditMailTemplatesViewModel : ViewModelWithNavigation
    {
        #region Services
        private readonly IMessageBroker _messageBroker;
        private readonly IDialogService _dialogService;
        private readonly IMailProvider _mailProvider;
        private readonly ITemplateRepository _templateRepository;
        #endregion
        #region Attributes
        private FlowDocument _visibleText;
        public FlowDocument VisibleText
        {
            get => _visibleText;
            set => SetProperty(ref _visibleText, value);
        }

        private Template _template;
        public Template Template
        {
            get => _template;
            set => SetProperty(ref _template, value);
        }
        #endregion
        #region Commands
        public ICommand ShowMailCommand { get; }
        public ICommand PreviewPageCommand { get; }
        public ICommand SaveTemplateCommand { get; }
        #endregion

        public EditMailTemplatesViewModel(ITemplateRepository templateRepository, IDialogService dialogService, IMessageBroker messageBroker, INavigationService navigationService) : base(navigationService)
        {
            _templateRepository = templateRepository;
            _navigationService = navigationService;
            _mailProvider = new MailProvider();
            _messageBroker = messageBroker;
            _dialogService = dialogService;


            VisibleText = new FlowDocument(new Paragraph(new Run(GetTemplateText("CertificateMail"))));
            ShowMailCommand = new RelayCommand<string>(SwitchHtmls, s => s != null);
            PreviewPageCommand = new RelayCommand(OpenTemplateViewer);
            SaveTemplateCommand = new RelayCommand(SaveTemplate);
        }

        public string GetTemplateText(string templateName)
        {
            string templateText = string.Empty;
            Template = _templateRepository.GetTemplateByName(templateName);
            Match match = Regex.Match(Template.HtmlString, @"<body>(.*?)</body>", RegexOptions.Singleline);
            string bodyContent = match.Groups[1].Value;
            templateText = bodyContent;
            return templateText;
        }

        public async void SaveTemplate()
        {
            string convertedText = GetPlainTextFromFlowDocument(VisibleText);
            string updatedHtmlString = UpdateTemplateBody(convertedText);

            Template.HtmlString = updatedHtmlString;
            try
            {
                _templateRepository.UpdateTemplate(Template);
            }
            catch (Exception ex)
            {
            }
        }

        private string UpdateTemplateBody(string updatedBodyContent)
        {
            string tempString = Template.HtmlString;
            string updatedTemplate = Regex.Replace(
                tempString,
                @"<body>(.*?)</body>",
                $"<body>{updatedBodyContent}</body>",
                RegexOptions.Singleline
            );

            return updatedTemplate;
        }
        public async void OpenTemplateViewer()
        {
            await ExecuteWithOverlayAsync(_messageBroker, async () =>
            {
                var dialogResult = await _dialogService.ShowDialogAsync<TemplatePreviewDialogViewModel, DialogResultType>(new DialogResultType
                {
                    DialogTitle = Template.Name,
                    DialogText = UpdateTemplateBody(GetPlainTextFromFlowDocument(VisibleText)),
                });
            });
        }

        private void SwitchHtmls(string Html)
        {
            VisibleText = new FlowDocument(new Paragraph(new Run(GetTemplateText(Html))));
        }

        private string GetPlainTextFromFlowDocument(FlowDocument document)
        {
            if (document == null)
                return string.Empty;

            return new TextRange(document.ContentStart, document.ContentEnd).Text;
        }

        private List<string> ValidatePlaceholders(string template, List<string> placeholders)
        {
            var invalidPlaceholders = new List<string>();

            // Regular expression to match text inside square brackets []
            var regex = new Regex(@"\[(.*?)\]");

            // Find matches in the template
            var matches = regex.Matches(template);

            foreach (Match match in matches)
            {
                var placeholder = match.Value; // Get the matched placeholder, including brackets

                // Check if it exists in the valid placeholders list
                if (!placeholders.Contains(placeholder))
                {
                    invalidPlaceholders.Add(placeholder); // Flag as invalid
                }
            }

            return invalidPlaceholders; // Return the list of invalid placeholders
        }

    }
}
