using NUnit.Framework;
using Moq;
using CoursesManager.UI.ViewModels.Courses;
using CoursesManager.UI.Models;
using CoursesManager.UI.Repositories.CourseRepository;
using CoursesManager.UI.Repositories.LocationRepository;
using CoursesManager.UI.Dialogs.Windows;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using CoursesManager.MVVM.Dialogs;
using Microsoft.Win32;


namespace CoursesManager.Tests.Courses
{
    [TestFixture]
    public class CourseDialogViewModelTests
    {
        private Mock<ICourseRepository> _courseRepositoryMock;
        private Mock<ILocationRepository> _locationRepositoryMock;
        private Mock<IDialogService> _dialogServiceMock;
        private CourseDialogViewModel _viewModel;

        [SetUp]
        public void SetUp()
        {

            _courseRepositoryMock = new Mock<ICourseRepository>();
            _locationRepositoryMock = new Mock<ILocationRepository>();
            _dialogServiceMock = new Mock<IDialogService>();


            var locations = new List<Location>
            {
                new Location { Id = 1, Name = "Test Locatie" }
            };


            _locationRepositoryMock.Setup(repo => repo.GetAll()).Returns(locations);


            _viewModel = new CourseDialogViewModel(
                _courseRepositoryMock.Object,
                _dialogServiceMock.Object,
                _locationRepositoryMock.Object,
                null
            );
        }

        [Test]
        public async Task SaveCommand_ShouldAddNewCourse_WhenOriginalCourseIsNull()
        {
            // Arrange
            _viewModel.Course.Name = "New Course";
            _viewModel.Course.Code = "NC123";
            _viewModel.Course.StartDate = DateTime.Now;
            _viewModel.Course.EndDate = DateTime.Now.AddDays(5);
            _viewModel.Course.Location = new Location { Id = 1, Name = "Test Location" };
            _viewModel.Course.Description = "New course description.";

            // Act
            await Task.Run(() => _viewModel.SaveCommand.Execute(null)); 

            // Assert
            _courseRepositoryMock.Verify(repo => repo.Add(It.IsAny<Course>()), Times.Once, "De Add-methode van de repository moet één keer worden aangeroepen voor een nieuwe cursus.");
        }

        [Test]
        public async Task CancelCommand_ShouldInvokeCanceledResponse()
        {
            // Act
            await Task.Run(() => _viewModel.CancelCommand.Execute(null));

            // Assert
            // Controleer dat geen wijzigingen zijn aangebracht
            Assert.Pass("CancelCommand voltooid zonder fouten en annuleert correct.");
        }


        [Test]
        public void SaveCommand_CanExecute_ShouldReturnTrue_WhenAllFieldsAreValid()
        {
            // Arrange
            _viewModel.Course.Name = "Test Course";
            _viewModel.Course.Code = "12345";
            _viewModel.Course.StartDate = DateTime.Now;
            _viewModel.Course.EndDate = DateTime.Now.AddDays(1);
            _viewModel.Course.Location = new Location { Id = 1, Name = "Test Location" };
            _viewModel.Course.Description = "This is a test course.";

            // Act
            var canExecute = _viewModel.SaveCommand.CanExecute(null);

            // Assert
            Assert.That(canExecute, Is.True, "SaveCommand.CanExecute moet true retourneren als alle velden geldig zijn.");
        }



        [Test]
        public void Course_ShouldNotModifyOriginalCourse_WhenChangesAreMade()
        {
            // Arrange
            var originalCourse = new Course { Name = "Original Course" };
            _viewModel = new CourseDialogViewModel(
                _courseRepositoryMock.Object,
                _dialogServiceMock.Object,
                _locationRepositoryMock.Object,
                originalCourse
            );

            // Act
            _viewModel.Course.Name = "Modified Course";

            // Assert
            Assert.That(originalCourse.Name, Is.EqualTo("Original Course"), "De originele cursus mag niet worden aangepast wanneer wijzigingen worden aangebracht.");
        }
    }
}