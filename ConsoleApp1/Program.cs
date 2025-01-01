namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
        }
    }

    public class Course
    {
        public int Id { get; set; }
        public Registration? Registration { get; set; }
    }

    public class Student
    {
        public int Id { get; set; }
        public Registration? Registration { get; set; }
    }

    public class Registration
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public Student? Student { get; set; }
        public Course? Course { get; set; }
    }

    public interface IStudentRepo
    {
        List<Student> GetAll();
        Student? GetStudentForRegistration(Registration registration);
    }

    public interface ICourseRepo
    {
        List<Course> GetAll();
        Course? GetCourseForRegistration(Registration registration);
    }

    public interface IRegistrationRepo
    {
        List<Registration> GetAll();
    }

    public interface IDataAccess<T>
    {
        List<T> GetAll();
    }

    public class StudentRepo : IStudentRepo
    {
        private readonly IRegistrationRepo _registrationRepo;
        private readonly IDataAccess<Student> _studentDataAccess;
        private List<Student> _students;

        public StudentRepo(IRegistrationRepo registrationRepo, IDataAccess<Student> studentDataAccess)
        {
            _registrationRepo = registrationRepo;
            _studentDataAccess = studentDataAccess;
            _students = new();
            GetAll();
        }

        public List<Student> GetAll()
        {
            if (_students.Count > 0) return _students;

            _students = _studentDataAccess.GetAll();
            return _students;
        }

        public Student? GetStudentForRegistration(Registration registration)
        {
            return _students.FirstOrDefault(s => s.Id == registration.StudentId);
        }
    }

    public class CourseRepo : ICourseRepo
    {
        private readonly IRegistrationRepo _registrationRepo;
        private readonly IDataAccess<Course> _courseDataAccess;
        private List<Course> _courses;

        public CourseRepo(IRegistrationRepo registrationRepo, IDataAccess<Course> courseDataAccess)
        {
            _registrationRepo = registrationRepo;
            _courseDataAccess = courseDataAccess;
            _courses = new();
            GetAll();
        }

        public List<Course> GetAll()
        {
            if (_courses.Count > 0) return _courses;

            _courses = _courseDataAccess.GetAll();
            return _courses;
        }

        public Course? GetCourseForRegistration(Registration registration)
        {
            return _courses.FirstOrDefault(c => c.Id == registration.CourseId);
        }
    }

    public class RegistrationRepo : IRegistrationRepo
    {
        private readonly ICourseRepo _courseRepo;
        private readonly IStudentRepo _studentRepo;
        private readonly IDataAccess<Registration> _registrationDataAccess;
        private List<Registration> _registrations;

        public RegistrationRepo(ICourseRepo coursRepo, IStudentRepo studentRepo, IDataAccess<Registration> registrationDataAccess)
        {
            _courseRepo = coursRepo;
            _studentRepo = studentRepo;
            _registrationDataAccess = registrationDataAccess;
            GetAll();
        }

        public List<Registration> GetAll()
        {
            if (_registrations.Count > 0) return _registrations;

            _registrations = _registrationDataAccess.GetAll();
            return _registrations;
        }

        public List<Registration> GetAllWithCourseAndStudent()
        {
            _registrations.ForEach(r =>
            {
                r.Student = _studentRepo.GetStudentForRegistration(r);
                r.Course = _courseRepo.GetCourseForRegistration(r);
            });

            return _registrations;
        }
    }



}
