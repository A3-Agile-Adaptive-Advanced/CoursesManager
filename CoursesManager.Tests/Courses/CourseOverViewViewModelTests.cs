//using CoursesManager.MVVM.Dialogs;
//using CoursesManager.MVVM.Messages;
//using CoursesManager.MVVM.Navigation;
//using CoursesManager.UI.Dialogs.ResultTypes;
//using CoursesManager.UI.Dialogs.ViewModels;
//using CoursesManager.UI.ViewModels.Courses;
//using CoursesManager.UI.Models;
//using CoursesManager.UI.Repositories.CourseRepository;
//using CoursesManager.UI.Repositories.RegistrationRepository;
//using CoursesManager.UI.Repositories.StudentRepository;
//using Moq;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Collections.ObjectModel;

//namespace CoursesManager.Tests.Courses
//{
//    [TestFixture]
//    public class CourseOverViewViewModelTests
//    {
//        private Mock<IStudentRepository> _studentRepositoryMock;
//        private Mock<IRegistrationRepository> _registrationRepositoryMock;
//        private Mock<ICourseRepository> _courseRepositoryMock;
//        private Mock<IDialogService> _dialogServiceMock;
//        private Mock<IMessageBroker> _messageBrokerMock;
//        private Mock<INavigationService> _navigationServiceMock;
//        private CourseOverViewViewModel _viewModel;
//        private CourseOverViewViewModel? _tempViewModel;

//        [SetUp]
//        public void Setup()
//        {
//            _studentRepositoryMock = new Mock<IStudentRepository>();
//            _registrationRepositoryMock = new Mock<IRegistrationRepository>();
//            _courseRepositoryMock = new Mock<ICourseRepository>();
//            _dialogServiceMock = new Mock<IDialogService>();
//            _messageBrokerMock = new Mock<IMessageBroker>();
//            _navigationServiceMock = new Mock<INavigationService>();

//            GlobalCache.Instance.Put("Opened Course", new Course { Id = 1, Name = "Test Course" }, false);

//            List<Student> students = new()
//            {
//            new Student { Id = 1, FirstName = "John", LastName = "Bergen", Courses = new System.Collections.ObjectModel.ObservableCollection<Course> { new Course { Id = 1, Name = "Test Course" } } },
//            new Student { Id = 2, FirstName = "Piet", LastName = "Hendriks", Courses = new System.Collections.ObjectModel.ObservableCollection<Course>  { new Course { Id = 1, Name = "Test Course" } } }
//            };
//            _studentRepositoryMock.Setup(repo => repo.GetAll()).Returns(students);

//            List<Registration> registrations = new()
//            {
//            new Registration { Id = 1, CourseId = 1, StudentId = 1 },
//            new Registration { Id = 2, CourseId = 1, StudentId = 2 }
//            };
//            _registrationRepositoryMock.Setup(repo => repo.GetAll()).Returns(registrations);

//            // Act
//            _viewModel = new CourseOverViewViewModel(
//            _studentRepositoryMock.Object,
//            _registrationRepositoryMock.Object,
//            _courseRepositoryMock.Object,
//            _dialogServiceMock.Object,
//            _messageBrokerMock.Object,
//            _navigationServiceMock.Object
//            );
//        }

//        [Test]
//        public void LoadCourseData_WhenCourseIsSet_ShouldPopulateStudentsAndPayments()
//        {
//            // Arrange

//            GlobalCache.Instance.Put("Opened Course", new Course { Id = 1, Name = "Test Course" }, false);

//            List<Student> students = new()
//            {
//            new Student { Id = 1, FirstName = "John", LastName = "Bergen", Courses = new System.Collections.ObjectModel.ObservableCollection<Course> { new Course { Id = 1, Name = "Test Course" } } },
//            new Student { Id = 2, FirstName = "Piet", LastName = "Hendriks", Courses = new System.Collections.ObjectModel.ObservableCollection<Course>  { new Course { Id = 1, Name = "Test Course" } } }
//            };
//            _studentRepositoryMock.Setup(repo => repo.GetAll()).Returns(students);
//            _studentRepositoryMock.Setup(repo => repo.GetById(It.IsAny<int>())).Returns<int>(id => students.FirstOrDefault(s => s.Id == id));

//            List<Registration> registrations = new()
//            {
//            new Registration { Id = 1, CourseId = 1, StudentId = 1 , PaymentStatus = true, IsAchieved = false},
//            new Registration { Id = 2, CourseId = 1, StudentId = 2 , PaymentStatus = true, IsAchieved = false}
//            };
//            _registrationRepositoryMock.Setup(repo => repo.GetAll()).Returns(registrations);

//            // Act
//            _tempViewModel = new CourseOverViewViewModel(
//            _studentRepositoryMock.Object,
//            _registrationRepositoryMock.Object,
//            _courseRepositoryMock.Object,
//            _dialogServiceMock.Object,
//            _messageBrokerMock.Object,
//            _navigationServiceMock.Object
//            );

//            // Assert
//            Assert.That(2, Is.EqualTo(_tempViewModel.StudentPayments.Count));
//        }


//        [Test]
//        public void DeleteCourseCommand_WhenCourseHasRegistrations_ShouldShowErrorDialog()
//        {
//            // Arrange
//            _registrationRepositoryMock.Setup(repo => repo.GetAllRegistrationsByCourse(It.IsAny<Course>()))
//                .Returns( [new Registration { Id = 1 }] );

//            // Act
//            _viewModel.DeleteCourseCommand.Execute(null);

//            // Assert
//            _dialogServiceMock.Verify(d => d.ShowDialogAsync<ErrorDialogViewModel, DialogResultType>(
//                It.Is<DialogResultType>(dr => dr.DialogText.Contains("actieve registraties"))), Times.Once);
//        }

//        [Test]
//        public void DeleteCourseCommand_WhenNoRegistrations_ShouldDeleteCourseAndPublishMessage()
//        {
//            // Arrange
//            List<Registration> emptyList = new ();
//            _registrationRepositoryMock.Setup(repo => repo.GetAllRegistrationsByCourse(It.IsAny<Course>()))
//                .Returns(emptyList);

//            var confirmationResult = DialogResult<DialogResultType>.Builder()
//            .SetSuccess(new DialogResultType { Result = true }, "Confirmed")
//            .Build();

//            _dialogServiceMock
//                .Setup(ds => ds.ShowDialogAsync<ConfirmationDialogViewModel, DialogResultType>(
//                    It.Is<DialogResultType>(result => result.DialogText == "Weet je zeker dat je deze cursus wilt verwijderen?")))
//                .ReturnsAsync(confirmationResult);

//            _dialogServiceMock
//                .Setup(ds => ds.ShowDialogAsync<NotifyDialogViewModel, DialogResultType>(It.IsAny<DialogResultType>()))
//                .ReturnsAsync(DialogResult<DialogResultType>.Builder().SetSuccess(new DialogResultType(), "Notification").Build());

//            // Act
//            _viewModel.DeleteCourseCommand.Execute(null);

//            // Assert
//            _courseRepositoryMock.Verify(repo => repo.Delete(It.IsAny<Course>()), Times.Once);
//            _messageBrokerMock.Verify(mb => mb.Publish(It.IsAny<CoursesChangedMessage>()), Times.Once);
//        }

//        [Test]
//        public void CheckboxChangedCommand_WhenCalled_ShouldUpdateRegistration()
//        {
//            // Arrange
//            var registration = new Registration { Id = 1, CourseId = 1, StudentId = 1, PaymentStatus = false, IsAchieved = false };
//            _registrationRepositoryMock.Setup(repo => repo.GetAll())
//                .Returns( [ registration ] );

//            var payment = new CourseStudentPayment(new Student { Id = 1 }, registration)
//            {
//                IsPaid = true,
//                IsAchieved = true
//            };

//            // Act
//            _viewModel.CheckboxChangedCommand.Execute(payment);

//            // Assert
//            _registrationRepositoryMock.Verify(repo => repo.Update(It.Is<Registration>(r => r.PaymentStatus == true && r.IsAchieved == true)), Times.Once);
//        }
//    }




//}

