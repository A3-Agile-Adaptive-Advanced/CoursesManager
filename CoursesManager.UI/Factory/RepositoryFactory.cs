using CoursesManager.UI.DataAccess;
using CoursesManager.UI.Repositories.AddressRepository;
using CoursesManager.UI.Repositories.CertificateRepository;
using CoursesManager.UI.Repositories.CourseRepository;
using CoursesManager.UI.Repositories.LocationRepository;
using CoursesManager.UI.Repositories.RegistrationRepository;
using CoursesManager.UI.Repositories.StudentRepository;
using CoursesManager.UI.Repositories.TemplateRepository;

namespace CoursesManager.UI.Factory;

public class RepositoryFactory
{
    public AddressRepository AddressRepository => new AddressRepository(new AddressDataAccess());

    public LocationRepository LocationRepository => new LocationRepository(new LocationDataAccess(), AddressRepository);

    public CourseRepository CourseRepository => new CourseRepository(new CourseDataAccess(), LocationRepository);

    public StudentRepository StudentRepository => new StudentRepository(new StudentDataAccess(), AddressRepository);

    public RegistrationRepository RegistrationRepository => new RegistrationRepository(new RegistrationDataAccess());

    public TemplateRepository TemplateRepository => new TemplateRepository(new TemplateDataAccess());

    public CertificateRepository CertificateRepository = new CertificateRepository(new CertificateDataAccess());

    public StudentRegistrationCourseAggregator StudentRegistrationCourseAggregator => new StudentRegistrationCourseAggregator(RegistrationRepository, CourseRepository, StudentRepository);
}

public class StudentRegistrationCourseAggregator(IRegistrationRepository registrationRepository, ICourseRepository courseRepository, IStudentRepository studentRepository)
{
    public void Load()
    {
        var registrations = registrationRepository.GetAll();
        var courses = courseRepository.GetAll();
        var students = studentRepository.GetAll();

        foreach (var registration in registrations)
        {
            registration.Student = students.FirstOrDefault(s => s.Id == registration.StudentId);
            registration.Course = courses.FirstOrDefault(c => c.Id == registration.CourseId);

            if (registration.Student is not null)
            {
                registration.Student.Registrations ??= [];

                if (!registration.Student.Registrations.Contains(registration))
                {
                    registration.Student.Registrations.Add(registration);
                }
            }

            if (registration.Course is not null)
            {
                registration.Course.Registrations ??= [];

                if (!registration.Course.Registrations.Contains(registration))
                {
                    registration.Course.Registrations.Add(registration);
                }
            }
        }
    }
}