using NUnit.Framework;
using Moq;
using CoursesManager.UI.ViewModels;
using CoursesManager.UI.Models;
using CoursesManager.UI.Services;
using CoursesManager.UI.Dialogs.Enums;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using CoursesManager.MVVM.Dialogs;
using CoursesManager.UI.Dialogs.ResultTypes;
using CoursesManager.UI.Dialogs.ViewModels;
using System.Windows;
using CoursesManager.UI.Repositories.RegistrationRepository;
using CoursesManager.UI.Repositories.StudentRepository;
using CoursesManager.UI.Repositories.CourseRepository;

namespace CoursesManager.Tests
{
    [TestFixture]
    [Apartment(System.Threading.ApartmentState.STA)]
    public class EditStudentViewModelTests
    {
        private Mock<IStudentRepository> _studentRepositoryMock;
        private Mock<ICourseRepository> _courseRepositoryMock;
        private Mock<IRegistrationRepository> _registrationRepositoryMock;
        private Mock<IDialogService> _dialogServiceMock;
        private EditStudentViewModel _viewModel;
        private Student _student;

        [SetUp]
        public void SetUp()
        {
            _student = new Student
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
            };

            _studentRepositoryMock = new Mock<IStudentRepository>();
            _courseRepositoryMock = new Mock<ICourseRepository>();
            _registrationRepositoryMock = new Mock<IRegistrationRepository>();
            _dialogServiceMock = new Mock<IDialogService>();

            // Mock course repository
            _courseRepositoryMock.Setup(repo => repo.GetAll())
                .Returns(new List<Course>
                {
                    new Course { ID = 1, Name = "Math" },
                    new Course { ID = 2, Name = "Science" }
                });

            // Mock registration repository
            _registrationRepositoryMock.Setup(repo => repo.GetAll())
                .Returns(new List<Registration>
                {
                    new Registration { StudentID = 1, CourseID = 1 }
                });

            // Initialize ViewModel
            _viewModel = new EditStudentViewModel(
                _studentRepositoryMock.Object,
                _courseRepositoryMock.Object,
                _registrationRepositoryMock.Object,
                _dialogServiceMock.Object,
                _student);
        }

        [Test]
        public async Task OnSaveAsync_ShouldUpdateStudentAndRegistrations_WhenValidationSucceeds()
        {
            // Arrange
            _viewModel.ParentWindow = new Mock<Window>().Object; // Ensure ParentWindow is set
            _viewModel.SelectableCourses.First(c => c.ID == 2).IsSelected = true; // Select "Science" course
    
            var confirmationResult = DialogResult<DialogResultType>.Builder()
                .SetSuccess(new DialogResultType { Result = true }, "Confirmed")
                .Build();

            _dialogServiceMock
                .Setup(ds => ds.ShowDialogAsync<ConfirmationDialogViewModel, DialogResultType>(
                    It.Is<DialogResultType>(result => result.DialogText == "Wilt u de wijzigingen opslaan?")))
                .ReturnsAsync(confirmationResult);

            _dialogServiceMock
                .Setup(ds => ds.ShowDialogAsync<NotifyDialogViewModel, DialogResultType>(It.IsAny<DialogResultType>()))
                .ReturnsAsync(DialogResult<DialogResultType>.Builder().SetSuccess(new DialogResultType(), "Notification").Build());

            // Act
            await _viewModel.OnSaveAsync();

            // Assert
            _dialogServiceMock.Verify(service =>
                    service.ShowDialogAsync<ConfirmationDialogViewModel, DialogResultType>(
                        It.Is<DialogResultType>(result => result.DialogText == "Wilt u de wijzigingen opslaan?")),
                Times.Once);

            _studentRepositoryMock.Verify(repo => repo.Update(It.IsAny<Student>()), Times.Once);
            _registrationRepositoryMock.Verify(repo => repo.Add(It.Is<Registration>(r => r.CourseID == 2)), Times.Once);

            _dialogServiceMock.Verify(service =>
                    service.ShowDialogAsync<NotifyDialogViewModel, DialogResultType>(
                        It.Is<DialogResultType>(result => result.DialogText == "Cursist succesvol opgeslagen.")),
                Times.Once);
        }

        [Test]
        public async Task OnSaveAsync_ShouldNotSave_WhenValidationFails()
        {
            // Arrange
            _viewModel.ParentWindow = null; // Simulate missing ParentWindow to cause validation failure

            _dialogServiceMock
                .Setup(ds => ds.ShowDialogAsync<NotifyDialogViewModel, DialogResultType>(
                    It.Is<DialogResultType>(result => result.DialogText == "Parentvenster is niet ingesteld.")));

            // Act
            await _viewModel.OnSaveAsync();

            // Assert
            // Verify that the Student repository's Update method was not called
            _studentRepositoryMock.Verify(repo => repo.Update(It.IsAny<Student>()), Times.Never);

            // Verify that the notification dialog was shown once
            _dialogServiceMock.Verify(ds =>
                    ds.ShowDialogAsync<NotifyDialogViewModel, DialogResultType>(
                        It.Is<DialogResultType>(result => result.DialogText == "Parentvenster is niet ingesteld.")),
                Times.Once);

            // Ensure no other dialog calls were made
            _dialogServiceMock.VerifyNoOtherCalls();
        }

    }
}