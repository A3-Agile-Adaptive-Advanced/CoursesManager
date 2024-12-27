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
        private readonly ICourseRepository _courseRepository;
        private readonly IDialogService _dialogService;
        private readonly IMessageBroker _messageBroker;

        private string _searchTerm = String.Empty;
        private bool _isSwitchToggled = true;
        private ObservableCollection<Course> _courses;
        private ObservableCollection<Course> _filteredCourses;

        public ICommand SearchCommand { get; }
        public ICommand SwitchToggleCommand { get; }
        public ICommand AddCourseCommand { get; }
        public ICommand CourseOptionCommand { get; }

        public ObservableCollection<Course> Courses
        {
            get => _courses;
            set => SetProperty(ref _courses, value);
        }

        public ObservableCollection<Course> FilteredCourses
        {
            get => _filteredCourses;
            private set => SetProperty(ref _filteredCourses, value);
        }

        public string SearchTerm
        {
            get => _searchTerm;
            set { 
                if (SetProperty(ref _searchTerm, value)) 
                    _ = FilterCoursesAsync(); 
            }
        }

        public bool IsSwitchToggled
        {
            get => _isSwitchToggled;
            set { 
                if (SetProperty(ref _isSwitchToggled, value))
                    _ = FilterCoursesAsync(); 
            }
        }

        public CoursesManagerViewModel(
            ICourseRepository courseRepository,
            IMessageBroker messageBroker,
            IDialogService dialogService,
            INavigationService navigationService) : base(navigationService)
        {
            _courseRepository = courseRepository;
            _messageBroker = messageBroker;
            _dialogService = dialogService;

            _messageBroker.Subscribe<CoursesChangedMessage, CoursesManagerViewModel>(OnCoursesChangedMessage, this);

            ViewTitle = "Cursus beheer";

            SearchCommand = new RelayCommand(() => _ = FilterCoursesAsync());
            SwitchToggleCommand = new RelayCommand(() => _ = FilterCoursesAsync());
            CourseOptionCommand = new RelayCommand<Course>(OpenCourseOptions);
            AddCourseCommand = new RelayCommand(OpenCourseDialog);

            LoadCourses();
        }

        private void OnCoursesChangedMessage(CoursesChangedMessage obj) => LoadCourses();

        private void LoadCourses()
        {
            Courses = new ObservableCollection<Course>(_courseRepository.GetAll());
            FilteredCourses = new ObservableCollection<Course>(Courses);
            _ = FilterCoursesAsync();
        }

        private async Task FilterCoursesAsync()
        {
            string searchTerm = string.IsNullOrWhiteSpace(SearchTerm)
                ? String.Empty
                : SearchTerm.Trim().ToLower();

            var now = DateTime.Now;
            var twoWeeksFromNow = now.AddDays(14);

            var filteredCourses = await Task.Run(() =>
                Courses
                .Where(course =>
                    // Filter courses based on the search string and ensure they are active
                    (string.IsNullOrEmpty(searchTerm)
                        || course.GenerateFilterString().Contains(searchTerm, StringComparison.CurrentCultureIgnoreCase))
                    && course.IsActive == IsSwitchToggled
                )
                .OrderBy(course =>
                    // Put courses that start within 2 weeks and are not paid at the top
                    (course.StartDate >= now && course.StartDate <= twoWeeksFromNow && !course.IsPayed) ? 0 : 1
                )
                .ThenBy(course => course.StartDate)
                .ToList()
            );

            // Update the filtered courses collection
            FilteredCourses.Clear();
            foreach (var course in filteredCourses) 
                FilteredCourses.Add(course);
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