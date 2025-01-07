using CoursesManager.UI.Models;
using CoursesManager.UI.Repositories.Base;

namespace CoursesManager.UI.Repositories.CourseRepository;

public interface ICourseRepository : IRepository<Course>
{
    List<Course> GetAllBetweenDates(DateTime start, DateTime end);
}