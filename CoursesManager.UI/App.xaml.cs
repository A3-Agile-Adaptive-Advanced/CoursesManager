using System.Collections.ObjectModel;
using System.Windows;
using CoursesManager.MVVM.Dialogs;
using CoursesManager.MVVM.Env;
using CoursesManager.MVVM.Mail.MailService;
using CoursesManager.MVVM.Navigation;
using CoursesManager.UI.ViewModels;
using CoursesManager.MVVM.Messages;
using CoursesManager.UI.DataAccess;
using CoursesManager.UI.Dialogs.ViewModels;
using CoursesManager.UI.Dialogs.Windows;
using CoursesManager.UI.Messages;
using CoursesManager.UI.Dialogs.ResultTypes;
using CoursesManager.UI.Factory;
using CoursesManager.UI.Helpers;
using CoursesManager.UI.Models;
using CoursesManager.UI.Repositories.LocationRepository;
using CoursesManager.UI.Repositories.RegistrationRepository;
using CoursesManager.UI.Repositories.StudentRepository;
using CoursesManager.UI.Views.Students;
using CoursesManager.UI.ViewModels.Students;
using CoursesManager.UI.Repositories.AddressRepository;
using CoursesManager.UI.Repositories.CourseRepository;
using CoursesManager.UI.Service;
using CoursesManager.UI.ViewModels.Courses;
using CoursesManager.UI.ViewModels.Mailing;
using CoursesManager.UI.Repositories.TemplateRepository;
using CoursesManager.UI.Mailing;
using CoursesManager.UI.Repositories.CertificateRepository;

namespace CoursesManager.UI;

public partial class App : Application
{
    //This is a temporary static class that will hold all the data that is used in the application.
    //This is a temporary solution until we have a database.
    public static ObservableCollection<Student> Students { get; private set; }

    public static ObservableCollection<Course> Courses { get; private set; }
    public static ObservableCollection<Location> Locations { get; private set; }
    public static ObservableCollection<Registration> Registrations { get; private set; }

    public static ICourseRepository CourseRepository { get; private set; }
    public static ILocationRepository LocationRepository { get; private set; }
    public static IRegistrationRepository RegistrationRepository { get; private set; }
    public static IStudentRepository StudentRepository { get; private set; }
    public static IAddressRepository AddressRepository { get; private set; }

    public static ITemplateRepository TemplateRepository { get; private set; }
    public static ICertificateRepository CertificateRepository { get; private set; }

    public StudentRegistrationCourseAggregator StudentRegistrationCourseAggregator { get; set; }

    public static INavigationService NavigationService { get; set; } = new NavigationService();
    public static IMessageBroker MessageBroker { get; set; } = new MessageBroker();
    public static IDialogService DialogService { get; set; } = new DialogService();
    public static IMailService MailService { get; set; } = new MailService();
    public static IMailProvider MailProvider { get; set; } = new MailProvider(MailService, TemplateRepository, CertificateRepository);

    public static IConfigurationService ConfigurationService { get; set; } = new ConfigurationService(new EncryptionService("SmpjQzNZMWdCdW11bTlER2owdFRzOHIzQUpWWmhYQ0U="));

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Initialize Dummy Data
        //SetupDummyDataTemporary();
        //InitializeRepositories();

        // Set MainWindow's DataContext
        MainWindow mw = new()
        {
            DataContext = new MainWindowViewModel(NavigationService, MessageBroker)
        };
        GlobalCache.Instance.Put("MainViewModel", mw.DataContext, true);

        var repositoryFactory = new RepositoryFactory();

        // Create the ViewModelFactory
        var viewModelFactory = new ViewModelFactory(
            repositoryFactory,
            MessageBroker,
            DialogService,
            ConfigurationService,
            MailProvider);

        // Register ViewModel

        RegisterViewModels(viewModelFactory);

        // Register Dialogs
        RegisterDialogs();

        // Subscribe to Application Close Messages
        MessageBroker.Subscribe<ApplicationCloseRequestedMessage, App>(ApplicationCloseRequestedHandler, this);

        var startupManager = new StartupManager(ConfigurationService, NavigationService, MessageBroker, repositoryFactory);
        startupManager.CheckConfigurationOnStartup();

        mw.Show();
    }

    private void InitializeRepositories()
    {
        //AddressRepository = new AddressRepository(new AddressDataAccess());
        //LocationRepository = new LocationRepository(new LocationDataAccess(), AddressRepository);
        //CourseRepository = new CourseRepository(new CourseDataAccess(), LocationRepository);
        //StudentRepository = new StudentRepository(new StudentDataAccess(), AddressRepository);
        //RegistrationRepository = new RegistrationRepository(new RegistrationDataAccess());
        //TemplateRepository = new TemplateRepository(new TemplateDataAccess());
        //StudentRegistrationCourseAggregator = new StudentRegistrationCourseAggregator(CourseRepository, StudentRepository, RegistrationRepository);
    }

    private void RegisterDialogs()
    {
        DialogService.RegisterDialog<ConfirmationDialogViewModel, YesNoDialogWindow, DialogResultType>((initial) => new ConfirmationDialogViewModel(initial));
        DialogService.RegisterDialog<NotifyDialogViewModel, ConfirmationDialogWindow, DialogResultType>((initial) => new NotifyDialogViewModel(initial));
        DialogService.RegisterDialog<ErrorDialogViewModel, ErrorDialogWindow, DialogResultType>((initial) => new ErrorDialogViewModel(initial));

        // Register Dialogs using the factory
        DialogService.RegisterDialog<EditStudentViewModel, EditStudentPopup, Student>(
            student => new EditStudentViewModel(
                StudentRepository,
                CourseRepository,
                RegistrationRepository,
                DialogService,
                student));

        DialogService.RegisterDialog<AddStudentViewModel, AddStudentPopup, Student>(
            (student) => new AddStudentViewModel(
                student,
                StudentRepository,
                CourseRepository,
                RegistrationRepository,
                DialogService
            ));

        DialogService.RegisterDialog<TemplatePreviewDialogViewModel, TemplatePreviewDialogWindow, DialogResultType>((initial) => new TemplatePreviewDialogViewModel(initial, MessageBroker));

        DialogService.RegisterDialog<CourseDialogViewModel, CourseDialogWindow, Course>((initial) => new CourseDialogViewModel(CourseRepository, DialogService, LocationRepository, initial));
    }

    private void RegisterViewModels(ViewModelFactory viewModelFactory)
    {
        INavigationService.RegisterViewModelFactory((nav) => viewModelFactory.CreateViewModel<StudentManagerViewModel>(nav));

        INavigationService.RegisterViewModelFactoryWithParameters((param, nav) => viewModelFactory.CreateViewModel<StudentDetailViewModel>(nav, param));

        INavigationService.RegisterViewModelFactory((nav) => viewModelFactory.CreateViewModel<CoursesManagerViewModel>(nav));

        INavigationService.RegisterViewModelFactory((nav) => viewModelFactory.CreateViewModel<CourseOverViewViewModel>(nav));

        INavigationService.RegisterViewModelFactory(() => viewModelFactory.CreateViewModel<ConfigurationViewModel>());

        INavigationService.RegisterViewModelFactory((nav) => viewModelFactory.CreateViewModel<EditMailTemplatesViewModel>(nav));
    }

    /// <summary>
    /// Enables us to close the app by sending a message through the messenger.
    /// </summary>
    /// <param name="obj"></param>
    private static async void ApplicationCloseRequestedHandler(ApplicationCloseRequestedMessage obj)
    {
        MessageBroker.Publish(new OverlayActivationMessage(true));

        var result = await DialogService.ShowDialogAsync<ConfirmationDialogViewModel, DialogResultType>(new DialogResultType
        {
            DialogTitle = "CoursesManager",
            DialogText = "Wil je de app afsluiten?"
        });

        MessageBroker.Publish(new OverlayActivationMessage(false));

        if (result.Data is null) return;

        if (result.Data.Result) Application.Current.Shutdown();
    }
}