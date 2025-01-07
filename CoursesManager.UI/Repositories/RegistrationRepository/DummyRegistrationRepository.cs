using CoursesManager.UI.Models;
using System.Collections.ObjectModel;

namespace CoursesManager.UI.Repositories.RegistrationRepository
{
    public class DummyRegistrationRepository : IRegistrationRepository
    {
        private readonly ObservableCollection<Registration> _registrations;

        public DummyRegistrationRepository(ObservableCollection<Registration> registrations)
        {
            _registrations = registrations ?? throw new ArgumentNullException(nameof(registrations), "Registrations collection cannot be null.");
        }

        public List<Registration> GetAll()
        {
            return _registrations.ToList();
        }

        public List<Registration> RefreshAll()
        {
            throw new NotImplementedException();
        }

        public Registration GetById(int id)
        {
            return _registrations.FirstOrDefault(r => r.Id == id);
        }

        public void Add(Registration registration)
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration), "Registration cannot be null.");

            registration.Id = _registrations.Any() ? _registrations.Max(r => r.Id) + 1 : 1;
            _registrations.Add(registration);
        }

        public void Update(Registration registration)
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration), "Registration cannot be null.");

            var existingRegistration = GetById(registration.Id);
            if (existingRegistration == null) throw new InvalidOperationException($"Registration with Id {registration.Id} does not exist.");

            existingRegistration.StudentId = registration.StudentId;
            existingRegistration.CourseId = registration.CourseId;
            existingRegistration.RegistrationDate = registration.RegistrationDate;
            existingRegistration.PaymentStatus = registration.PaymentStatus;
            existingRegistration.IsActive = registration.IsActive;
        }

        public void Delete(Registration data)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            var registration = GetById(id);
            if (registration == null) throw new InvalidOperationException($"Registration with Id {id} does not exist.");

            _registrations.Remove(registration);
        }

        public List<Registration> GetAllRegistrationsByCourse(Course course)
        {
            throw new NotImplementedException();
        }
        public List<Registration> GetAllRegistrationsByStudent(Student student)
        {
            throw new NotImplementedException();
        }
    }
}