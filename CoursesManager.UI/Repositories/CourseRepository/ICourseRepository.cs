using CoursesManager.UI.Models;
using CoursesManager.UI.Repositories.Base;
using System.Collections.ObjectModel;

namespace CoursesManager.UI.Repositories.CourseRepository;

public interface ICourseRepository : IRepository<Course>
{
    ObservableCollection<Course> GetAllBetweenDates(DateTime start, DateTime end);
}