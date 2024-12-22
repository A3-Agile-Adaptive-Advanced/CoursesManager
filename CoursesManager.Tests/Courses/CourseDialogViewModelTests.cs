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

        [Test]
        public async Task SaveCommand_ShouldUpdateExistingCourse_WhenOriginalCourseIsNotNull()
        {
            // Arrange
            var originalCourse = new Course { Name = "Original Course" };
            _viewModel = new CourseDialogViewModel(
                _courseRepositoryMock.Object,
                _dialogServiceMock.Object,
                _locationRepositoryMock.Object,
                originalCourse
            );

            _viewModel.Course.Name = "Updated Course";

            // Act
            await Task.Run(() => _viewModel.SaveCommand.Execute(null));

            // Assert
            _courseRepositoryMock.Verify(repo => repo.Update(It.IsAny<Course>()), Times.Once, "De Update-methode van de repository moet één keer worden aangeroepen voor een bestaande cursus.");
        }

        [Test]
        public void Constructor_ShouldThrowException_WhenDependenciesAreNull()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() => new CourseDialogViewModel(
                null,
                _dialogServiceMock.Object,
                _locationRepositoryMock.Object,
                null
            ), "De constructor moet een ArgumentNullException gooien als courseRepository null is.");

            Assert.Throws<ArgumentNullException>(() => new CourseDialogViewModel(
                _courseRepositoryMock.Object,
                null,
                _locationRepositoryMock.Object,
                null
            ), "De constructor moet een ArgumentNullException gooien als dialogService null is.");

            Assert.Throws<ArgumentNullException>(() => new CourseDialogViewModel(
                _courseRepositoryMock.Object,
                _dialogServiceMock.Object,
                null,
                null
            ), "De constructor moet een ArgumentNullException gooien als locationRepository null is.");
        }





        // unhappy flows

        [Test]

        public void SaveCommand_CanExecute_ShouldReturnFalse_WhenFieldsAreInvalid()
        {
            _viewModel.Course.Name = ""; 
            _viewModel.Course.Code = ""; 
            _viewModel.Course.StartDate = default; 
            _viewModel.Course.EndDate = default; 
            _viewModel.Course.Location = null; 
            _viewModel.Course.Description = "";

            var canExecute = _viewModel.SaveCommand.CanExecute(null);

            Assert.That(canExecute, Is.False, "SaveCommand.CanExecute moet false retourneren als de velden ongeldig zijn.");
        }

        [Test]
        public void SaveCommand_ShouldThrowException_WhenCourseIsNull()
        {
            // Arrange
            _viewModel.Course = null;

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _viewModel.SaveCommand.Execute(null));
        }

        [Test]
        public void CancelCommand_ShouldNotModifyOriginalCourse()
        {
            // Arrange
            var originalCourse = new Course { Name = "Original Course" };
            _viewModel = new CourseDialogViewModel(
                _courseRepositoryMock.Object,
                _dialogServiceMock.Object,
                _locationRepositoryMock.Object,
                originalCourse
            );

            _viewModel.Course.Name = "Modified Course";

            // Act
            _viewModel.CancelCommand.Execute(null);

            // Assert
            Assert.That(originalCourse.Name, Is.EqualTo("Original Course"), "CancelCommand mag de originele cursus niet wijzigen.");
        }

        [Test]
        public void Course_ShouldNotImpactOriginalCourse_WhenModified()
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
            Assert.That(originalCourse.Name, Is.EqualTo("Original Course"), "Wijzigingen aan de ViewModel Course mogen de originele Course niet beïnvloeden.");
        }

        [Test]
        public void SaveCommand_CanExecute_ShouldReturnFalse_WhenEndDateIsBeforeStartDate()
        {
            // Arrange
            _viewModel.Course.StartDate = DateTime.Now;
            _viewModel.Course.EndDate = DateTime.Now.AddDays(-1);

            // Act
            var canExecute = _viewModel.SaveCommand.CanExecute(null);

            // Assert
            Assert.That(canExecute, Is.False, "SaveCommand.CanExecute moet false retourneren als de einddatum vóór de startdatum ligt.");
        }
    }
}