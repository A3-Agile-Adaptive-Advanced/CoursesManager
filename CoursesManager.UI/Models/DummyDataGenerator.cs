using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoursesManager.UI.Models
{
    // This class is used to generate dummy data for the students.
    // We can use this one until we have a database to store the data.
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    namespace CoursesManager.UI.Models
    {
        public static class DummyDataGenerator
        {
            private static Random _random = new Random();

            public static ObservableCollection<Student> GenerateStudents(int count)
            {
                ObservableCollection<Student> students = new ObservableCollection<Student>();

                for (int i = 0; i < count; i++)
                {
                    Student student = new Student
                    {
                        Id = i + 1,
                        FirstName = GetRandomFirstName(),
                        Insertion = GetRandomInsertion(),
                        LastName = GetRandomLastName(),
                        Email = $"{GetRandomFirstName()}.{GetRandomLastName()}@example.com".ToLower(),
                        PhoneNumber = GenerateRandomPhoneNumber(),
                        PostCode = GenerateRandomPostCode(),
                        HouseNumber = _random.Next(1, 300).ToString(),
                        AwaitingPayement = _random.Next(0, 2) == 1,
                        DateCreated = DateTime.Now,
                        //Courses = GenerateCourses(GenerateRandomSizeNumber())
                    };

                    students.Add(student);
                }

                return students;
            }

            public static ObservableCollection<Course> GenerateCourses(int count)
            {
                ObservableCollection<Course> courses = new ObservableCollection<Course>();
                ObservableCollection<Student> student = new ObservableCollection<Student>();
                

                for (int i = 1; i < count; i++)
                {
                    Course course = new Course
                    {
                        ID = i,
                        Name = $"Cursus{i+7}",
                        Code = GenerateRandomCourseCode(),
                        Description = "Wat is Lorem Ipsum?\r\nLorem Ipsum is slechts een proeftekst uit het drukkerij- en zetterijwezen. Lorem Ipsum is de standaard proeftekst in deze bedrijfstak sinds de 16e eeuw, toen een onbekende drukker een zethaak met letters nam en ze door elkaar husselde om een font-catalogus te maken. Het heeft niet alleen vijf eeuwen overleefd maar is ook, vrijwel onveranderd, overgenomen in elektronische letterzetting. Het is in de jaren '60 populair geworden met de introductie van Letraset vellen met Lorem Ipsum passages en meer recentelijk door desktop publishing software zoals Aldus PageMaker die versies van Lorem Ipsum bevatten.\r\n\r\nWaarom gebruiken we het?\r\nHet is al geruime tijd een bekend gegeven dat een lezer, tijdens het bekijken van de layout van een pagina, afgeleid wordt door de tekstuele inhoud. Het belangrijke punt van het gebruik van Lorem Ipsum is dat het uit een min of meer normale verdeling van letters bestaat, in tegenstelling tot \"Hier uw tekst, hier uw tekst\" wat het tot min of meer leesbaar nederlands maakt. Veel desktop publishing pakketten en web pagina editors gebruiken tegenwoordig Lorem Ipsum als hun standaard model tekst, en een zoekopdracht naar \"lorem ipsum\" ontsluit veel websites die nog in aanbouw zijn. Verscheidene versies hebben zich ontwikkeld in de loop van de jaren, soms per ongeluk soms expres (ingevoegde humor en dergelijke).\r\n\r\n\r\nWaar komt het vandaan?\r\nIn tegenstelling tot wat algemeen aangenomen wordt is Lorem Ipsum niet zomaar willekeurige tekst. het heeft zijn wortels in een stuk klassieke latijnse literatuur uit 45 v.Chr. en is dus meer dan 2000 jaar oud. Richard McClintock, een professor latijn aan de Hampden-Sydney College in Virginia, heeft één van de meer obscure latijnse woorden, consectetur, uit een Lorem Ipsum passage opgezocht, en heeft tijdens het zoeken naar het woord in de klassieke literatuur de onverdachte bron ontdekt. Lorem Ipsum komt uit de secties 1.10.32 en 1.10.33 van \"de Finibus Bonorum et Malorum\" (De uitersten van goed en kwaad) door Cicero, geschreven in 45 v.Chr. Dit boek is een verhandeling over de theorie der ethiek, erg populair tijdens de renaissance. De eerste regel van Lorem Ipsum, \"Lorem ipsum dolor sit amet..\", komt uit een zin in sectie 1.10.32.\r\n\r\nHet standaard stuk van Lorum Ipsum wat sinds de 16e eeuw wordt gebruikt is hieronder, voor wie er interesse in heeft, weergegeven. Secties 1.10.32 en 1.10.33 van \"de Finibus Bonorum et Malorum\" door Cicero zijn ook weergegeven in hun exacte originele vorm, vergezeld van engelse versies van de 1914 vertaling door H. Rackham.\r\n\r\nWaar kan ik het vinden?\r\nEr zijn vele variaties van passages van Lorem Ipsum beschikbaar maar het merendeel heeft te lijden gehad van wijzigingen in een of andere vorm, door ingevoegde humor of willekeurig gekozen woorden die nog niet half geloofwaardig ogen. Als u een passage uit Lorum Ipsum gaat gebruiken dient u zich ervan te verzekeren dat er niets beschamends midden in de tekst verborgen zit. Alle Lorum Ipsum generators op Internet hebben de eigenschap voorgedefinieerde stukken te herhalen waar nodig zodat dit de eerste echte generator is op internet. Het gebruikt een woordenlijst van 200 latijnse woorden gecombineerd met een handvol zinsstructuur modellen om een Lorum Ipsum te genereren die redelijk overkomt. De gegenereerde Lorum Ipsum is daardoor altijd vrij van herhaling, ingevoegde humor of ongebruikelijke woorden etc.",
                        Participants = i,
                        IsActive = _random.Next(0, 2) == 1, // Randomly true or false
                        IsPayed = _random.Next(0, 2) == 1, // Randomly true or false
                        Category = $"Category{i % 3}",
                        StartDate = DateTime.Now.AddDays(_random.Next(1, 30)),
                        EndDate = DateTime.Now.AddDays(_random.Next(31, 60)),
                        LocationId = _random.Next(1, count),
                        DateCreated = DateTime.Now,
                        students = GenerateStudents(i + 1) 
                    };
                    courses.Add(course);
                }

                return courses;
            }

            public static ObservableCollection<Location> GenerateLocations(int count)
            {
                ObservableCollection<Location> locations = new ObservableCollection<Location>();

                for (int i = 0; i < count; i++)
                {
                    Location location = new Location
                    {
                        Id = i + 1,
                        Name = $"Location{i + 1}",
                        Address = GenerateRandomAddress(),
                        Capacity = $"{_random.Next(10, 100)}",
                        DateCreated = DateTime.Now
                    };

                    locations.Add(location);
                }

                return locations;
            }

            private static Address GenerateRandomAddress()
            {
                return new Address
                {
                    Id = _random.Next(1, 1000),
                    Country = "Nederland",
                    Zipcode = GenerateRandomPostCode(),
                    City = $"City{_random.Next(1, 100)}",
                    Street = $"Street{_random.Next(1, 100)}",
                    HouseNumber = _random.Next(1, 300).ToString()
                };
            }

            public static ObservableCollection<Registration> GenerateRegistrations(int studentCount, int courseCount)
            {
                ObservableCollection<Registration> registrations = new ObservableCollection<Registration>();

                for (int i = 0; i < studentCount; i++)
                {
                    Registration registration = new Registration
                    {
                        ID = i + 1,
                        StudentID = _random.Next(1, studentCount + 1),
                        CourseID = _random.Next(1, courseCount + 1),
                        RegistrationDate = DateTime.Now.AddDays(-_random.Next(1, 30)),
                        PaymentStatus = _random.Next(0, 2) == 1,
                        IsActive = _random.Next(0, 2) == 1,
                        DateCreated = DateTime.Now
                    };

                    registrations.Add(registration);
                }

                return registrations;
            }

            private static string GetRandomFirstName()
            {
                string[] firstNames =
                    { "John", "Jane", "Michael", "Emily", "Chris", "Anna", "David", "Sarah", "Robert", "Emma", };
                return firstNames[_random.Next(firstNames.Length)];
            }

            private static string GetRandomInsertion()
            {
                string[] insertions = { "", "van", "de", "der", "von", "van der" };
                return insertions[_random.Next(insertions.Length)];
            }

            private static string GetRandomLastName()
            {
                string[] lastNames =
                {
                    "Smith", "Johnson", "Brown", "Williams", "Jones", "Garcia", "Miller", "Davis", "Rodriguez",
                    "Martinez"
                };
                return lastNames[_random.Next(lastNames.Length)];
            }

            private static string GenerateRandomPhoneNumber()
            {
                return $"+1-{_random.Next(100, 999)}-{_random.Next(100, 999)}-{_random.Next(1000, 9999)}";
            }

            public static int GenerateRandomSizeNumber()
            {
                return _random.Next(0, 9);
            }

            private static string GenerateRandomNumber()
            {
                return $"{_random.Next(1, 999)}";
            }

            private static string GenerateRandomPostCode()
            {
                string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                return
                    $"{_random.Next(1000, 9999)} {letters[_random.Next(letters.Length)]}{letters[_random.Next(letters.Length)]}";
            }

            private static string GenerateRandomHouseNumberExtension()
            {
                string[] extensions = { "", "A", "B", "C", "D", "E" };
                return extensions[_random.Next(extensions.Length)];
            }

            private static string GenerateRandomCourseCode()
            {
                string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                return
                    $"{letters[_random.Next(letters.Length)]}{letters[_random.Next(letters.Length)]}{letters[_random.Next(letters.Length)]}.{letters[_random.Next(letters.Length)]}{letters[_random.Next(letters.Length)]}";
            }
        }
    }
}
