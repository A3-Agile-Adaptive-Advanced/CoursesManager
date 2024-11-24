using CoursesManager.UI.Models;
using System.Collections.ObjectModel;

namespace CoursesManager.UI.Repositories.StudentRepository
{
    public class DummyStudentRepository : IStudentRepository
    {
        private readonly ObservableCollection<Student> _students;

        public DummyStudentRepository(ObservableCollection<Student> students)
        {
            _students = students ?? throw new ArgumentNullException(nameof(students), "Students collection cannot be null.");
        }

        public List<Student> GetAll()
        {
            return _students.Where(s => !s.Is_deleted).ToList();
        }

        public Student GetById(int id)
        {
            return _students.FirstOrDefault(s => s.Id == id && !s.Is_deleted);
        }

        public void Add(Student student)
        {
            if (student == null) throw new ArgumentNullException(nameof(student), "Student cannot be null.");

            student.DateCreated = DateTime.Now;
            student.Id = _students.Any() ? _students.Max(s => s.Id) + 1 : 1;
            _students.Add(student);
        }

        public void Update(Student student)
        {
            if (student == null) throw new ArgumentNullException(nameof(student), "Student cannot be null.");

            var existingStudent = GetById(student.Id);
            if (existingStudent == null) throw new InvalidOperationException($"Student with ID {student.Id} does not exist.");

            existingStudent.FirstName = student.FirstName;
            existingStudent.LastName = student.LastName;
            existingStudent.Email = student.Email;
            existingStudent.PhoneNumber = student.PhoneNumber;
            existingStudent.PostCode = student.PostCode;
            existingStudent.HouseNumber = student.HouseNumber;
            existingStudent.HouseNumberExtension = student.HouseNumberExtension;
        }

        public void Delete(Student data)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            var student = GetById(id);
            if (student == null) throw new InvalidOperationException($"Student with ID {id} does not exist.");

            student.Is_deleted = true;
            student.date_deleted = DateTime.Now;
        }

        public bool EmailExists(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email cannot be null or empty.", nameof(email));

            return _students.Any(s => s.Email.Equals(email, StringComparison.OrdinalIgnoreCase) && !s.Is_deleted);
        }
    }
}