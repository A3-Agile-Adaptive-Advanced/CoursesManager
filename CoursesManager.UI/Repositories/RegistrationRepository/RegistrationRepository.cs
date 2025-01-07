using System.Collections.ObjectModel;
using CoursesManager.UI.DataAccess;
using CoursesManager.UI.Helpers;
using CoursesManager.UI.Models;
using CoursesManager.UI.Repositories.Base;
using CoursesManager.UI.Service;

namespace CoursesManager.UI.Repositories.RegistrationRepository;

public class RegistrationRepository : BaseRepository<Registration>, IRegistrationRepository
{
    private readonly RegistrationDataAccess _registrationDataAccess;
    private readonly IStudentRegistrationCourseAggregator _studentRegistrationCourseAggregator;

    private readonly ObservableCollection<Registration> _registrations;

    private const string Cachekey = "registrationsCache";

    private static readonly object SharedLock = new();

    public RegistrationRepository(RegistrationDataAccess registrationDataAccess,
        IStudentRegistrationCourseAggregator studentRegistrationCourseAggregator)
    {
        _registrationDataAccess = registrationDataAccess;
        _studentRegistrationCourseAggregator = studentRegistrationCourseAggregator;

        try
        {
            _registrations = GlobalCache.Instance.Get(Cachekey) as ObservableCollection<Registration> ?? SetupCache(Cachekey);
        }
        catch
        {
            _registrations = SetupCache(Cachekey);
        }
        finally
        {
            GetAll();
        }
    }

    public ObservableCollection<Registration> GetAll()
    {
        lock (SharedLock)
        {
            if (_registrations.Count == 0)
            {
                _registrationDataAccess.GetAll().ForEach(_registrations.Add);

                _studentRegistrationCourseAggregator.AggregateFromRegistratios(_registrations);
            }

            return _registrations;
        }
    }

    public Registration? GetById(int id)
    {
        lock (SharedLock)
        {
            var item = _registrations.FirstOrDefault(r => r.Id == id);

            if (item is null)
            {
                item = _registrationDataAccess.GetById(id);

                if (item is not null) _registrations.Add(item);
            }

            return item;
        }
    }

    public void Add(Registration registration)
    {
        lock (SharedLock)
        {
            _registrationDataAccess.Add(registration);
            _registrations.Add(registration);
        }
    }

    public void Update(Registration registration)
    {
        lock (_registrations)
        {
            ArgumentNullException.ThrowIfNull(registration);

            _registrationDataAccess.Update(registration);

            var item = GetById(registration.Id) ?? throw new InvalidOperationException($"Registration with id: {registration.Id} does not exist.");

            OverwriteItemInPlace(item, registration);
        }
    }

    public void Delete(Registration registration)
    {
        ArgumentNullException.ThrowIfNull(registration);

        Delete(registration.Id);
    }

    public void Delete(int id)
    {
        lock (SharedLock)
        {
            _registrationDataAccess.Delete(id);

            var item = GetById(id) ?? throw new InvalidOperationException($"Registration with id: {id} does not exist.");

            _registrations.Remove(item);
        }
    }

    public List<Registration> GetAllRegistrationsByCourse(Course course)
    {
        ArgumentNullException.ThrowIfNull(course);

        lock (SharedLock)
        {
            return _registrations.Where(r => r.CourseId == course.Id).ToList();
        }
    }

    public List<Registration> GetAllRegistrationsByStudent(Student student)
    {
        ArgumentNullException.ThrowIfNull(student);

        lock (SharedLock)
        {
            return _registrations.Where(r => r.StudentId == student.Id).ToList();
        }
    }
}
