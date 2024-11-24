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

        public Registration GetById(int id)
        {
            return _registrations.FirstOrDefault(r => r.ID == id);
        }

        public void Add(Registration registration)
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration), "Registration cannot be null.");

            registration.DateCreated = DateTime.Now;
            registration.ID = _registrations.Any() ? _registrations.Max(r => r.ID) + 1 : 1;
            _registrations.Add(registration);
        }

        public void Update(Registration registration)
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration), "Registration cannot be null.");

            var existingRegistration = GetById(registration.ID);
            if (existingRegistration == null) throw new InvalidOperationException($"Registration with ID {registration.ID} does not exist.");

            existingRegistration.StudentID = registration.StudentID;
            existingRegistration.CourseID = registration.CourseID;
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
            if (registration == null) throw new InvalidOperationException($"Registration with ID {id} does not exist.");

            _registrations.Remove(registration);
        }
    }
}