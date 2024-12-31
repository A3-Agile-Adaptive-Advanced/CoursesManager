using System.Collections.ObjectModel;
using System.Reflection;
using Moq;
using CoursesManager.MVVM.Dialogs;
using CoursesManager.MVVM.Messages;
using CoursesManager.MVVM.Navigation;
using CoursesManager.UI.Models;
using CoursesManager.UI.Repositories.RegistrationRepository;
using CoursesManager.UI.ViewModels.Students;
using CoursesManager.UI.ViewModels;

namespace CoursesManager.Tests.Students
{
    [TestFixture]
    public class StudentDetailViewModelTests
    {
        private Mock<IRegistrationRepository> _mockRegistrationRepository;
        private Mock<INavigationService> _mockNavigationService;
        private Mock<IDialogService> _mockDialogService;
        private Mock<IMessageBroker> _mockMessageBroker;

        private Student _testStudent;
        private StudentDetailViewModel _viewModel;

        [SetUp]
        public void SetUp()
        {
            _mockRegistrationRepository = new Mock<IRegistrationRepository>();
            _mockNavigationService = new Mock<INavigationService>();
            _mockDialogService = new Mock<IDialogService>();
            _mockMessageBroker = new Mock<IMessageBroker>();

            _testStudent = new Student
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Courses = new ObservableCollection<Course>
                {
                    new Course
                    {
                        Id = 1,
                        Name = "Math 101",
                        Code = "MATH101",
                        Description = "Basic Mathematics",
                        Participants = 30,
                        IsActive = true,
                        IsPayed = true,
                        Category = "Mathematics",
                        StartDate = DateTime.Now,
                        EndDate = DateTime.Now.AddMonths(3),
                        LocationId = 1,
                        Location = new Location
                        {
                            Id = 1,
                            Name = "Main Campus",
                            Address = new Address
                            {
                                Id = 1,
                                Country = "USA",
                                ZipCode = "12345",
                                City = "City",
                                Street = "Street",
                                HouseNumber = "123"
                            }
                        }
                    }
                }
            };

            var registration = new Registration
            {
                Id = 1,
                StudentId = _testStudent.Id,
                Student = _testStudent,
                CourseId = _testStudent.Courses.First().Id,
                Course = _testStudent.Courses.First(),
                RegistrationDate = DateTime.Now,
                PaymentStatus = true,
                IsActive = true,
                IsAchieved = false,
            };
            

            // Mock registration repository
            _mockRegistrationRepository.Setup(repo => repo.GetAll())
                .Returns(new List<Registration>
                {
                    new Registration { StudentId = 1, CourseId = 1 }
                });

            _viewModel = new StudentDetailViewModel(
                _mockDialogService.Object,
                _mockMessageBroker.Object,
                _mockRegistrationRepository.Object,
                _mockNavigationService.Object,
                _testStudent);
        }

        [Test]
        public void Constructor_InitializesProperties()
        {
            // Assert initial properties are set correctly
            Assert.That(_viewModel.Student, Is.EqualTo(_testStudent));
            Assert.That(_viewModel.EditStudent, Is.Not.Null);

        }

        [Test]
        public async Task OpenEditStudentPopup_CallsDialogServiceAndReloadsDetails()
        {
            // Arrange
            var dialogResult = DialogResult<Student>.Builder()
                .SetOutcome(DialogOutcome.Success)
                .Build();

            _mockDialogService.Setup(dialog => dialog.ShowDialogAsync<EditStudentViewModel, Student>(_testStudent))
                .ReturnsAsync(dialogResult);

            // Act
            await InvokePrivateMethodAsync("OpenEditStudentPopup", _testStudent);

            // Assert
            _mockDialogService.Verify(dialog => dialog.ShowDialogAsync<EditStudentViewModel, Student>(_testStudent),
                Times.Once);
        }

        private void InvokePrivateMethod(string methodName, params object[] parameters)
        {
            var method =
                typeof(StudentDetailViewModel).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(method, Is.Not.Null, $"Method '{methodName}' not found.");
            method?.Invoke(_viewModel, parameters);
        }

        private async Task InvokePrivateMethodAsync(string methodName, params object[] parameters)
        {
            var method =
                typeof(StudentDetailViewModel).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(method, Is.Not.Null, $"Method '{methodName}' not found.");
            var result = method?.Invoke(_viewModel, parameters);
            if (result is Task taskResult)
            {
                await taskResult;
            }
        }
    }
}