using System.Collections.ObjectModel;
using System.Windows;
using CoursesManager.MVVM.Dialogs;
using CoursesManager.MVVM.Navigation;
using CoursesManager.UI.ViewModels;
using CoursesManager.MVVM.Messages;
using CoursesManager.UI.Dialogs.ViewModels;
using CoursesManager.UI.Dialogs.Windows;
using CoursesManager.UI.Messages;
using CoursesManager.UI.Dialogs.ResultTypes;
using CoursesManager.UI.Factory;
using CoursesManager.UI.Models;
using CoursesManager.UI.Models.CoursesManager.UI.Models;
using CoursesManager.UI.Models.Repositories.AddressRepository;
using CoursesManager.UI.Models.Repositories.CourseRepository;
using CoursesManager.UI.Models.Repositories.LocationRepository;
using CoursesManager.UI.Models.Repositories.RegistrationRepository;
using CoursesManager.UI.Models.Repositories.StudentRepository;
using CoursesManager.UI.Views.Students;
using CoursesManager.UI.ViewModels.Courses;

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


    public static IMessageBroker MessageBroker { get; set; } = new MessageBroker();
    public static IDialogService DialogService { get; set; } = new DialogService();
    public static INavigationService NavigationService { get; set; } = new NavigationService();

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Initialize Dummy Data
        SetupDummyDataTemporary();
        InitializeRepositories();

        // Set MainWindow's DataContext
        MainWindow mw = new()
        {
            DataContext = new MainWindowViewModel(NavigationService, MessageBroker)
        };
        GlobalCache.Instance.Put("MainViewModel", mw.DataContext, true);

        // Create the ViewModelFactory
        var viewModelFactory = new ViewModelFactory(
            CourseRepository,
            LocationRepository,
            RegistrationRepository,
            StudentRepository,
            AddressRepository,
            MessageBroker,
            DialogService);

        // Register ViewModel

        RegisterViewModels(viewModelFactory);

        // Register Dialogs
        RegisterDialogs();

        // Subscribe to Application Close Messages
        MessageBroker.Subscribe<ApplicationCloseRequestedMessage, App>(ApplicationCloseRequestedHandler, this);

        // Navigate to the Initial ViewModel
        NavigationService.NavigateTo<StudentManagerViewModel>();

        mw.Show();

        NavigationService.NavigateTo<StudentManagerViewModel>();
    }

    private void InitializeRepositories()
    {
        CourseRepository = new CourseRepository(Courses);
        StudentRepository = new StudentRepository(Students);
        RegistrationRepository = new RegistrationRepository(Registrations);
        AddressRepository = new AddressRepository();
        LocationRepository = new LocationRepository();
    }

    private static void SetupDummyDataTemporary()
    {
        //This is a temporary static class that will hold all the data that is used in the application.
        //This is a temporary solution until we have a database.
        Students = DummyDataGenerator.GenerateStudents(50);
        Courses = DummyDataGenerator.GenerateCourses(30);
        Locations = DummyDataGenerator.GenerateLocations(15);
        Registrations = DummyDataGenerator.GenerateRegistrations(50, 41);

        foreach (var registration in Registrations)
        {
            registration.Student = Students.FirstOrDefault(s => s.Id == registration.StudentID);
            registration.Course = Courses.FirstOrDefault(c => c.ID == registration.CourseID);
        }

        foreach (var course in Courses)
        {
            course.Location = Locations.FirstOrDefault(s => s.Id == course.LocationId);
        }
    }

    private void RegisterDialogs()
    {
        DialogService.RegisterDialog<ConfirmationDialogViewModel, YesNoDialogWindow, DialogResultType>((initial) => new ConfirmationDialogViewModel(initial));
        DialogService.RegisterDialog<NotifyDialogViewModel, ConfirmationDialogWindow, DialogResultType>((initial) => new NotifyDialogViewModel(initial));
    }

    private void RegisterViewModels(ViewModelFactory viewModelFactory)
    {
        // Register StudentManagerViewModel
        INavigationService.RegisterViewModelFactory(() => viewModelFactory.CreateViewModel<StudentManagerViewModel>());

        // Register CoursesManagerViewModel
        INavigationService.RegisterViewModelFactory((nav) => viewModelFactory.CreateViewModel<CoursesManagerViewModel>(nav));

        // Register CourseOverViewViewModel
        INavigationService.RegisterViewModelFactory(() => viewModelFactory.CreateViewModel<CourseOverViewViewModel>());

        // Register Dialogs using the factory
        DialogService.RegisterDialog<EditStudentViewModel, EditStudentPopup, Student>(
            student => new EditStudentViewModel(
                StudentRepository,
                CourseRepository,
                RegistrationRepository,
                DialogService,
                student));

        DialogService.RegisterDialog<AddStudentViewModel, AddStudentPopup, bool>(
            initial => viewModelFactory.CreateViewModel<AddStudentViewModel>());
    }

    /// <summary>
    /// Enables us to close the app by sending a message through the messenger.
    /// </summary>
    /// <param name="obj"></param>
    private static async void ApplicationCloseRequestedHandler(ApplicationCloseRequestedMessage obj)
    {
        var result = await DialogService.ShowDialogAsync<ConfirmationDialogViewModel, DialogResultType>(new DialogResultType
        {
            DialogTitle = "CoursesManager",
            DialogText = "Wil je de app afsluiten?"
        });

        if (result.Data is null) return;

        if (result.Data.Result) Application.Current.Shutdown();
    }
}