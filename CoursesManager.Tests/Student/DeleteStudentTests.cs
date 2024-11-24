using NUnit.Framework;
using Moq;
using CoursesManager.UI.ViewModels;
using CoursesManager.UI.Models;
using CoursesManager.UI.Services;
using CoursesManager.UI.Dialogs.Enums;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CoursesManager.MVVM.Dialogs;
using CoursesManager.UI.Dialogs.ResultTypes;
using System.Windows;
using System.Linq;
using CoursesManager.UI.Dialogs.ViewModels;
using CoursesManager.MVVM.Messages;
using CoursesManager.UI.Repositories.RegistrationRepository;
using CoursesManager.UI.Repositories.StudentRepository;
using CoursesManager.UI.Repositories.CourseRepository;

namespace CoursesManager.Tests
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

            _testStudent = new Student
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "johndoe@example.com",
                Is_deleted = false
            };

            _mockStudentRepository.Setup(repo => repo.GetAll()).Returns(new List<Student> { _testStudent });

            _viewModel = new StudentManagerViewModel(
                _mockDialogService.Object,
                _mockStudentRepository.Object,
                _mockCourseRepository.Object,
                _mockRegistrationRepository.Object,
                _mockMessageBroker.Object);

            _viewModel.LoadStudents();
        }

        [Test]
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
            _testStudent.Is_deleted = true;
            _testStudent.date_deleted = DateTime.Now;

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

        [Test]
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
