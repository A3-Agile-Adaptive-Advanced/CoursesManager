namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var studentDataAccess = new StudentDataAccess();
            var courseDataAccess = new CourseDataAccess();
            var registrationDataAccess = new RegistrationDataAccess();

            Lazy<IRegistrationRepo> registrationRepoLazy = null;
            Lazy<IStudentRepo> studentRepoLazy = null;
            Lazy<ICourseRepo> courseRepoLazy = null;

            var registrationRepo = new RegistrationRepo(
                new Lazy<ICourseRepo>(() => courseRepoLazy.Value),
                new Lazy<IStudentRepo>(() => studentRepoLazy.Value),
                new RegistrationDataAccess());

            var studentRepo = new StudentRepo(
                new Lazy<IRegistrationRepo>(() => registrationRepoLazy.Value),
                new StudentDataAccess());

            var courseRepo = new CourseRepo(
                new Lazy<IRegistrationRepo>(() => registrationRepoLazy.Value),
                new CourseDataAccess());

            // Assign the Lazy instances
            registrationRepoLazy = new Lazy<IRegistrationRepo>(() => registrationRepo);
            studentRepoLazy = new Lazy<IStudentRepo>(() => studentRepo);
            courseRepoLazy = new Lazy<ICourseRepo>(() => courseRepo);


            var registrations = registrationRepo.GetAllWithCourseAndStudent();
            var students = studentRepo.GetAll();
            var courses = courseRepo.GetAll();



            var simpleStudentRepo = new SimpleStudentRepo(studentDataAccess);
            var simpleCourseRepo = new SimpleCourseRepo(courseDataAccess);
            var simpleRegistrationRepo = new SimpleRegistrationRepo(registrationDataAccess);

            var service = new CourseStudentService(simpleCourseRepo, simpleStudentRepo, simpleRegistrationRepo);

            var test1 = service.GetAllCoursesWithRegistrations();
            var test2 = service.GetAllStudentsWithRegistrations();
            var test3 = service.GetAllRegistrationsWithDetails();
        }
    }

    public class CourseDataAccess : IDataAccess<Course>
    {
        public List<Course> GetAll()
        {
            return
            [
                new(1),
                new(2),
                new(3),
                new(4),
                new(5)
            ];
        }
    }

    public class StudentDataAccess : IDataAccess<Student>
    {
        public List<Student> GetAll()
        {
            return
            [
                new(1),
                new(2),
                new(3),
                new(4),
                new(5),
                new(6),
                new(7),
                new(8),
                new(9),
                new(10),
                new(11),
                new(12),
                new(13),
                new(14),
                new(15)
            ];
        }
    }

    public class RegistrationDataAccess : IDataAccess<Registration>
    {
        public List<Registration> GetAll()
        {
            return
            [
                new(1, 1, 1),
                new(2, 1, 2),
                new(3, 1, 3),
                new(4, 2, 4),
                new(5, 3, 5),
                new(6, 4, 1),
                new(7, 4, 2),
                new(8, 5, 3),
                new(9, 5, 4),
                new(10, 6, 5),
                new(11, 7, 1),
                new(12, 8, 2),
                new(13, 9, 3),
                new(14, 10, 4),
                new(15, 10, 5),
                new(16, 11, 1),
                new(17, 12, 2),
                new(18, 13, 3),
                new(19, 14, 4),
                new(20, 15, 5),
                new(21, 15, 1)
            ];
        }
    }

    public class Course
    {
        public Course()
        {
            
        }

        public Course(int id)
        {
            Id = id;
        }

        public int Id { get; set; }
        public List<Registration>? Registrations { get; set; }
    }

    public class Student
    {
        public Student()
        {
            
        }

        public Student(int id)
        {
            Id = id;
        }

        public int Id { get; set; }
        public List<Registration>? Registrations { get; set; }
    }

    public class Registration
    {
        public Registration()
        {
            
        }

        public Registration(int id, int studentId, int courseId)
        {
            Id = id;
            StudentId = studentId;
            CourseId = courseId;
        }

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
        List<Registration>? GetRegistrationsForCourseWithStudent(Course course);
        List<Registration>? GetRegistrationsForStudentWithCourse(Student student);
    }

    public interface IDataAccess<T>
    {
        List<T> GetAll();
    }

    public class StudentRepo : IStudentRepo
    {
        private readonly Lazy<IRegistrationRepo> _registrationRepo;
        private readonly IDataAccess<Student> _studentDataAccess;
        private List<Student> _students;

        public StudentRepo(Lazy<IRegistrationRepo> registrationRepo, IDataAccess<Student> studentDataAccess)
        {
            _registrationRepo = registrationRepo;
            _studentDataAccess = studentDataAccess;
            _students = new();
        }

        private void EnsureStudentsLoaded()
        {
            if (_students.Count > 0) return;

            _students = GetAll();
        }

        public List<Student> GetAll()
        {
            if (_students.Count > 0) return _students;

            _students = _studentDataAccess.GetAll();
            _students.ForEach(s => s.Registrations = _registrationRepo.Value.GetRegistrationsForStudentWithCourse(s));
            return _students;
        }

        public Student? GetStudentForRegistration(Registration registration)
        {
            EnsureStudentsLoaded();

            return _students.FirstOrDefault(s => s.Id == registration.StudentId);
        }
    }

    public class CourseRepo : ICourseRepo
    {
        private readonly Lazy<IRegistrationRepo> _registrationRepo;
        private readonly IDataAccess<Course> _courseDataAccess;
        private List<Course> _courses;

        public CourseRepo(Lazy<IRegistrationRepo> registrationRepo, IDataAccess<Course> courseDataAccess)
        {
            _registrationRepo = registrationRepo;
            _courseDataAccess = courseDataAccess;
            _courses = new();
        }

        private void EnsureCoursesLoaded()
        {
            if (_courses.Count > 0) return;

            _courses = GetAll();
        }

        public List<Course> GetAll()
        {
            if (_courses.Count > 0) return _courses;

            _courses = _courseDataAccess.GetAll();
            _courses.ForEach(c => c.Registrations = _registrationRepo.Value.GetRegistrationsForCourseWithStudent(c));
            return _courses;
        }

        public Course? GetCourseForRegistration(Registration registration)
        {
            EnsureCoursesLoaded();

            return _courses.FirstOrDefault(c => c.Id == registration.CourseId);
        }
    }

    public class RegistrationRepo : IRegistrationRepo
    {
        private readonly Lazy<ICourseRepo> _courseRepo;
        private readonly Lazy<IStudentRepo> _studentRepo;
        private readonly IDataAccess<Registration> _registrationDataAccess;
        private List<Registration> _registrations;

        public RegistrationRepo(Lazy<ICourseRepo> coursRepo, Lazy<IStudentRepo> studentRepo, IDataAccess<Registration> registrationDataAccess)
        {
            _courseRepo = coursRepo;
            _studentRepo = studentRepo;
            _registrationDataAccess = registrationDataAccess;
            _registrations = new();
        }

        public List<Registration> GetAll()
        {
            if (_registrations.Count > 0) return _registrations;

            _registrations = _registrationDataAccess.GetAll();
            return _registrations;
        }

        private void EnsureRegistrationsLoaded()
        {
            if (_registrations.Count > 0) return;

            _registrations = GetAll();
        }

        public List<Registration>? GetRegistrationsForCourseWithStudent(Course course)
        {
            EnsureRegistrationsLoaded();

            var res = _registrations.Where(r => r.CourseId == course.Id).ToList();
            
            res.ForEach(r =>
            {
                r.Student = _studentRepo.Value.GetStudentForRegistration(r);
                r.Course = course;
            });

            return res;
        }

        public List<Registration>? GetRegistrationsForStudentWithCourse(Student student)
        {
            EnsureRegistrationsLoaded();

            var res = _registrations.Where(r => r.StudentId == student.Id).ToList();

            res.ForEach(r =>
            {
                r.Course = _courseRepo.Value.GetCourseForRegistration(r);
                r.Student = student;
            });

            return res;
        }

        public List<Registration> GetAllWithCourseAndStudent()
        {
            EnsureRegistrationsLoaded();

            _registrations.ForEach(r =>
            {
                r.Student = _studentRepo.Value.GetStudentForRegistration(r);
                r.Course = _courseRepo.Value.GetCourseForRegistration(r);
            });

            return _registrations;
        }
    }


    public class SimpleCourseRepo : ICourseRepo
    {
        private readonly IDataAccess<Course> _courseDataAccess;
        private List<Course> _courses;

        public SimpleCourseRepo(IDataAccess<Course> courseDataAccess)
        {
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
            throw new NotImplementedException();
        }
    }

    public class SimpleStudentRepo : IStudentRepo
    {
        private readonly IDataAccess<Student> _studentDataAccess;
        private List<Student> _students;

        public SimpleStudentRepo(IDataAccess<Student> studentDataAccess)
        {
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
            throw new NotImplementedException();
        }
    }

    public class SimpleRegistrationRepo : IRegistrationRepo
    {
        private readonly IDataAccess<Registration> _registrationDataAccess;
        private List<Registration> _registrations;

        public SimpleRegistrationRepo(IDataAccess<Registration> registrationDataAccess)
        {
            _registrationDataAccess = registrationDataAccess;
            _registrations = new();
            GetAll();
        }

        public List<Registration> GetAll()
        {
            if (_registrations.Count > 0) return _registrations;

            _registrations = _registrationDataAccess.GetAll();
            return _registrations;
        }

        public List<Registration>? GetRegistrationsForCourseWithStudent(Course course)
        {
            throw new NotImplementedException();
        }

        public List<Registration>? GetRegistrationsForStudentWithCourse(Student student)
        {
            throw new NotImplementedException();
        }
    }

    public class CourseStudentService
    {
        private readonly ICourseRepo _courseRepo;
        private readonly IStudentRepo _studentRepo;
        private readonly IRegistrationRepo _registrationRepo;

        public CourseStudentService(
            ICourseRepo courseRepo,
            IStudentRepo studentRepo,
            IRegistrationRepo registrationRepo)
        {
            _courseRepo = courseRepo;
            _studentRepo = studentRepo;
            _registrationRepo = registrationRepo;
        }

        public List<Registration> GetAllRegistrationsWithDetails()
        {
            var registrations = _registrationRepo.GetAll();
            var students = _studentRepo.GetAll();
            var courses = _courseRepo.GetAll();

            foreach (var registration in registrations)
            {
                registration.Student = students.FirstOrDefault(s => s.Id == registration.StudentId);
                registration.Course = courses.FirstOrDefault(c => c.Id == registration.CourseId);

                if (registration.Student is not null)
                {
                    registration.Student.Registrations ??= [];
                    registration.Student.Registrations.Add(registration);
                }

                if (registration.Course is not null)
                {
                    registration.Course.Registrations ??= [];
                    registration.Course.Registrations.Add(registration);
                }
            }

            return registrations;
        }

        public List<Course> GetAllCoursesWithRegistrations()
        {
            GetAllRegistrationsWithDetails();
            return _courseRepo.GetAll();
        }

        public List<Student> GetAllStudentsWithRegistrations()
        {
            GetAllRegistrationsWithDetails();
            return _studentRepo.GetAll();
        }
    }
}
