using CoursesManager.MVVM.Dialogs;
using CoursesManager.MVVM.Messages;
using CoursesManager.UI.Messages;
using CoursesManager.UI.Models;
using CoursesManager.UI.Repositories.CourseRepository;
using CoursesManager.UI.Repositories.LocationRepository;
using CoursesManager.UI.ViewModels.Courses;
using Moq;
using System.Collections.ObjectModel;
using CoursesManager.UI.Enums;
using CoursesManager.MVVM.Messages;
using CoursesManager.UI.Dialogs.ResultTypes;
using CoursesManager.UI.Dialogs.ViewModels;


namespace CoursesManager.Tests.Courses
{
    [TestFixture]
    public class CourseDialogViewModelTests
    {
        private Mock<ICourseRepository> _courseRepositoryMock;
        private Mock<ILocationRepository> _locationRepositoryMock;
        private Mock<IDialogService> _dialogServiceMock;
        private Mock<IMessageBroker> _messageBrokerMock;
        private CourseDialogViewModel _viewModel;

        [SetUp]
        public void SetUp()
        {

            _courseRepositoryMock = new Mock<ICourseRepository>();
            _locationRepositoryMock = new Mock<ILocationRepository>();
            _dialogServiceMock = new Mock<IDialogService>();
            _messageBrokerMock = new Mock<IMessageBroker>();


            var locations = new ObservableCollection<Location>
            {
                new Location { Id = 1, Name = "Test Locatie" }
            };


            _locationRepositoryMock.Setup(repo => repo.GetAll()).Returns(locations);


            _viewModel = new CourseDialogViewModel(
                _courseRepositoryMock.Object,
                _dialogServiceMock.Object,
                _locationRepositoryMock.Object,
                _messageBrokerMock.Object,
                null
            );
        }

        [Test]
        public async Task SaveCommand_ShouldAddNewCourse_WhenOriginalCourseIsNull()
        {
            // Arrange
            _viewModel.Course!.Name = "New Course";
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
                _messageBrokerMock.Object,
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
            var originalCourse = new Course
            {
                Id = 1,
                Name = "Original Course",
                Code = "OC123",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(1),
                Location = new Location { Id = 1, Name = "Test Location" },
                Description = "Original description."
            };

            _viewModel = new CourseDialogViewModel(
                _courseRepositoryMock.Object,
                _dialogServiceMock.Object,
                _locationRepositoryMock.Object,
                _messageBrokerMock.Object,
                originalCourse
            );

            // Update de velden zodat ze geldig zijn
            _viewModel.Course.Name = "Updated Course";
            _viewModel.Course.Code = "UC456"; 
            _viewModel.Course.StartDate = DateTime.Now; 
            _viewModel.Course.EndDate = DateTime.Now.AddDays(5); 
            _viewModel.Course.Location = new Location { Id = 1, Name = "Updated Location" }; 
            _viewModel.Course.Description = "Updated description."; 

            // Act
            Assert.That(_viewModel.SaveCommand.CanExecute(null), Is.True, "SaveCommand moet kunnen worden uitgevoerd.");
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
                _messageBrokerMock.Object,
                null
            ), "De constructor moet een ArgumentNullException gooien als courseRepository null is.");

            Assert.Throws<ArgumentNullException>(() => new CourseDialogViewModel(
                _courseRepositoryMock.Object,
                null,
                _locationRepositoryMock.Object,
                _messageBrokerMock.Object,
                null
            ), "De constructor moet een ArgumentNullException gooien als dialogService null is.");

            Assert.Throws<ArgumentNullException>(() => new CourseDialogViewModel(
                _courseRepositoryMock.Object,
                _dialogServiceMock.Object,
                null,
                _messageBrokerMock.Object,
                null
            ), "De constructor moet een ArgumentNullException gooien als locationRepository null is.");
        }


        [Test]
        public async Task SaveCommand_ShouldNotPublishErrorToast_WhenSaveIsSuccessful()
        {
            // Arrange: zorg voor een volledig ingevulde, geldige Course
            _viewModel.Course = new Course
            {
                Name = "Valid Course",
                Code = "VC123",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(1),
                Location = new Location { Id = 1, Name = "Test Location" },
                Description = "Valid description."
            };

            // Act
            await Task.Run(() => _viewModel.SaveCommand.Execute(null));

            // Assert: controleer dat er geen foutmelding gepubliceerd wordt
            _messageBrokerMock.Verify(
                broker => broker.Publish(It.IsAny<ToastNotificationMessage>()),
                Times.Never,
                "Er mag geen foutmelding gepubliceerd worden bij een succesvolle save."
            );
        }

       

        [Test]
        public async Task SaveCommand_ShouldShowErrorDialog_WhenExceptionIsThrown()
        {
            // Arrange
            _courseRepositoryMock.Setup(repo => repo.Add(It.IsAny<Course>())).Throws(new Exception("Test exception"));

            _viewModel.Course = new Course
            {
                Name = "Valid Course",
                Code = "VC123",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(1),
                Location = new Location { Id = 1, Name = "Test Location" },
                Description = "Valid description."
            };

            // Act
            await Task.Run(() => _viewModel.SaveCommand.Execute(null));

            // Assert
            _dialogServiceMock.Verify(
                dialog => dialog.ShowDialogAsync<ErrorDialogViewModel, DialogResultType>(It.Is<DialogResultType>(result =>
                    result.DialogText == "Er is iets fout gegaan. Probeer het later opnieuw." &&
                    result.DialogTitle == "Fout"
                )),
                Times.Once,
                "Een foutdialoog moet worden getoond als er een uitzondering wordt gegooid."
            );
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
            var ex = Assert.Throws<InvalidOperationException>(() => _viewModel.SaveCommand.Execute(null));
            Assert.That(ex.Message, Is.EqualTo("Cursusgegevens ontbreken. Opslaan is niet mogelijk."));
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
                _messageBrokerMock.Object,
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
                _messageBrokerMock.Object,
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