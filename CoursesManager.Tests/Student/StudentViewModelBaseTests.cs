using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CoursesManager.UI.Models;
using CoursesManager.UI.Repositories.CourseRepository;
using CoursesManager.UI.Repositories.RegistrationRepository;
using CoursesManager.UI.Repositories.StudentRepository;
using CoursesManager.UI.ViewModels.Students;
using CoursesManager.MVVM.Dialogs;
using System.Reflection;

[TestFixture]
public class StudentViewModelBaseTests
{
    private Mock<IStudentRepository> _studentRepositoryMock;
    private Mock<ICourseRepository> _courseRepositoryMock;
    private Mock<IRegistrationRepository> _registrationRepositoryMock;
    private Mock<IDialogService> _dialogServiceMock;
    private StudentViewModelBase _viewModel;
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

        _courseRepositoryMock.Setup(repo => repo.GetAll())
            .Returns(new List<Course>
            {
                new Course { Id = 1, Name = "Math", IsActive = true },
                new Course { Id = 2, Name = "Science", IsActive = true }
            });

        _registrationRepositoryMock.Setup(repo => repo.GetAll())
            .Returns(new List<Registration>
            {
                new Registration { StudentId = 1, CourseId = 1 }
            });

        _viewModel = new TestStudentViewModelBase(
            _studentRepositoryMock.Object,
            _courseRepositoryMock.Object,
            _registrationRepositoryMock.Object,
            _dialogServiceMock.Object,
            _student);
    }

    [Test]
    public void InitializeSelectableCourses_ShouldReturnSelectableCourses()
    {
        // Act
        var methodInfo = typeof(StudentViewModelBase).GetMethod("InitializeSelectableCourses", BindingFlags.NonPublic | BindingFlags.Instance);
        var result = (ObservableCollection<SelectableCourse>)methodInfo.Invoke(_viewModel, null);

        // Assert
        Assert.That(result, Is.Not.Null, "The result should not be null.");
        Assert.That(result.Count, Is.EqualTo(2), "The result should contain 2 courses.");
        Assert.That(result.Any(c => c.Id == 1 && c.IsSelected), Is.True, "Course with Id 1 should be selected.");
        Assert.That(result.Any(c => c.Id == 2 && !c.IsSelected), Is.True, "Course with Id 2 should not be selected."); }

    private class TestStudentViewModelBase : StudentViewModelBase
    {
        public TestStudentViewModelBase(
            IStudentRepository studentRepository,
            ICourseRepository courseRepository,
            IRegistrationRepository registrationRepository,
            IDialogService dialogService,
            Student student)
            : base(studentRepository, courseRepository, registrationRepository, dialogService, student)
        {
        }

        protected override Task OnSaveAsync()
        {
            throw new NotImplementedException();
        }
    }
}