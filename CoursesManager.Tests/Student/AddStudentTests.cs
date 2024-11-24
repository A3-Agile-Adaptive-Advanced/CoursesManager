using CoursesManager.UI.Models;
using CoursesManager.UI.ViewModels;
using CoursesManager.MVVM.Dialogs;
using Moq;
using NUnit.Framework;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using CoursesManager.UI.Dialogs.ResultTypes;
using CoursesManager.UI.Dialogs.ViewModels;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using CoursesManager.UI.Repositories.RegistrationRepository;
using CoursesManager.UI.Repositories.StudentRepository;
using CoursesManager.UI.Repositories.CourseRepository;

namespace CoursesManager.Tests
{
    [TestFixture]
    [Apartment(System.Threading.ApartmentState.STA)]
    public class AddStudentViewModelTests
    {
        private Mock<IStudentRepository> _mockStudentRepository;
        private Mock<ICourseRepository> _mockCourseRepository;
        private Mock<IRegistrationRepository> _mockRegistrationRepository;
        private Mock<IDialogService> _mockDialogService;
        private AddStudentViewModel _viewModel;

        [SetUp]
        public void SetUp()
        {

            _mockStudentRepository = new Mock<IStudentRepository>();
            _mockCourseRepository = new Mock<ICourseRepository>();
            _mockRegistrationRepository = new Mock<IRegistrationRepository>();
            _mockDialogService = new Mock<IDialogService>();

            _mockCourseRepository
                .Setup(repo => repo.GetAll())
                .Returns(new List<Course>
                {
                    new Course { ID = 1, Name = "Math" },
                    new Course { ID = 2, Name = "Science" }
                });

            _mockStudentRepository
                .Setup(repo => repo.GetAll())
                .Returns(new List<Student>
                {
                    new Student { Email = "existing@student.com" }
                });

            _viewModel = new AddStudentViewModel(
                initial: true,
                studentRepository: _mockStudentRepository.Object,
                courseRepository: _mockCourseRepository.Object,
                registrationRepository: _mockRegistrationRepository.Object,
                dialogService: _mockDialogService.Object
            );

            _viewModel.ParentWindow = new Window
                {
                    Content = new StackPanel
                    {
                        Children =
                        {
                            new TextBox { Name = "FirstName", Text = _viewModel.Student.FirstName },
                            new TextBox { Name = "LastName", Text = _viewModel.Student.LastName },
                            new TextBox { Name = "Email", Text = _viewModel.Student.Email },
                            new ComboBox { Name = "Course", SelectedItem = _viewModel.SelectedCourse }
                        }
                    }
                };
                
        }

        [Test]
        public async Task Save_ValidStudent_AddsStudentAndRegistration()
        {
            // Arrange
            _viewModel.Student.FirstName = "John";
            _viewModel.Student.LastName = "Doe";
            _viewModel.Student.Email = "new@student.com";
            _viewModel.Student.DateOfBirth = new DateTime(1990, 1, 1);
            _viewModel.SelectedCourse = "Math";

            // Act
            await _viewModel.Save();

            // Assert
            _mockStudentRepository.Verify(repo => repo.Add(It.IsAny<Student>()), Times.Once);
            _mockRegistrationRepository.Verify(repo => repo.Add(It.IsAny<Registration>()), Times.Once);
            _mockDialogService.Verify(service =>
                    service.ShowDialogAsync<NotifyDialogViewModel, DialogResultType>(
                        It.Is<DialogResultType>(result => result.DialogText == "Student succesvol toegevoegd")),
                Times.Once);
        }


        [Test]
        public async Task Save_DuplicateEmail_ShowsError()
        {
            // Arrange
            _viewModel.Student.FirstName = "John";
            _viewModel.Student.LastName = "Doe";
            _viewModel.Student.Email = "existing@student.com";
            _viewModel.Student.DateOfBirth = new DateTime(1990, 1, 1);
            _viewModel.SelectedCourse = "Math";

            // Act
            await _viewModel.Save();

            // Assert
            _mockStudentRepository.Verify(repo => repo.Add(It.IsAny<Student>()), Times.Never);
            _mockRegistrationRepository.Verify(repo => repo.Add(It.IsAny<Registration>()), Times.Never);
            _mockDialogService.Verify(service =>
                    service.ShowDialogAsync<NotifyDialogViewModel, DialogResultType>(
                        It.Is<DialogResultType>(result =>
                            result.DialogText.Contains("bestaat al."))),
                Times.Once);
        
        }

        [Test]
        public async Task Save_InvalidCourse_ShowsError()
        {
            // Arrange
            _viewModel.Student.FirstName = "John";
            _viewModel.Student.LastName = "Doe";
            _viewModel.Student.Email = "new@student.com";
            _viewModel.Student.DateOfBirth = new DateTime(1990, 1, 1);
            _viewModel.SelectedCourse = "InvalidCourse";

            // Act
            await _viewModel.Save();

            // Assert
            _mockStudentRepository.Verify(repo => repo.Add(It.IsAny<Student>()), Times.Never);
            _mockRegistrationRepository.Verify(repo => repo.Add(It.IsAny<Registration>()), Times.Never);
            _mockDialogService.Verify(service =>
                    service.ShowDialogAsync<NotifyDialogViewModel, DialogResultType>(
                        It.Is<DialogResultType>(result =>
                            result.DialogText.Contains("Geselecteerde cursus niet gevonden"))),
                Times.Once);
        }
    }
}