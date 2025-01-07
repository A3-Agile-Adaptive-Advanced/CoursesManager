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
using CoursesManager.UI.Messages;

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
        private Course _course;

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

            var course = new Course { Id = 1, Name = "Test Course" };
            GlobalCache.Instance.Put("Opened Course", course, false);



            ObservableCollection<Student> students = new()
            {
                new Student { Id = 1, FirstName = "John", LastName = "Bergen" },
                new Student { Id = 2, FirstName = "Piet", LastName = "Hendriks"}
            };

            var registrations = new ObservableCollection<Registration>
            {
                new Registration
                {
                    Course = course,
                    CourseId = course.Id,
                    Id = 1,
                    IsAchieved = false,
                    IsActive = true,
                    PaymentStatus = false,
                    RegistrationDate = DateTime.Now,
                    Student = students[0],
                    StudentId = students[0].Id
                },
                new Registration
                {
                    Course = course,
                    CourseId = course.Id,
                    Id = 1,
                    IsAchieved = false,
                    IsActive = true,
                    PaymentStatus = false,
                    RegistrationDate = DateTime.Now,
                    Student = students[1],
                    StudentId = students[1].Id
                }
            };

            foreach (var student in students)
            {
                student.Registrations = new ObservableCollection<Registration>
                {
                    registrations.FirstOrDefault(r => r.StudentId == student.Id)!
                };
            }

            course.Registrations = registrations;
            _course = course;
            

            _studentRepositoryMock.Setup(repo => repo.GetAll()).Returns(students);
            _studentRepositoryMock.Setup(repo => repo.GetById(It.IsAny<int>())).Returns<int>(id => students.FirstOrDefault(s => s.Id == id));

            _courseRepositoryMock.Setup(repo => repo.GetById(It.IsAny<int>())).Returns<int>(id => course);
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
            GlobalCache.Instance.Put("Opened Course", _course, false);

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
                Registrations = new ObservableCollection<Registration> { registration },
                StartDate = DateTime.Now.AddDays(10),
                EndDate = DateTime.Now.AddMonths(1)
            };

            var student = new Student { Id = 1 };

            _courseRepositoryMock.Setup(repo => repo.GetById(It.IsAny<int>()))
                .Returns(course);

            _registrationRepositoryMock.Setup(repo => repo.GetAllRegistrationsByCourse(It.IsAny<Course>()))
                .Returns(new List<Registration> { registration });

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

            _courseRepositoryMock.Verify(repo => repo.Update(It.Is<Course>(c =>
                c.Id == course.Id &&
                c.IsPayed == true
            )), Times.Once);
        }
    }
}

