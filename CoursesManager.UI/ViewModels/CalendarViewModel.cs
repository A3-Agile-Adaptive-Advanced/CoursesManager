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
        public ICommand OnCalendarDateChangedCommand { get; }
        public ICommand OnDaySelectedCommand { get; }
        public ICommand CourseOptionCommand { get; }

        public ObservableCollection<Course> CoursesBetweenDates { get; private set; }
        public ObservableCollection<Course> CoursesForSelectedDay { get; private set; }

        public bool IsPopoverOpen { get; set; }

        public CalendarViewModel(INavigationService navigationService, ICourseRepository courseRepository) : base(navigationService)
        {
            ViewTitle = "Cursus Agenda";
            _courseRepository = courseRepository;

            CourseOptionCommand = new RelayCommand<Course>(OpenCourse);
            CoursesBetweenDates = new ObservableCollection<Course>(_courseRepository.GetAll());
            CoursesForSelectedDay = new ObservableCollection<Course>(_courseRepository.GetAllBetweenDates(DateTime.Today, DateTime.Today));

            OnCalendarDateChangedCommand = new RelayCommand<CalendarLayout>(layout =>
            {
                DateTime start = layout.DaysInCurrentView.First().Date;
                DateTime end = layout.DaysInCurrentView.Last().Date;

                UpdateCourses(CoursesBetweenDates, start, end);
            });

            OnDaySelectedCommand = new RelayCommand<CalendarDay>(day => { UpdateCourses(CoursesForSelectedDay, day.Date, day.Date); });

            CourseOptionCommand = new RelayCommand<Course>(course =>
            {
                GlobalCache.Instance.Put("Opened Course", course, false);
                _navigationService.NavigateTo<CourseOverViewViewModel>();
            });
        }

        private void UpdateCourses(
            ObservableCollection<Course> targetCollection,
            DateTime start,
            DateTime end)
        {
            targetCollection.Clear();
            var newCourses = _courseRepository.GetAllBetweenDates(start, end);
            foreach (var course in newCourses)
            {
                targetCollection.Add(course);
            }
        }

        private void OpenCourse(Course? parameter)
        {
            GlobalCache.Instance.Put("Opened Course", parameter, false);
            _navigationService.NavigateTo<CourseOverViewViewModel>();
        }
    }
}
