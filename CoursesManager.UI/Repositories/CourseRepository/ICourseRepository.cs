using CoursesManager.UI.Models;

namespace CoursesManager.UI.Repositories.CourseRepository;

public interface ICourseRepository : IRepository<Course>
{
    bool HasActiveRegistrations(Course course);
    void SetInactive(Course course);
}