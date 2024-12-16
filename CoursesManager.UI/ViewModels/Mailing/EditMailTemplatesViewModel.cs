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
        public ICommand EnterKeyCommand { get; }

        public ICommand ShowMailCommand { get; }


        public EditMailTemplatesViewModel(INavigationService navigationService) : base(navigationService)
        {
            _navigationService = navigationService;
            _mailProvider = new MailProvider();

            VisibleText = new FlowDocument(new Paragraph(new Run(GetTemplateText("CertificateMail"))));
            ShowMailCommand = new RelayCommand<string>(SwitchHtmls, s => s != null);

        }

        public string GetTemplateText(string templateName)
        {
            string templateText = string.Empty;
            Template originalTemplate = _templateRepository.GetTemplateByName(templateName);
            Match match = Regex.Match(originalTemplate.HtmlString, @"<body.*?>(.*?)</body>", RegexOptions.Singleline);
            string bodyContent = match.Groups[1].Value;
            templateText = bodyContent;

            return templateText;
        }

        private void SwitchHtmls(string Html)
        {
            VisibleText = new FlowDocument(new Paragraph(new Run(GetTemplateText(Html))));
        }

    }
}
