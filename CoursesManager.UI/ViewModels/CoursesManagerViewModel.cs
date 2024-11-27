using System.Collections.ObjectModel;
using System.Windows.Input;
using CoursesManager.MVVM.Commands;
using CoursesManager.MVVM.Data;
using CoursesManager.MVVM.Messages;
using CoursesManager.MVVM.Navigation;
using CoursesManager.UI.Messages;
using CoursesManager.UI.Models;
using CoursesManager.UI.ViewModels.Courses;
using CoursesManager.MVVM.Dialogs;
using CoursesManager.UI.ViewModels.Students;
using System.Diagnostics;
using CoursesManager.MVVM.Messages;
using CoursesManager.UI.Repositories.RegistrationRepository;
using CoursesManager.UI.Repositories.CourseRepository;

namespace CoursesManager.UI.ViewModels
{
    public class CoursesManagerViewModel : ViewModelWithNavigation
    {
        // Properties
        private readonly ICourseRepository _courseRepository;
        private readonly IDialogService _dialogService;
        private readonly IMessageBroker _messageBroker;

        private string _searchText = String.Empty;
        private bool _isToggled = true;

        // Getters and Setters
        public ICommand SearchCommand { get; }

        public ICommand ToggleCommand { get; }
        public ICommand AddCourseCommand { get; }
        public ICommand CourseOptionCommand { get; }

        private ObservableCollection<Course> _courses;

        public ObservableCollection<Course> Courses
        {
            get => _courses;
            private set => SetProperty(ref _courses, value);
        }

        private ObservableCollection<Course> _filteredCourses;

        public ObservableCollection<Course> FilteredCourses
        {
            get => _filteredCourses;
            private set => SetProperty(ref _filteredCourses, value);
        }

        public string SearchText
        {
            get => _searchText;
            set { if (SetProperty(ref _searchText, value)) _ = FilterRecordsAsync(); }
        }

        public bool IsToggled
        {
            get => _isToggled;
            set { if (SetProperty(ref _isToggled, value)) _ = FilterRecordsAsync(); }
        }

        // Constructor
        public CoursesManagerViewModel(ICourseRepository courseRepository, IMessageBroker messageBroker, IDialogService dialogService, INavigationService navigationService) : base(navigationService)
        {
            _courseRepository = courseRepository;
            _messageBroker = messageBroker;
            _dialogService = dialogService;
            
            _messageBroker.Subscribe<CoursesChangedMessage, CoursesManagerViewModel>(OnCoursesChangedMessage, this);

            ViewTitle = "Cursus beheer";
            _messageBroker = messageBroker;
            SearchCommand = new RelayCommand(() => _ = FilterRecordsAsync());
            ToggleCommand = new RelayCommand(() => _ = FilterRecordsAsync());
            CourseOptionCommand = new RelayCommand<Course>(OpenCourseOptions);
            AddCourseCommand = new RelayCommand(OpenCourseDialog);

            LoadCourses();
        }

        private void OnCoursesChangedMessage(CoursesChangedMessage obj)
        {
            LoadCourses();
        }

        private void LoadCourses()
        {
            Courses = new ObservableCollection<Course>(_courseRepository.GetAll());
            FilteredCourses = new ObservableCollection<Course>(Courses);

            FilterRecordsAsync();
        }

        // Methods
        private void OnSearchCommand() => FilterRecordsAsync();

        private void OnToggleCommand() => FilterRecordsAsync();

        private async Task FilterRecordsAsync()
        {
            string searchTerm = string.IsNullOrWhiteSpace(SearchText)
                ? String.Empty
                : SearchText.Trim().Replace(" ", "").ToLower();

            var filtered = await Task.Run(() =>
                Courses.Where(course =>
                    (string.IsNullOrEmpty(searchTerm)
                    || course.GenerateFilterString().ToLower().Contains(searchTerm))
                    && course.IsActive == IsToggled
                ).OrderBy(course => course.IsPayed)
                .ThenBy(course => course.StartDate)
                .ToList());

            UpdateFilteredCourses(filtered);
        }

        private void UpdateFilteredCourses(IEnumerable<Course> filteredCourses)
        {
            FilteredCourses.Clear();
            foreach (var course in filteredCourses) FilteredCourses.Add(course);
        }

        private void OpenCourseOptions(Course parameter)
        {
            GlobalCache.Instance.Put("Opened Course", parameter, true);
            _navigationService.NavigateTo<CourseOverViewViewModel>();
        }

        private async void OpenCourseDialog()
        {
            var dialogResult = await _dialogService.ShowDialogAsync<CourseDialogViewModel, Course>();

            if (dialogResult != null && dialogResult.Data != null && dialogResult.Outcome == DialogOutcome.Success)
            {
                LoadCourses();
            }
        }
    }
}