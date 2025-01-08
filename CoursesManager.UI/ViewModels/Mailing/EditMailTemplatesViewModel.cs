using CoursesManager.MVVM.Commands;
using CoursesManager.MVVM.Data;
using CoursesManager.MVVM.Dialogs;
using CoursesManager.MVVM.Exceptions;
using CoursesManager.MVVM.Messages;
using CoursesManager.MVVM.Navigation;
using CoursesManager.UI.Dialogs.ResultTypes;
using CoursesManager.UI.Dialogs.ViewModels;
using CoursesManager.UI.Enums;
using CoursesManager.UI.Messages;
using CoursesManager.UI.Models;
using CoursesManager.UI.Repositories.TemplateRepository;
using CoursesManager.UI.Service.PlaceholderService;
using CoursesManager.UI.Service.TextHandlerService;
using System.Windows.Documents;
using System.Windows.Input;

namespace CoursesManager.UI.ViewModels.Mailing
{
    public class EditMailTemplatesViewModel : ViewModelWithNavigation
    {
        #region Services
        private readonly IMessageBroker _messageBroker;
        private readonly IDialogService _dialogService;
        private readonly ITemplateRepository _templateRepository;
        private readonly IPlaceholderService _placeholderService;
        private readonly ITextHandlerService _textHandlerService;
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

        public EditMailTemplatesViewModel(ITemplateRepository templateRepository, IDialogService dialogService, IMessageBroker messageBroker, IPlaceholderService placeholderService, ITextHandlerService textHandlerService, INavigationService navigationService) : base(navigationService)
        {
            _templateRepository = templateRepository;
            _navigationService = navigationService;
            _messageBroker = messageBroker;
            _dialogService = dialogService;
            _placeholderService = placeholderService;
            _textHandlerService = textHandlerService;

            ShowMailCommand = new RelayCommand<string>(LoadTemplateHtml, s => s != null);
            PreviewPageCommand = new AsyncRelayCommand(OpenTemplateViewer);
            SaveTemplateCommand = new RelayCommand(ValidateAndSaveTemplate);

            VisibleText = new FlowDocument(new Paragraph(new Run(ExtractTemplateBodyText("CertificateMail", null))));
        }

        public void ValidateAndSaveTemplate()
        {
            string convertedText = _textHandlerService.RetrieveTextFromDocument(VisibleText);
            List<string> invalidPlaceholders = _placeholderService.ValidatePlaceholders(convertedText);

            if (invalidPlaceholders.Count != 0)
            {
                VisibleText = _textHandlerService.UpdateVisibleTextWithErrorFormatting(convertedText, invalidPlaceholders);

                string invalidPlaceholdersMessage = string.Join(", ", invalidPlaceholders);
                _messageBroker.Publish(new ToastNotificationMessage(true,
                    $"1 of meerdere placeholders zijn incorrect: {invalidPlaceholdersMessage}.\nDruk op toets '[' om alle geldige opties in te zien.", ToastType.Warning));
                return;
            }

            string updatedHtmlString = _textHandlerService.ReplaceBodyContent(CurrentTemplate.HtmlString, convertedText);
            CurrentTemplate.HtmlString = updatedHtmlString;

            UpdateTemplateInDatabase(CurrentTemplate);
        }
        public async Task OpenTemplateViewer()
        {
            await ExecuteWithOverlayAsync(_messageBroker, async () =>
            {
                var dialogResult = await _dialogService.ShowDialogAsync<TemplatePreviewDialogViewModel, DialogResultType>(new DialogResultType
                {
                    DialogTitle = CurrentTemplate.Name,
                    DialogText = _textHandlerService.ReplaceBodyContent(CurrentTemplate.HtmlString, _textHandlerService.RetrieveTextFromDocument(VisibleText)),
                });
            });
        }

        #region Helper methods

        private void UpdateTemplateInDatabase(Template template)
        {
            try
            {
                _templateRepository.Update(template);

                InitializeVisibleText(null, CurrentTemplate);

                _messageBroker.Publish(new ToastNotificationMessage(true,
                    "Template opgeslagen", ToastType.Confirmation));
            }
            catch (DataAccessException)
            {
                ShowUserGeneralErrorNotification();
            }
        }
        private string ExtractTemplateBodyText(string? templateName, Template? template)
        {
            if (!(template == null && string.IsNullOrEmpty(templateName)))
            {
                try
                {
                    CurrentTemplate = template ?? _templateRepository.GetTemplateByName(templateName);
                    return _textHandlerService.ExtractBodyContent(CurrentTemplate.HtmlString);
                }
                catch (DataAccessException)
                {
                    ShowUserGeneralErrorNotification();
                    return string.Empty;
                }
            }
            return string.Empty;
        }

        private void LoadTemplateHtml(string name)
        {
            InitializeVisibleText(name, null);
        }

        private void ShowUserGeneralErrorNotification()
        {
            _messageBroker.Publish(new ToastNotificationMessage(true,
                "Er is een fout opgetreden, neem contact op met de systeembeheerder.", ToastType.Error));
        }

        private void InitializeVisibleText(string? templateName = null, Template? template = null)
        {
            string templateText = ExtractTemplateBodyText(templateName, template);
            VisibleText = new FlowDocument(new Paragraph(new Run(templateText)));
        }

        #endregion

        #region Method(s) for external use
        //This gives the codebehind the abillity to show feedback to the user. this method is only used by the codebehind of EditMailTemplates.
        public void ShowErrorMessage(string errorMessage, ToastType type)
        {
            _messageBroker.Publish(new ToastNotificationMessage(true,
                errorMessage,
                type));
        }
        #endregion

    }
}
