﻿using CoursesManager.MVVM.Data;
using CoursesManager.MVVM.Dialogs;
using CoursesManager.MVVM.Messages;
using CoursesManager.MVVM.Navigation;
using CoursesManager.UI.Mailing;
using CoursesManager.UI.Models;
using CoursesManager.UI.Repositories.AddressRepository;
using CoursesManager.UI.Repositories.CourseRepository;
using CoursesManager.UI.Repositories.LocationRepository;
using CoursesManager.UI.Repositories.RegistrationRepository;
using CoursesManager.UI.Repositories.StudentRepository;
using CoursesManager.UI.Repositories.TemplateRepository;
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
        private readonly ICourseRepository _courseRepository;
        private readonly ILocationRepository _locationRepository;
        private readonly IRegistrationRepository _registrationRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IAddressRepository _addressRepository;
        private readonly ITemplateRepository _templateRepository;
        private readonly IMessageBroker _messageBroker;
        private readonly IDialogService _dialogService;
        private readonly IConfigurationService _configurationService;
        private readonly IMailProvider _mailProvider;
        private readonly IPlaceholderService _placeholderService;
        private readonly ITextHandlerService _textHandlerService;


        public ViewModelFactory(
            ICourseRepository courseRepository,
            ILocationRepository locationRepository,
            IRegistrationRepository registrationRepository,
            IStudentRepository studentRepository,
            IAddressRepository addressRepository,
            ITemplateRepository templateRepository,
            IMessageBroker messageBroker,
            IDialogService dialogService,
            IConfigurationService configurationService,
            IPlaceholderService placeholderService,
            ITextHandlerService textHandlerService,
            IMailProvider mailProvider)
        {
            _courseRepository = courseRepository;
            _locationRepository = locationRepository;
            _registrationRepository = registrationRepository;
            _studentRepository = studentRepository;
            _addressRepository = addressRepository;
            _templateRepository = templateRepository;
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
                Type vmType when vmType == typeof(CourseOverViewViewModel) =>
                    new CourseOverViewViewModel(_studentRepository, _registrationRepository, _courseRepository, _dialogService, _messageBroker, navigationService, _mailProvider) as T,
                Type vmType when vmType == typeof(StudentManagerViewModel) =>
                    new StudentManagerViewModel(_dialogService, _studentRepository, _courseRepository,
                        _registrationRepository, _messageBroker, navigationService) as T,
                Type vmType when vmType == typeof(StudentDetailViewModel) =>
                    new StudentDetailViewModel(
                        _dialogService,
                        _messageBroker,
                        _registrationRepository,
                        navigationService,
                        parameter as Student) as T,
                Type vmType when vmType == typeof(CoursesManagerViewModel) =>
                    new CoursesManagerViewModel(_courseRepository, _messageBroker, _dialogService, navigationService) as T,
                Type vmType when vmType == typeof(EditMailTemplatesViewModel) =>
                    new EditMailTemplatesViewModel(_templateRepository, _dialogService, _messageBroker, _placeholderService, _textHandlerService, navigationService) as T,

                Type vmType when vmType == typeof(ConfigurationViewModel) =>
                    new ConfigurationViewModel(_configurationService, _messageBroker, navigationService) as T,

                // Add other view model cases here...
                _ => throw new ArgumentException($"Unknown ViewModel type: {typeof(T)}")
            };
        }
    }
}