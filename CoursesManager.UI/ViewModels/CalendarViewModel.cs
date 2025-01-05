using CoursesManager.MVVM.Commands;
using CoursesManager.MVVM.Data;
using CoursesManager.MVVM.Navigation;
using CoursesManager.UI.Models;
using CoursesManager.UI.Repositories.CourseRepository;
using CoursesManager.UI.Views.Controls.CursusCalendar;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CoursesManager.UI.ViewModels
{
    public class CalendarViewModel : ViewModelWithNavigation
    {
        private ICourseRepository _courseRepository;

        public ICommand OnCalendarDateChangedCommand { get; }
        public ICommand OnDaySelectedCommand { get; }

        public List<Course> CoursesBetweenDates { get; private set; }

        public CalendarViewModel(INavigationService navigationService, ICourseRepository courseRepository) : base(navigationService)
        {
            ViewTitle = "Cursus Agenda";

            _courseRepository = courseRepository;

            OnCalendarDateChangedCommand = new RelayCommand<CalendarLayout>((calendarLayout) =>
            {
                var daysInCurrentView = calendarLayout.DaysInCurrentView;
                var courses = _courseRepository.GetAllBetweenDates(daysInCurrentView.First().Date, daysInCurrentView.Last().Date);

                CoursesBetweenDates = new List<Course>(courses);
            });

            OnDaySelectedCommand = new RelayCommand(() =>
            {
                LogUtil.Info("select");
            });
        }
    }
}
