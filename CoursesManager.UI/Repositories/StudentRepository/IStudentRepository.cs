using CoursesManager.UI.Models;
using CoursesManager.UI.Repositories.Base;

namespace CoursesManager.UI.Repositories.StudentRepository;

public interface IStudentRepository : IRepository<Student>
{
    List<Student> GetNotDeletedStudents();

    List<Student> GetDeletedStudents();
}