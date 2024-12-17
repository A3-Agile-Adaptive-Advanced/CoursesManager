
using CoursesManager.MVVM.Navigation;
using CoursesManager.UI.ViewModels;

namespace CoursesManager.UI.Service
{
    public class StartupManager
    {
        private readonly IConfigurationService _configurationService;

        private readonly INavigationService _navigationService;

        public StartupManager(IConfigurationService configurationService, INavigationService navigationService)
        {
            _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

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
                    _navigationService.NavigateTo<CoursesManagerViewModel>();
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
    }
}

