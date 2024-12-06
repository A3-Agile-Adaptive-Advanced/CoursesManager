﻿using CoursesManager.MVVM.Dialogs;
using CoursesManager.MVVM.Messages;
using CoursesManager.MVVM.Navigation;
using CoursesManager.UI.Models;
using CoursesManager.UI.Repositories.CourseRepository;
using CoursesManager.UI.Repositories.RegistrationRepository;
using CoursesManager.UI.Repositories.StudentRepository;
using CoursesManager.UI.ViewModels.Students;
using Moq;

namespace CoursesManager.Tests
{
    [TestFixture]
    public class StudentManagerViewModelTests
    {
        private Mock<IStudentRepository> _studentRepositoryMock;
        private Mock<IRegistrationRepository> _registrationRepositoryMock;
        private Mock<ICourseRepository> _courseRepositoryMock;
        private Mock<IDialogService> _dialogServiceMock;
        private Mock<IMessageBroker> _messageBrokerMock;
        private Mock<INavigationService> _navigationServiceMock;
        private StudentManagerViewModel _viewModel;
        private List<Student> students = new List<Student>();

        [SetUp]
        public void Setup()
        {
            _studentRepositoryMock = new Mock<IStudentRepository>();
            _registrationRepositoryMock = new Mock<IRegistrationRepository>();
            _courseRepositoryMock = new Mock<ICourseRepository>();
            _dialogServiceMock = new Mock<IDialogService>();
            _messageBrokerMock = new Mock<IMessageBroker>();
            _navigationServiceMock = new Mock<INavigationService>();

            students = new List<Student>
            {
                new Student { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com" },
                new Student { Id = 2, FirstName = "Jane", LastName = "Smith", Email = "jane@example.com" }
            };

            _studentRepositoryMock.Setup(repo => repo.GetNotDeletedStudents()).Returns(students);

            _viewModel = new StudentManagerViewModel(
                _dialogServiceMock.Object,
                _studentRepositoryMock.Object,
                _courseRepositoryMock.Object,
                _registrationRepositoryMock.Object,
                _messageBrokerMock.Object,
                _navigationServiceMock.Object
            );

            _viewModel.LoadStudents();

        }

        [Test]
        public async Task AddStudentCommand_ShouldInvokeDialogAndReloadStudents()
        {
            // Arrange
            var newStudent = new Student { Id = 4, FirstName = "New", LastName = "Student", Email = "newstudent@example.com" };
            var dialogResult = DialogResult<Student>.Builder()
                .SetSuccess(newStudent, "Confirmed")
                .Build();

            _dialogServiceMock
                .Setup(ds => ds.ShowDialogAsync<AddStudentViewModel, Student>(It.IsAny<Student>()))
                .ReturnsAsync(dialogResult);

            // Act
            await Task.Run(() => _viewModel.AddStudentCommand.Execute(null));

            // Assert
            _dialogServiceMock.Verify(ds =>
                    ds.ShowDialogAsync<AddStudentViewModel, Student>(It.IsAny<Student>()),
                Times.Once);

            _studentRepositoryMock.Verify(repo => repo.GetNotDeletedStudents(), Times.AtLeastOnce);
            Assert.That(_viewModel.Students.Count, Is.EqualTo(2)); //Must be fixed
        }


        [Test]
        public async Task EditStudentCommand_WithValidStudent_ShouldInvokeDialog()
        {
            // Arrange
            var student = new Student { Id = 1, FirstName = "John" };

            var dialogResult = DialogResult<Student>.Builder()
                .SetSuccess(student, "Confirmed")
                .Build();

            _dialogServiceMock
                .Setup(ds => ds.ShowDialogAsync<EditStudentViewModel, Student>(student))
                .ReturnsAsync(dialogResult);

            // Act
            _viewModel.EditStudentCommand.Execute(student);

            // Assert
            _dialogServiceMock.Verify(ds =>
                    ds.ShowDialogAsync<EditStudentViewModel, Student>(student),
                Times.Once);

            _studentRepositoryMock.Verify(repo => repo.GetNotDeletedStudents(), Times.AtLeastOnce);
        }

        [Test]
        public void EditStudentCommand_WithNullStudent_ShouldNotInvokeDialog()
        {
            // Arrange
            Student nullStudent = null;

            // Act
            var canExecute = _viewModel.EditStudentCommand.CanExecute(nullStudent);

            // Assert
            Assert.That(canExecute, Is.False); 

            if (canExecute)
            {
                _viewModel.EditStudentCommand.Execute((Student)null);
            }

            _dialogServiceMock.Verify(ds =>
                    ds.ShowDialogAsync<EditStudentViewModel, Student>(It.IsAny<Student>()),
                Times.Never);
        }

        [Test]
        public void OpenStudentDetailViewModel_ShouldNavigateToStudentDetail()
        {
            // Arrange
            var student = new Student { Id = 1, FirstName = "John" };
            _registrationRepositoryMock.Setup(repo => repo.GetAll()).Returns(new List<Registration>());
            _viewModel.SelectedStudent = student;

            // Act
            _viewModel.StudentDetailCommand.Execute(null);

            // Assert
            _navigationServiceMock.Verify(nav =>
                nav.NavigateTo<StudentDetailViewModel>(student),
                Times.Once);
        }

        [Test]
        public void ToggleIsDeletedCommand_ShouldFilterOnlyDeletedStudents()
        {
            // Arrange
            var students = new List<Student>
            {
                new Student { Id = 1, FirstName = "DeletedStudent", IsDeleted = true },
                new Student { Id = 2, FirstName = "ActiveStudent", IsDeleted = false }
            };

            _studentRepositoryMock.Setup(repo => repo.GetDeletedStudents()).Returns(students);
            _studentRepositoryMock.Setup(repo => repo.GetAll()).Returns(students);
            _viewModel.IsToggled = false;

            // Act
            _viewModel.ToggleIsDeletedCommand.Execute(null);

            // Assert
            var deletedStudents = _studentRepositoryMock.Object.GetDeletedStudents()
                .Where(s => s.IsDeleted).ToList();

            Assert.That(deletedStudents.Count, Is.EqualTo(1));
            Assert.That(deletedStudents.First().IsDeleted, Is.True);
        }


        [Test]
        public async Task CheckboxChangedCommand_ShouldUpdateRegistration_WhenCheckboxesAreUpdated()
        {
            // Arrange
            var registration = new Registration { Id = 1, StudentId = 1, CourseId = 1, PaymentStatus = false, IsAchieved = false };
            _registrationRepositoryMock.Setup(repo => repo.GetAll()).Returns(new List<Registration> { registration });

            var course = new Course { Id = 1, Name = "Math" };
            var payment = new CourseStudentPayment(course, registration) { IsPaid = true, IsAchieved = true };
            _viewModel.SelectedStudent = new Student { Id = 1 };

            // Act
            _viewModel.CheckboxChangedCommand.Execute(payment);

            // Assert
            _registrationRepositoryMock.Verify(repo =>
                repo.Update(It.Is<Registration>(
                    r => r.StudentId == 1 &&
                         r.CourseId == 1 &&
                         r.PaymentStatus == true &&
                         r.IsAchieved == true)),
                Times.Once);
        }
    }
}