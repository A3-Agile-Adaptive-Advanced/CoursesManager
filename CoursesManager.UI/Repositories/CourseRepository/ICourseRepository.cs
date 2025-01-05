using CoursesManager.UI.Models;

namespace CoursesManager.UI.Repositories.CourseRepository;

public interface ICourseRepository : IRepository<Course>
{
    List<Course> GetAllBetweenDates(DateTime start, DateTime end);
}