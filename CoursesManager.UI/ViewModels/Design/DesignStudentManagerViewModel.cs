using CoursesManager.UI.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoursesManager.MVVM.Data;

namespace CoursesManager.UI.ViewModels.Design
{
    public class DesignStudentManagerViewModel : ViewModel
    {
        public ObservableCollection<Student> FilteredStudentRecords { get; set; }

        public DesignStudentManagerViewModel()
        {
            // Generate some random Students for design-time
            FilteredStudentRecords = GenerateRandomStudents(100); 
        }

        // Method to generate random Students
        public static ObservableCollection<Student> GenerateRandomStudents(int count)
        {
            ObservableCollection<Student> students = new ObservableCollection<Student>();
            Random random = new Random();

            for (int i = 0; i < count; i++)
            {
                Student student = new Student
                {
                    FirstName = GetRandomFirstName(),
                    Insertion = GetRandomInsertion(),
                    LastName = GetRandomLastName(),
                    Email = $"{GetRandomFirstName()}.{GetRandomLastName()}@example.com".ToLower(),
                    PhoneNumber = GenerateRandomPhoneNumber(),
                    PostCode = GenerateRandomPostCode(),
                    HouseNumber = random.Next(1, 300).ToString(),
                    HouseNumberExtension = GenerateRandomHouseNumberExtension(),
                    AwaitingPayement = random.Next(0, 2) == 1 // Randomly true or false
                };

                students.Add(student);
            }

            return students;
        }

        // Helper methods (copied from your Student class)
        private static string GetRandomFirstName()
        {
            string[] firstNames = { "John", "Jane", "Michael", "Emily", "Chris", "Anna", "David", "Sarah", "Robert", "Emma" };
            Random random = new Random();
            return firstNames[random.Next(firstNames.Length)];
        }

        private static string GetRandomInsertion()
        {
            string[] insertions = { "", "van", "de", "der", "von", "van der" };
            Random random = new Random();
            return insertions[random.Next(insertions.Length)];
        }

        private static string GetRandomLastName()
        {
            string[] lastNames = { "Smith", "Johnson", "Brown", "Williams", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez" };
            Random random = new Random();
            return lastNames[random.Next(lastNames.Length)];
        }

        private static string GenerateRandomPhoneNumber()
        {
            Random random = new Random();
            return $"+1-{random.Next(100, 999)}-{random.Next(100, 999)}-{random.Next(1000, 9999)}";
        }

        private static string GenerateRandomPostCode()
        {
            Random random = new Random();
            string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return $"{random.Next(1000, 9999)} {letters[random.Next(letters.Length)]}{letters[random.Next(letters.Length)]}";
        }

        private static string GenerateRandomHouseNumberExtension()
        {
            string[] extensions = { "", "A", "B", "C", "D", "E" };
            Random random = new Random();
            return extensions[random.Next(extensions.Length)];
        }
    }
}
