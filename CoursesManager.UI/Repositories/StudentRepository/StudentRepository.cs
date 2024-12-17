using CoursesManager.UI.Models;
using CoursesManager.UI.DataAccess;
using MySql.Data.MySqlClient;

namespace CoursesManager.UI.Repositories.StudentRepository;

public class StudentRepository : IStudentRepository
{
    private readonly StudentDataAccess _studentDataAccess = new();

    public List<Student> GetAll()
    {
        return _studentDataAccess.GetAll();
    }

    public List<Student> GetNotDeletedStudents()
    {
        return _studentDataAccess.GetNotDeletedStudents();
    }

    public List<Student> GetDeletedStudents()
    {
        return _studentDataAccess.GetDeletedStudents();
    }

    public List<Student> RefreshAll()
    {
        return _studentDataAccess.FetchAll();
    }

    public Student? GetById(int id)
    {
        return _studentDataAccess.GetById(id);
    }

    public void Add(Student student)
    {
        _studentDataAccess.Add(student);
    }

    public void Update(Student student)
    {
        _studentDataAccess.Update(student);
    }

    public void Delete(Student student)
    {
        _studentDataAccess.DeleteById(student.Id);
    }

    public void Delete(int id)
    {
        throw new NotImplementedException();
    }
}