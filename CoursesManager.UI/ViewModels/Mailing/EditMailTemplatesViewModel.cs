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
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using CoursesManager.MVVM.Mail.MailService;
using CoursesManager.UI.Enums;
using MySql.Data.MySqlClient;
using CoursesManager.MVVM.Exceptions;
using CoursesManager.UI.Messages;

namespace CoursesManager.UI.ViewModels.Mailing
{
    public class EditMailTemplatesViewModel : ViewModelWithNavigation
    {
        #region Services
        private readonly IMessageBroker _messageBroker;
        private readonly IDialogService _dialogService;
        private readonly ITemplateRepository _templateRepository;
        #endregion
        #region Attributes
        private FlowDocument _visibleText;
        public FlowDocument VisibleText
        {
            get => _visibleText;
            set => SetProperty(ref _visibleText, value);
        }

        private Template _currentTemplate;
        public Template CurrentTemplate
        {
            get => _currentTemplate;
            set => SetProperty(ref _currentTemplate, value);
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
            _messageBroker = messageBroker;
            _dialogService = dialogService;

            ShowMailCommand = new RelayCommand<string>(SwitchHtmls, s => s != null);
            PreviewPageCommand = new RelayCommand(OpenTemplateViewer);
            SaveTemplateCommand = new RelayCommand(SaveTemplate);

            VisibleText = new FlowDocument(new Paragraph(new Run(GetTemplateText("CertificateMail", null))));
        }

        public void SaveTemplate()
        {
            string convertedText = GetPlainTextFromFlowDocument(VisibleText);
            List<string> invalidPlaceholders = ValidatePlaceholders(convertedText);

            if (invalidPlaceholders.Count != 0)
            {
                ReUploadTextWithErrorFormatting(convertedText, invalidPlaceholders);
                _messageBroker.Publish(new ToastNotificationMessage(true,
                    "1 of meerdere placeholders zijn incorrect. Druk op toets '[' om alle geldige opties in te zien.", ToastType.Warning, false));
                return;
            }

            string updatedHtmlString = UpdateTemplateBody(convertedText);
            CurrentTemplate.HtmlString = updatedHtmlString;

            UpdateTemplate(CurrentTemplate);
        }
        public async void OpenTemplateViewer()
        {
            await ExecuteWithOverlayAsync(_messageBroker, async () =>
            {
                var dialogResult = await _dialogService.ShowDialogAsync<TemplatePreviewDialogViewModel, DialogResultType>(new DialogResultType
                {
                    DialogTitle = CurrentTemplate.Name,
                    DialogText = UpdateTemplateBody(GetPlainTextFromFlowDocument(VisibleText)),
                });
            });
        }
        //This gives the codebehind the abillity to show feedback to the user. this method is only used by the codebehind of EditMailTemplates.
        public void ShowErrorMessage(string errorMessage, ToastType type)
        {
            _messageBroker.Publish(new ToastNotificationMessage(true,
                errorMessage,
                type, false));
        }

        #region Helper methods

        private void UpdateTemplate(Template template)
        {
            try
            {
                _templateRepository.Update(template);
                VisibleText = new FlowDocument(new Paragraph(new Run(GetTemplateText(null, CurrentTemplate))));
                _messageBroker.Publish(new ToastNotificationMessage(true,
                    "Template opgeslagen", ToastType.Confirmation, false));
            }
            catch (DataAccessException)
            {
                _messageBroker.Publish(new ToastNotificationMessage(true,
                    "Er is een fout opgetreden, template niet opgeslagen in de database",
                    ToastType.Error, false));
            }
        }
        private string GetTemplateText(string? templateName, Template? template)
        {
            string templateText = string.Empty;
            if (!(template == null && string.IsNullOrEmpty(templateName)))
            {
                try
                {
                    CurrentTemplate = template ?? _templateRepository.GetTemplateByName(templateName);
                }
                catch (DataAccessException)
                {
                    _messageBroker.Publish(new ToastNotificationMessage(true,
                        "Er is een fout opgetreden, template kon niet worden opgehaald uit de database",
                        ToastType.Error, false));
                }
                Match match = Regex.Match(CurrentTemplate.HtmlString, @"<body>(.*?)</body>", RegexOptions.Singleline);
                string bodyContent = match.Groups[1].Value;
                templateText = bodyContent;
            }

            return templateText;
        }
        private string UpdateTemplateBody(string updatedBodyContent)
        {
            string tempString = CurrentTemplate.HtmlString;
            string updatedTemplate = Regex.Replace(
                tempString,
                @"<body>(.*?)</body>",
                $"<body>{updatedBodyContent}</body>",
                RegexOptions.Singleline
            );

            return updatedTemplate;
        }

        private void SwitchHtmls(string Html)
        {
            VisibleText = new FlowDocument(new Paragraph(new Run(GetTemplateText(Html, null))));
        }

        private string GetPlainTextFromFlowDocument(FlowDocument document)
        {

            if (document == null)
            {
                _messageBroker.Publish(new ToastNotificationMessage(true,
                    "Er is een onverwachte fout opgetreden, neem contact op met de systeembeheerder",
                    ToastType.Error, false));
                return string.Empty;
            }

            string plainText = new TextRange(document.ContentStart, document.ContentEnd).Text;
            return plainText.TrimEnd();
        }

        private List<string> ValidatePlaceholders(string htmlString)
        {
            var invalidPlaceholders = new List<string>();
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

            var regex = new Regex(@"\[(.*?)\]");
            var matches = regex.Matches(htmlString);

            foreach (Match match in matches)
            {
                var placeholder = match.Value;

                if (!placeholders.Contains(placeholder))
                {
                    invalidPlaceholders.Add(placeholder);
                }
            }

            return invalidPlaceholders;
        }

        private void ReUploadTextWithErrorFormatting(string inputText, List<string> flaggedWords)
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

            VisibleText = newDocument;
        }

        #endregion

    }
}
