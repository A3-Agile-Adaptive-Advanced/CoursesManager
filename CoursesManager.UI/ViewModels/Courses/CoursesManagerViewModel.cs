using System.Collections.ObjectModel;
using System.Windows.Input;
using CoursesManager.MVVM.Commands;
using CoursesManager.MVVM.Data;
using CoursesManager.MVVM.Messages;
using CoursesManager.MVVM.Navigation;
using CoursesManager.UI.Models;
using CoursesManager.MVVM.Dialogs;
using CoursesManager.UI.Repositories.CourseRepository;
using CoursesManager.UI.ViewModels.Courses;

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
        private bool _isPayed;
        public bool IsPayed
        {
            get => _isPayed;
            set => SetProperty(ref _isPayed, value);
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

        private async Task FilterRecordsAsync()
        {
            string searchTerm = string.IsNullOrWhiteSpace(SearchText)
                ? String.Empty
                : SearchText.Trim().Replace(" ", "").ToLower();

            var now = DateTime.Now;
            var twoWeeksFromNow = now.AddDays(14);

            var filtered = await Task.Run(() =>
                Courses
                .Where(course =>
                    // Filter courses based on the search string and ensure they are active
                    (string.IsNullOrEmpty(searchTerm)
                        || course.GenerateFilterString().Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase))
                    && course.IsActive == IsToggled
                )
                .OrderBy(course =>
                    // Put courses that start within 2 weeks and are not paid at the top
                    (course.StartDate >= now && course.StartDate <= twoWeeksFromNow && !course.IsPayed) ? 0 : 1
                )
                .ThenBy(course => course.StartDate)
                .ToList()
            );

            UpdateFilteredCourses(filtered);
        }

        private void UpdateFilteredCourses(IEnumerable<Course> filteredCourses)
        {
            FilteredCourses.Clear();
            foreach (var course in filteredCourses) FilteredCourses.Add(course);
        }

        private void OpenCourseOptions(Course parameter)
        {
            GlobalCache.Instance.Put("Opened Course", parameter, false);
            _navigationService.NavigateTo<CourseOverViewViewModel>();
        }

        private async void OpenCourseDialog()
        {
            await ExecuteWithOverlayAsync(_messageBroker, async () =>
            {
                var dialogResult = await _dialogService.ShowDialogAsync<CourseDialogViewModel, Course>();

                if (dialogResult != null && dialogResult.Data != null && dialogResult.Outcome == DialogOutcome.Success)
                {
                    LoadCourses();
                }
            });
        }
    }
}