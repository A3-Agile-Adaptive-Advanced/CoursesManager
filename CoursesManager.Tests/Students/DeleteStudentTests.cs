using System.Collections.ObjectModel;
using Moq;
using CoursesManager.UI.Models;
using CoursesManager.MVVM.Dialogs;
using CoursesManager.UI.Dialogs.ResultTypes;
using CoursesManager.UI.Dialogs.ViewModels;
using CoursesManager.MVVM.Messages;
using CoursesManager.MVVM.Navigation;
using CoursesManager.UI.Repositories.RegistrationRepository;
using CoursesManager.UI.Repositories.StudentRepository;
using CoursesManager.UI.Repositories.CourseRepository;
using CoursesManager.UI.ViewModels.Students;

namespace CoursesManager.Tests.Students
{
    [TestFixture]
    [Apartment(System.Threading.ApartmentState.STA)]
    public class DeleteStudentTests
    {
        private Mock<IStudentRepository> _mockStudentRepository;
        private Mock<ICourseRepository> _mockCourseRepository;
        private Mock<IRegistrationRepository> _mockRegistrationRepository;
        private Mock<IDialogService> _mockDialogService;
        private Mock<IMessageBroker> _mockMessageBroker;
        private Mock<INavigationService> _mockNavigationService;
        private StudentManagerViewModel _viewModel;
        private Student _testStudent;

        [SetUp]
        public void SetUp()
        {
            _mockStudentRepository = new Mock<IStudentRepository>();
            _mockCourseRepository = new Mock<ICourseRepository>();
            _mockRegistrationRepository = new Mock<IRegistrationRepository>();
            _mockDialogService = new Mock<IDialogService>();
            _mockMessageBroker = new Mock<IMessageBroker>();
            _mockNavigationService = new Mock<INavigationService>();

            _testStudent = new Student
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "johndoe@example.com",
                IsDeleted = false
            };

            _mockStudentRepository.Setup(repo => repo.GetAll()).Returns(new ObservableCollection<Student> { _testStudent });

            _viewModel = new StudentManagerViewModel(
                _mockDialogService.Object,
                _mockStudentRepository.Object,
                _mockCourseRepository.Object,
                _mockRegistrationRepository.Object,
                _mockMessageBroker.Object,
                _mockNavigationService.Object);

            _viewModel.LoadStudents();
        }

        //[Test]
        public async Task DeleteStudent_SoftDeletesStudent_WhenConfirmed()
        {
            // Arrange
            _mockDialogService
                .Setup(d => d.ShowDialogAsync<ConfirmationDialogViewModel, DialogResultType>(It.IsAny<DialogResultType>()))
                .ReturnsAsync(DialogResult<DialogResultType>.Builder()
                    .SetSuccess(new DialogResultType { Result = true })
                    .Build());

            _mockDialogService
                .Setup(d => d.ShowDialogAsync<NotifyDialogViewModel, DialogResultType>(It.IsAny<DialogResultType>()))
                .ReturnsAsync(DialogResult<DialogResultType>.Builder()
                    .SetSuccess(new DialogResultType())
                    .Build());

            // Act
            await Task.Run(() => _viewModel.DeleteStudentCommand.Execute(_testStudent));
            _testStudent.IsDeleted = true;
            _testStudent.DeletedAt = DateTime.Now;

            // Assert
            _mockStudentRepository.Verify(repo => repo.Update(_testStudent), Times.Once);
            _mockDialogService.Verify(d =>
                d.ShowDialogAsync<ConfirmationDialogViewModel, DialogResultType>(
                    It.Is<DialogResultType>(r => r.DialogText == "Wilt u deze cursist verwijderen?")),
                Times.Once);

            _mockDialogService.Verify(d =>
                d.ShowDialogAsync<NotifyDialogViewModel, DialogResultType>(
                    It.Is<DialogResultType>(r => r.DialogText == "Cursist succesvol verwijderd.")),
                Times.Once);
        }

        //[Test]
        public async Task DeleteStudent_DoesNotDelete_WhenCancelled()
        {
            // Arrange
            _mockDialogService
                .Setup(d => d.ShowDialogAsync<ConfirmationDialogViewModel, DialogResultType>(It.IsAny<DialogResultType>()))
                .ReturnsAsync(DialogResult<DialogResultType>.Builder()
                    .SetSuccess(new DialogResultType { Result = false })
                    .Build());

            // Act
            await Task.Run(() => _viewModel.DeleteStudentCommand.Execute(_testStudent));

            // Assert
            _mockStudentRepository.Verify(repo => repo.Delete(It.IsAny<int>()), Times.Never);
            _mockDialogService.Verify(d =>
                d.ShowDialogAsync<ConfirmationDialogViewModel, DialogResultType>(
                    It.Is<DialogResultType>(r => r.DialogText == "Wilt u deze cursist verwijderen?")),
                Times.Once);

            _mockDialogService.Verify(d =>
                d.ShowDialogAsync<NotifyDialogViewModel, DialogResultType>(It.IsAny<DialogResultType>()),
                Times.Never);
        }
    }
}