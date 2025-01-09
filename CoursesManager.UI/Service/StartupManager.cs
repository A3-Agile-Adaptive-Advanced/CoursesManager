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
                    INavigationService.CanNavigate = false;
                    OpenConfigurationUi();
                }
                else
                {
                    NavigateToStartPage();
                }
            }
            catch (Exception ex)
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