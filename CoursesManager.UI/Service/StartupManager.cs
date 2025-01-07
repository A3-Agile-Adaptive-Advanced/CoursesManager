using CoursesManager.MVVM.Messages;
using CoursesManager.MVVM.Navigation;
using CoursesManager.UI.Enums;
using CoursesManager.UI.Factory;
using CoursesManager.UI.Messages;
using CoursesManager.UI.ViewModels;
using CoursesManager.UI.ViewModels.Courses;

namespace CoursesManager.UI.Service
{
    public class StartupManager
    {
        private readonly IConfigurationService _configurationService;

        private readonly INavigationService _navigationService;

        private readonly IMessageBroker _messageBroker;
        private readonly RepositoryFactory _repositoryFactory;

        public StartupManager(IConfigurationService configurationService, INavigationService navigationService,
            IMessageBroker messageBroker)
        {
            _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _messageBroker = messageBroker;
        }

        public void CheckConfigurationOnStartup()
        {
            try
            {
                if (!_configurationService.ValidateSettings())
                {
                    Console.WriteLine("Configuratie is ongeldig. start de configuratie-instellingen");
                    INavigationService.CanNavigate = false;
                    OpenConfigurationUi();
                }
                else
                {
                    Console.WriteLine("Configuratie is geldig. Applicatie kan doorgaan.");
                    NavigateToStartPage();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fout tijdens configuratiecontrole: {ex.Message}");
                OpenConfigurationUi();
            }
        }

        private void OpenConfigurationUi()
        {
            Console.WriteLine("Configuratie-UI wordt geopend...");
            _navigationService.NavigateTo<ConfigurationViewModel>();
        }

        private void NavigateToStartPage()
        {
            _messageBroker.Publish(new ToastNotificationMessage(
                true,
                "Navigeren naar de startpagina...",
                ToastType.Confirmation));

            _navigationService.NavigateTo<CoursesManagerViewModel>();
        }
    }
}