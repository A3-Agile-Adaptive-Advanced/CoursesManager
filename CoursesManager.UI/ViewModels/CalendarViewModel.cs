using CoursesManager.MVVM.Commands;
using CoursesManager.MVVM.Data;
using CoursesManager.MVVM.Navigation;
using CoursesManager.UI.Models;
using CoursesManager.UI.Repositories.CourseRepository;
using CoursesManager.UI.ViewModels.Courses;
using CoursesManager.UI.Views.Controls.CoursesCalendar;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CoursesManager.UI.ViewModels
{
    public class CalendarViewModel : ViewModelWithNavigation
    {
        private ICourseRepository _courseRepository;
        private ObservableCollection<Course> _coursesBetweenDates;
        private ObservableCollection<Course> _coursesForSelectedDay;

        public ICommand OnCalendarDateChangedCommand { get; }
        public ICommand OnDaySelectedCommand { get; }
        public ICommand CourseOptionCommand { get; }

        public ObservableCollection<Course> CoursesBetweenDates
        {
            get => _coursesBetweenDates;
            set => SetProperty(ref _coursesBetweenDates, value);
        }

        public ObservableCollection<Course> CoursesForSelectedDay
        {
            get => _coursesForSelectedDay;
            set => SetProperty(ref _coursesForSelectedDay, value);
        }

        public bool IsPopoverOpen { get; set; }

        public CalendarViewModel(INavigationService navigationService, ICourseRepository courseRepository) : base(navigationService)
        {
            ViewTitle = "Cursus Agenda";

            _courseRepository = courseRepository;

            CourseOptionCommand = new RelayCommand<Course>(OpenCourse);
            CoursesBetweenDates = new ObservableCollection<Course>(_courseRepository.GetAll());
            CoursesForSelectedDay = new ObservableCollection<Course>(_courseRepository.GetAllBetweenDates(DateTime.Today, DateTime.Today));

            OnCalendarDateChangedCommand = new RelayCommand<CalendarLayout>(UpdateCoursesCalendarView);
            OnDaySelectedCommand = new RelayCommand<CalendarDay>(UpdateCoursesForSelectedDay);
        }

        private void UpdateCoursesCalendarView(CalendarLayout calendarLayout)
        {
            ObservableCollection<CalendarDay> daysInCurrentView = calendarLayout.DaysInCurrentView;
            ObservableCollection<Course> courses = _courseRepository.GetAllBetweenDates(daysInCurrentView.First().Date, daysInCurrentView.Last().Date);

            CoursesBetweenDates.Clear();
            foreach (Course course in courses)
                CoursesBetweenDates.Add(course);
        }

        private void UpdateCoursesForSelectedDay(CalendarDay calendarDay)
        {
            ObservableCollection<Course> courses = _courseRepository.GetAllBetweenDates(calendarDay.Date, calendarDay.Date);

            CoursesForSelectedDay.Clear();
            foreach (Course course in courses)
                CoursesForSelectedDay.Add(course);
        }

        private void OpenCourse(Course? parameter)
        {
            GlobalCache.Instance.Put("Opened Course", parameter, false);
            _navigationService.NavigateTo<CourseOverViewViewModel>();
        }
    }
}
