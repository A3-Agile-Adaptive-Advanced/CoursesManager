using CoursesManager.UI.DataAccess;
using CoursesManager.UI.Models;

namespace CoursesManager.UI.Repositories.RegistrationRepository;

public class RegistrationRepository : BaseRepository, IRegistrationRepository
{
    private readonly RegistrationDataAccess _registrationDataAccess;

    private readonly List<Registration> _allRegistrations;

    public RegistrationRepository()
    {
        _registrationDataAccess = new RegistrationDataAccess();
        _allRegistrations = new();
    }

    public List<Registration> GetAll()
    {
        if (_allRegistrations.Count == 0 || ShouldRefresh)
        {
            return RefreshAll();
        }

        return _allRegistrations;
    }

    public List<Registration> RefreshAll()
    {
        lock (_allRegistrations)
        {
            _lastUpdated = DateTime.Now;

            _allRegistrations.Clear();

            _allRegistrations.AddRange(_registrationDataAccess.GetAll());

            return _allRegistrations;
        }
    }

    public Registration? GetById(int id)
    {
        lock (_allRegistrations)
        {
            return _allRegistrations.FirstOrDefault(reg => reg.Id == id);
        }
    }

    public void Add(Registration registration)
    {
        lock (_allRegistrations)
        {
            ArgumentNullException.ThrowIfNull(registration);

            _registrationDataAccess.Add(registration);
            _allRegistrations.Add(registration);
        }
    }

    public void Update(Registration registration)
    {
        lock (_allRegistrations)
        {
            ArgumentNullException.ThrowIfNull(registration);

            _registrationDataAccess.Update(registration);

            if (!_allRegistrations.Contains(registration))
            {
                var existing = _allRegistrations.FirstOrDefault(reg => reg.Id == registration.Id);
                if (existing is not null)
                {
                    existing.CourseId = registration.CourseId;
                    existing.Course = registration.Course;
                    existing.StudentId = registration.StudentId;
                    existing.Student = registration.Student;
                    existing.RegistrationDate = registration.RegistrationDate;
                    existing.PaymentStatus = registration.PaymentStatus;
                    existing.IsActive = registration.IsActive;
                    existing.IsAchieved = registration.IsAchieved;
                }
                else
                {
                    _allRegistrations.Add(registration);
                }
            }
        }
    }

    public void Delete(Registration registration)
    {
        ArgumentNullException.ThrowIfNull(registration);

        Delete(registration.Id);
    }

    public void Delete(int id)
    {
        lock (_allRegistrations)
        {
            _registrationDataAccess.Delete(id);

            _allRegistrations.RemoveAll(reg => reg.Id == id);
        }
    }

    public List<Registration> GetAllRegistrationsByCourse(Course course)
    {
        ArgumentNullException.ThrowIfNull(course);

        return _registrationDataAccess.GetAllRegistrationsByCourse(course.Id);
    }
}
