using CoursesManager.UI.Models;

namespace CoursesManager.UI.Repositories.RegistrationRepository;

public interface IRegistrationRepository : IRepository<Registration>
{
    List<Registration> GetAllRegistrationsByCourse(Course course);
    List<Registration> GetAllRegistrationsByStudent(Student student);
}