using CoursesManager.MVVM.Data;
using CoursesManager.MVVM.Dialogs;
using CoursesManager.MVVM.Messages;
using CoursesManager.MVVM.Navigation;
using CoursesManager.UI.Factory;
using CoursesManager.UI.Mailing;
using CoursesManager.UI.Models;
using CoursesManager.UI.Repositories.CourseRepository;
using CoursesManager.UI.Repositories.RegistrationRepository;
using CoursesManager.UI.Repositories.StudentRepository;
using CoursesManager.UI.Service;
using CoursesManager.UI.Service.PlaceholderService;
using CoursesManager.UI.Service.TextHandlerService;
using CoursesManager.UI.ViewModels.Courses;
using CoursesManager.UI.ViewModels.Students;
using Moq;
using NUnit.Framework;

namespace CoursesManager.Tests.Factory
{
    [TestFixture]
    public class ViewModelFactoryTests
    {
        private Mock<IMessageBroker> _mockMessageBroker;
        private Mock<IDialogService> _mockDialogService;
        private Mock<IConfigurationService> _mockConfigurationService;
        private Mock<IMailProvider> _mockMailProvider;
        private Mock<IPlaceholderService> _mockPlaceholderService;
        private Mock<ITextHandlerService> _mockTextHandlerService;
        private Mock<INavigationService> _mockNavigationService;
        private RepositoryFactory _repositoryFactory;
        private ViewModelFactory _viewModelFactory;

        [SetUp]
        public void SetUp()
        {
            _mockMessageBroker = new Mock<IMessageBroker>();
            _mockDialogService = new Mock<IDialogService>();
            _mockConfigurationService = new Mock<IConfigurationService>();
            _mockMailProvider = new Mock<IMailProvider>();
            _mockPlaceholderService = new Mock<IPlaceholderService>();
            _mockTextHandlerService = new Mock<ITextHandlerService>();

            _repositoryFactory = new RepositoryFactory
            {
                StudentRegistrationCourseAggregator = new StudentRegistrationCourseAggregator(_repositoryFactory)
            };

            _viewModelFactory = new ViewModelFactory(
                _repositoryFactory,
                _mockMessageBroker.Object,
                _mockDialogService.Object,
                _mockConfigurationService.Object,
                _mockPlaceholderService.Object,
                _mockTextHandlerService.Object,
                _mockMailProvider.Object
            );
        }

        [Test]
        public void CreateViewModel_CourseOverViewViewModel_ReturnsInstance()
        {
            // Act
            var viewModel = _viewModelFactory.CreateViewModel<CourseOverViewViewModel>(_mockNavigationService.Object);

            // Assert
            Assert.That(viewModel, Is.Not.Null);
            Assert.That(viewModel, Is.InstanceOf<CourseOverViewViewModel>());
        }

        [Test]
        public void CreateViewModel_StudentManagerViewModel_ReturnsInstance()
        {
            // Act
            var viewModel = _viewModelFactory.CreateViewModel<StudentManagerViewModel>(_mockNavigationService.Object);

            // Assert
            Assert.That(viewModel, Is.Not.Null);
            Assert.That(viewModel, Is.InstanceOf<StudentManagerViewModel>());
        }

        [Test]
        public void CreateViewModel_StudentDetailViewModel_ReturnsInstance()
        {
            // Arrange
            var student = new Student();

            // Act
            var viewModel = _viewModelFactory.CreateViewModel<StudentDetailViewModel>(_mockNavigationService.Object, student);

            // Assert
            Assert.That(viewModel, Is.Not.Null);
            Assert.That(viewModel, Is.InstanceOf<StudentDetailViewModel>());
            Assert.That(viewModel.Student, Is.EqualTo(student));
        }

        [Test]
        public void CreateViewModel_UnknownViewModelType_ThrowsArgumentException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => _viewModelFactory.CreateViewModel<UnknownViewModel>(_mockNavigationService.Object));
            Assert.That(ex.Message, Is.EqualTo($"Unknown ViewModel type: {typeof(UnknownViewModel)}"));
        }

        private class UnknownViewModel : ViewModelWithNavigation
        {
            public UnknownViewModel(INavigationService navigationService) : base(navigationService)
            {
            }
        }
    }
}


