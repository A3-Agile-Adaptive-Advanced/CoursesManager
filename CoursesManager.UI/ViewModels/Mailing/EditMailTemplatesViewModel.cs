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
using CoursesManager.MVVM.Dialogs;
using CoursesManager.UI.Dialogs.ResultTypes;
using CoursesManager.UI.Dialogs.ViewModels;
using CoursesManager.UI.ViewModels.Students;
using CoursesManager.MVVM.Messages;

namespace CoursesManager.UI.ViewModels.Mailing
{
    public class EditMailTemplatesViewModel : ViewModelWithNavigation
    {
        #region Services
        private readonly IMessageBroker _messageBroker;
        private readonly IDialogService _dialogService;
        private readonly IMailProvider _mailProvider;
        private readonly TemplateRepository _templateRepository = new();
        private readonly HtmlParser _htmlParser = new();
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
        public ICommand SaveTemplateCommand {  get; }
        #endregion

        public EditMailTemplatesViewModel(IDialogService dialogService, IMessageBroker messageBroker, INavigationService navigationService) : base(navigationService)
        {
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
            Match match = Regex.Match(Template.HtmlString, @"<body.*?>(.*?)</body>", RegexOptions.Singleline);
            string bodyContent = match.Groups[1].Value;
            templateText = bodyContent;

            return templateText;
        }

        public async void SaveTemplate()
        {

        }

        private string UpdateTemplateBody(string updatedBodyContent)
        {
            if (Template == null || string.IsNullOrEmpty(Template.HtmlString))
                throw new InvalidOperationException("No template is loaded.");
            string tempString = Template.HtmlString;
            string updatedTemplate = Regex.Replace(
                tempString,
                @"<body.*?>(.*?)</body>",
                $"<body>{updatedBodyContent}</body>",
                RegexOptions.Singleline
            );

            return updatedTemplate;
        }

        private void SwitchHtmls(string Html)
        {
            VisibleText = new FlowDocument(new Paragraph(new Run(GetTemplateText(Html))));
        }

        private string GetPlainTextFromFlowDocument(FlowDocument document)
        {
            if (document == null)
                return string.Empty;

            return UpdateTemplateBody(new TextRange(document.ContentStart, document.ContentEnd).Text);
        }

        public async void OpenTemplateViewer()
        {
            await ExecuteWithOverlayAsync(_messageBroker, async () =>
            {
                var dialogResult = await _dialogService.ShowDialogAsync<TemplatePreviewDialogViewModel, DialogResultType>(new DialogResultType
                {
                    DialogTitle = Template.Name,
                    DialogText = GetPlainTextFromFlowDocument(VisibleText),
                });
            });
        }
    }
}
