using CoursesManager.MVVM.Commands;
using CoursesManager.MVVM.Data;
using CoursesManager.MVVM.Navigation;
using CoursesManager.UI.Models;
using CoursesManager.UI.Repositories.CourseRepository;
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

        public CalendarViewModel(INavigationService navigationService, ICourseRepository courseRepository) : base(navigationService)
        {
            ViewTitle = "Cursus Agenda";

            _courseRepository = courseRepository;

            CoursesBetweenDates = new ObservableCollection<Course>(_courseRepository.GetAll());
            CoursesForSelectedDay = new();

            OnCalendarDateChangedCommand = new RelayCommand<CalendarLayout>((calendarLayout) =>
            {
                ObservableCollection<CalendarDay> daysInCurrentView = calendarLayout.DaysInCurrentView;
                List<Course> courses = _courseRepository.GetAllBetweenDates(daysInCurrentView.First().Date, daysInCurrentView.Last().Date);

                CoursesBetweenDates.Clear();
                foreach (Course course in courses)
                    CoursesBetweenDates.Add(course);
            });

            OnDaySelectedCommand = new RelayCommand<CalendarDay>((calendarDay) =>
            {
                List<Course> courses = _courseRepository.GetAllBetweenDates(calendarDay.Date, calendarDay.Date);

                CoursesForSelectedDay.Clear();
                foreach (Course course in courses)
                    CoursesForSelectedDay.Add(course);
            });
        }
    }
}
