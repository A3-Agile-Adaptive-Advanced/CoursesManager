using CoursesManager.MVVM.Commands;
using CoursesManager.MVVM.Data;
using CoursesManager.MVVM.Navigation;
using CoursesManager.UI.Repositories.CourseRepository;
using System.Windows.Input;

namespace CoursesManager.UI.ViewModels
{
    public class CalendarViewModel : ViewModelWithNavigation
    {
        private ICourseRepository _courseRepository;

        public ICommand OnCalendarDateChangedCommand { get; }
        public ICommand OnDaySelectedCommand { get; }

        public CalendarViewModel(INavigationService navigationService, ICourseRepository courseRepository) : base(navigationService)
        {
            ViewTitle = "Cursus Agenda";

            _courseRepository = courseRepository;

            OnCalendarDateChangedCommand = new RelayCommand(() =>
            {
                LogUtil.Info("update");
            });

            OnDaySelectedCommand = new RelayCommand(() =>
            {
                LogUtil.Info("select");
            });
        }
    }
}
