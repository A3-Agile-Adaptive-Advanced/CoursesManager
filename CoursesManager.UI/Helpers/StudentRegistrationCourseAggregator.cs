using CoursesManager.UI.Repositories.CourseRepository;
using CoursesManager.UI.Repositories.RegistrationRepository;
using CoursesManager.UI.Repositories.StudentRepository;

namespace CoursesManager.UI.Helpers;

public class StudentRegistrationCourseAggregator(
    ICourseRepository courseRepository,
    IStudentRepository studentRepository,
    IRegistrationRepository registrationRepository)
{
    public void Aggregate()
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