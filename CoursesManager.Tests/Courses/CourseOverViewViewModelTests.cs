using System.Collections.ObjectModel;
using CoursesManager.MVVM.Dialogs;
using CoursesManager.MVVM.Messages;
using CoursesManager.MVVM.Navigation;
using CoursesManager.UI.Dialogs.ResultTypes;
using CoursesManager.UI.Dialogs.ViewModels;
using CoursesManager.UI.Enums;
using CoursesManager.UI.Mailing;
using CoursesManager.UI.Models;
using CoursesManager.UI.Repositories.CourseRepository;
using CoursesManager.UI.Repositories.RegistrationRepository;
using CoursesManager.UI.Repositories.StudentRepository;
using CoursesManager.UI.ViewModels.Courses;
using Moq;
using System.Diagnostics;

namespace CoursesManager.Tests.Courses
{
    [TestFixture]
    public class CourseOverViewViewModelTests
    {
        private Mock<IStudentRepository> _studentRepositoryMock;
        private Mock<IRegistrationRepository> _registrationRepositoryMock;
        private Mock<ICourseRepository> _courseRepositoryMock;
        private Mock<IDialogService> _dialogServiceMock;
        private Mock<IMessageBroker> _messageBrokerMock;
        private Mock<INavigationService> _navigationServiceMock;
        private Mock<IMailProvider> _mailProviderMock;
        private CourseOverViewViewModel _viewModel;
        private CourseOverViewViewModel? _tempViewModel;

        [SetUp]
        public void Setup()
        {
            _studentRepositoryMock = new Mock<IStudentRepository>();
            _registrationRepositoryMock = new Mock<IRegistrationRepository>();
            _courseRepositoryMock = new Mock<ICourseRepository>();
            _dialogServiceMock = new Mock<IDialogService>();
            _messageBrokerMock = new Mock<IMessageBroker>();
            _navigationServiceMock = new Mock<INavigationService>();
            _mailProviderMock = new Mock<IMailProvider>();
            GlobalCache.Instance.Put("Opened Course", new Course { Id = 1, Name = "Test Course" }, false);

            List<Student> students = new()
            {
            new Student { Id = 1, FirstName = "John", LastName = "Bergen", Courses = new System.Collections.ObjectModel.ObservableCollection<Course> { new Course { Id = 1, Name = "Test Course" } } },
            new Student { Id = 2, FirstName = "Piet", LastName = "Hendriks", Courses = new System.Collections.ObjectModel.ObservableCollection<Course>  { new Course { Id = 1, Name = "Test Course" } } }
            };
            _studentRepositoryMock.Setup(repo => repo.GetAll()).Returns(students);

            List<Registration> registrations = new()
            {
            new Registration { Id = 1, CourseId = 1, StudentId = 1 },
            new Registration { Id = 2, CourseId = 1, StudentId = 2 }
            };
            _registrationRepositoryMock.Setup(repo => repo.GetAll()).Returns(registrations);

            // Act
            _viewModel = new CourseOverViewViewModel(
            _studentRepositoryMock.Object,
            _registrationRepositoryMock.Object,
            _courseRepositoryMock.Object,
            _dialogServiceMock.Object,
            _messageBrokerMock.Object,
            _navigationServiceMock.Object,
            _mailProviderMock.Object
            );
        }

        [Test]
        public void Test_LoadCourseData_When_Course_Is_Set_Should_Populate_Students_And_Payments()
        {
            // Arrange
            var course = new Course
            {
                Id = 1,
                Name = "Advanced Programming Concepts",
                Code = "CS202",
                Description = "A detailed course on advanced programming concepts.",
                Participants = 2,
                IsActive = true,
                IsPayed = true,
                Category = "Computer Science",
                StartDate = new DateTime(2023, 10, 1),
                EndDate = new DateTime(2023, 12, 15),
                LocationId = 101,
                Location = new Location
                {
                    Id = 101,
                    Name = "Main Campus, Room 204",
                    Address = new Address
                    {
                        Id = 201,
                        Country = "United States",
                        ZipCode = "12345",
                        City = "Techville",
                        Street = "University Blvd",
                        HouseNumber = "123"
                    }
                },
                DateCreated = new DateTime(2023, 9, 15),
                Students = new ObservableCollection<Student>
    {
        new Student
        {
            Id = 1,
            FirstName = "Alice",
            LastName = "Johnson",
            Email = "alice.johnson@example.com",
            Phone = "123-456-7890",
            IsDeleted = false,
            DateOfBirth = new DateTime(2000, 5, 15),
            AddressId = 201,
            Address = new Address
            {
                Id = 201,
                Country = "United States",
                ZipCode = "12345",
                City = "Techville",
                Street = "University Blvd",
                HouseNumber = "321"
            },
            Registrations = new ObservableCollection<Registration>()
        },
        new Student
        {
            Id = 2,
            FirstName = "Bob",
            LastName = "Smith",
            Email = "bob.smith@example.com",
            Phone = "987-654-3210",
            IsDeleted = false,
            DateOfBirth = new DateTime(1999, 8, 20),
            AddressId = 202,
            Address = new Address
            {
                Id = 202,
                Country = "United States",
                ZipCode = "54321",
                City = "Innovate City",
                Street = "Tech Street",
                HouseNumber = "456"
            },
            Registrations = new ObservableCollection<Registration>()
        }
    },
                Registrations = new List<Registration>
    {
        new Registration
        {
            Id = 1,
            StudentId = 1,
            Student = null,
            CourseId = 1,
            Course = null,
            RegistrationDate = new DateTime(2023, 9, 1),
            PaymentStatus = true,
            IsActive = true,
            IsAchieved = false
        },
        new Registration
        {
            Id = 2,
            StudentId = 2,
            Student = null,
            CourseId = 1,
            Course = null,
            RegistrationDate = new DateTime(2023, 9, 2),
            PaymentStatus = false,
            IsActive = true,
            IsAchieved = false
        }
    },
                Image = new byte[] { 255, 216, 255 }
            };

            // Assign circular references
            foreach (var registration in course.Registrations)
            {
                registration.Course = course;
                registration.Student = course.Students.FirstOrDefault(s => s.Id == registration.StudentId);
            }

            foreach (var student in course.Students)
            {
                student.Registrations = new ObservableCollection<Registration>(course.Registrations.Where(r => r.StudentId == student.Id).ToList());
            }
            GlobalCache.Instance.Put("Opened Course", course, false);

            _courseRepositoryMock.Setup(repo => repo.GetById(It.IsAny<int>())).Returns<int>(id => course);

            List<Student> students = new()
            {
            new Student { Id = 1, FirstName = "John", LastName = "Bergen", Courses = new System.Collections.ObjectModel.ObservableCollection<Course> { new Course { Id = 1, Name = "Test Course" } } },
            new Student { Id = 2, FirstName = "Piet", LastName = "Hendriks", Courses = new System.Collections.ObjectModel.ObservableCollection<Course>  { new Course { Id = 1, Name = "Test Course" } } }
            };
            _studentRepositoryMock.Setup(repo => repo.GetAll()).Returns(students);
            _studentRepositoryMock.Setup(repo => repo.GetById(It.IsAny<int>())).Returns<int>(id => students.FirstOrDefault(s => s.Id == id));

            List<Registration> registrations = new()
            {
            new Registration { Id = 1, CourseId = 1, StudentId = 1 , PaymentStatus = true, IsAchieved = false},
            new Registration { Id = 2, CourseId = 1, StudentId = 2 , PaymentStatus = true, IsAchieved = false}
            };
            _registrationRepositoryMock.Setup(repo => repo.GetAll()).Returns(registrations);

            // Act
            _tempViewModel = new CourseOverViewViewModel(
            _studentRepositoryMock.Object,
            _registrationRepositoryMock.Object,
            _courseRepositoryMock.Object,
            _dialogServiceMock.Object,
            _messageBrokerMock.Object,
            _navigationServiceMock.Object,
            _mailProviderMock.Object
            );

            // Assert
            Assert.That(2, Is.EqualTo(_tempViewModel.StudentPayments.Count));
        }


        [Test]
        public void Test_DeleteCourseCommand_When_Course_Has_Registrations_Should_Show_ErrorDialog()
        {
            // Arrange
            _registrationRepositoryMock.Setup(repo => repo.GetAllRegistrationsByCourse(It.IsAny<Course>()))
                .Returns([new Registration { Id = 1 }]);

            // Act
            _viewModel.DeleteCourseCommand.Execute(null);

            // Assert
            _messageBrokerMock.Verify(d => d.Publish(It.Is<ToastNotificationMessage>(msg =>
                    msg.SetVisibillity == true &&
                    msg.NotificationText == "Cursus heeft nog actieve registraties." &&
                    msg.ToastType == ToastType.Error)),
                Times.Once);
        }

        [Test]
        public void Test_DeleteCourseCommand_With_No_Registrations()
        {
            // Arrange
            List<Registration> emptyList = new();
            _registrationRepositoryMock.Setup(repo => repo.GetAllRegistrationsByCourse(It.IsAny<Course>()))
                .Returns(emptyList);

            var confirmationResult = DialogResult<DialogResultType>.Builder()
            .SetSuccess(new DialogResultType { Result = true }, "Confirmed")
            .Build();

            _dialogServiceMock
                .Setup(ds => ds.ShowDialogAsync<ConfirmationDialogViewModel, DialogResultType>(
                    It.Is<DialogResultType>(result => result.DialogText == "Weet je zeker dat je deze cursus wilt verwijderen?")))
                .ReturnsAsync(confirmationResult);

            _dialogServiceMock
                .Setup(ds => ds.ShowDialogAsync<NotifyDialogViewModel, DialogResultType>(It.IsAny<DialogResultType>()))
                .ReturnsAsync(DialogResult<DialogResultType>.Builder().SetSuccess(new DialogResultType(), "Notification").Build());

            // Act
            _viewModel.DeleteCourseCommand.Execute(null);

            // Assert
            _courseRepositoryMock.Verify(repo => repo.Delete(It.IsAny<Course>()), Times.Once);
            _messageBrokerMock.Verify(mb => mb.Publish(It.IsAny<CoursesChangedMessage>()), Times.Once);
        }

        [Test]
        public void Test_CheckboxChangedCommand()
        {
            // Arrange
            var registration = new Registration
            {
                Id = 1,
                CourseId = 1,
                StudentId = 1,
                PaymentStatus = false,
                IsAchieved = false,
                RegistrationDate = DateTime.Now,
                IsActive = true
            };

            var course = new Course
            {
                Id = 1,
                Registrations = new List<Registration> { registration },
                StartDate = DateTime.Now.AddDays(10),
                EndDate = DateTime.Now.AddMonths(1),
                IsPayed = false
            };

            var student = new Student { Id = 1 };

            _courseRepositoryMock.Setup(repo => repo.GetById(It.IsAny<int>()))
                .Returns(course);

            _registrationRepositoryMock.Setup(repo => repo.Update(It.IsAny<Registration>()));
            _registrationRepositoryMock.Setup(repo => repo.Add(It.IsAny<Registration>()));

            _studentRepositoryMock.Setup(repo => repo.GetById(It.IsAny<int>()))
                .Returns(student);

            var payment = new CourseStudentPayment(student, registration)
            {
                IsPaid = true,
                IsAchieved = true
            };

            _viewModel.CurrentCourse = course;

            // Act
            _viewModel.CheckboxChangedCommand.Execute(payment);

            // Assert
            _registrationRepositoryMock.Verify(repo => repo.Update(It.Is<Registration>(r =>
                r.StudentId == payment.Student.Id &&
                r.PaymentStatus == payment.IsPaid &&
                r.IsAchieved == payment.IsAchieved
            )), Times.Once);
        }
    }




}

