using CoursesManager.MVVM.Data;
using CoursesManager.MVVM.Dialogs;
using CoursesManager.MVVM.Messages;
using CoursesManager.MVVM.Navigation;
using CoursesManager.UI.Mailing;
using CoursesManager.UI.Models;
using CoursesManager.UI.Service;
using CoursesManager.UI.Service.PlaceholderService;
using CoursesManager.UI.Service.TextHandlerService;
using CoursesManager.UI.ViewModels;
using CoursesManager.UI.ViewModels.Courses;
using CoursesManager.UI.ViewModels.Mailing;
using CoursesManager.UI.ViewModels.Students;

namespace CoursesManager.UI.Factory
{
    public class ViewModelFactory
    {
        private readonly IMessageBroker _messageBroker;
        private readonly IDialogService _dialogService;
        private readonly IConfigurationService _configurationService;
        private readonly IMailProvider _mailProvider;
        private readonly IPlaceholderService _placeholderService;
        private readonly ITextHandlerService _textHandlerService;

        private readonly RepositoryFactory _repositoryFactory;

        public ViewModelFactory(
            RepositoryFactory repositoryFactory,
            IMessageBroker messageBroker,
            IDialogService dialogService,
            IConfigurationService configurationService,
            IPlaceholderService placeholderService,
            ITextHandlerService textHandlerService,
            IMailProvider mailProvider)
        {
            _repositoryFactory = repositoryFactory;
            _messageBroker = messageBroker;
            _dialogService = dialogService;
            _configurationService = configurationService;
            _placeholderService = placeholderService;
            _textHandlerService = textHandlerService;
            _mailProvider = mailProvider;
        }

        public T CreateViewModel<T>(object? parameter = null) where T : class
        {
            return typeof(T) switch
            {
                _ => throw new ArgumentException($"Unknown ViewModel type: {typeof(T)}")
            };
        }

        // If the viewmodel wants a navigation service put it in here
        public T CreateViewModel<T>(INavigationService navigationService, object? parameter = null)
            where T : ViewModelWithNavigation
        {
            return typeof(T) switch
            {
                Type vmType when vmType == typeof(CalendarViewModel) =>
                    new CalendarViewModel(navigationService, _repositoryFactory.CourseRepository) as T,
                Type vmType when vmType == typeof(CourseOverViewViewModel) =>
                    new CourseOverViewViewModel(
                        _repositoryFactory.StudentRepository,
                        _repositoryFactory.RegistrationRepository,
                        _repositoryFactory.CourseRepository,
                        _dialogService,
                        _messageBroker,
                        navigationService,
                        _mailProvider) as T,
                Type vmType when vmType == typeof(StudentManagerViewModel) =>
                    new StudentManagerViewModel(_dialogService, _repositoryFactory.StudentRepository, _repositoryFactory.CourseRepository,
                        _repositoryFactory.RegistrationRepository, _messageBroker, navigationService) as T,
                Type vmType when vmType == typeof(StudentDetailViewModel) =>
                    new StudentDetailViewModel(
                        _dialogService,
                        _messageBroker,
                        _repositoryFactory.RegistrationRepository,
                        navigationService,
                        parameter as Student) as T,
                Type vmType when vmType == typeof(CoursesManagerViewModel) =>
                    new CoursesManagerViewModel(_repositoryFactory.CourseRepository, _messageBroker, _dialogService, navigationService) as T,
                Type vmType when vmType == typeof(EditMailTemplatesViewModel) =>
                    new EditMailTemplatesViewModel(_repositoryFactory.TemplateRepository, _dialogService, _messageBroker, _placeholderService, _textHandlerService, navigationService) as T,

                Type vmType when vmType == typeof(ConfigurationViewModel) =>
                    new ConfigurationViewModel(_configurationService, _messageBroker, navigationService) as T,

                // Add other view model cases here...
                _ => throw new ArgumentException($"Unknown ViewModel type: {typeof(T)}")
            };
        }
    }
}