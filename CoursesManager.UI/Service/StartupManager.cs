
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
                    INavigationService.CanNavigate = false;
                    OpenConfigurationUi();
                }
                else
                {
                    NavigateToStartPage();
                }
            }
            catch (Exception)
            {
                OpenConfigurationUi();
            }
        }

        private void OpenConfigurationUi()
        {

            _navigationService.NavigateTo<ConfigurationViewModel>();
        }

        private void NavigateToStartPage()
        {
            _navigationService.NavigateTo<CoursesManagerViewModel>();
        }
    }
}