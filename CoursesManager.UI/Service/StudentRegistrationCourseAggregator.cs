using System.Collections.ObjectModel;
using CoursesManager.UI.Factory;
using CoursesManager.UI.Models;

namespace CoursesManager.UI.Service;

public interface IStudentRegistrationCourseAggregator
{
    void AggregateFromCourses(ObservableCollection<Course> courses);

    void AggregateFromStudents(ObservableCollection<Student> students);

    void AggregateFromRegistratios(ObservableCollection<Registration> registrations);
}

public class StudentRegistrationCourseAggregator(RepositoryFactory repositoryFactory) : IStudentRegistrationCourseAggregator
{
    private bool _processedBefore = false;

    private bool ProcessedBefore()
    {
        var res = _processedBefore;
        _processedBefore = false;
        return res;
    }

    public void AggregateFromCourses(ObservableCollection<Course> courses)
    {
        if (ProcessedBefore()) return;

        var registrations = repositoryFactory.RegistrationRepository.GetAll();
        var students = repositoryFactory.StudentRepository.GetAll();
        Load(registrations, courses, students);
    }

    public void AggregateFromStudents(ObservableCollection<Student> students)
    {
        if (ProcessedBefore()) return;

        var registrations = repositoryFactory.RegistrationRepository.GetAll();
        var courses = repositoryFactory.CourseRepository.GetAll();
        Load(registrations, courses, students);
    }

    public void AggregateFromRegistratios(ObservableCollection<Registration> registrations)
    {
        if (ProcessedBefore()) return;

        var courses = repositoryFactory.CourseRepository.GetAll();
        var students = repositoryFactory.StudentRepository.GetAll();
        Load(registrations, courses, students);
    }

    private static void Load(ObservableCollection<Registration> registrations, ObservableCollection<Course> courses, ObservableCollection<Student> students)
    {
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