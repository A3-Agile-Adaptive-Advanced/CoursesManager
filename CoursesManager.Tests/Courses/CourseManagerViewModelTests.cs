using CoursesManager.MVVM.Dialogs;
using CoursesManager.MVVM.Messages;
using CoursesManager.MVVM.Navigation;
using CoursesManager.UI.Models;
using CoursesManager.UI.Repositories.CourseRepository;
using CoursesManager.UI.ViewModels;
using CoursesManager.UI.ViewModels.Courses;
using Moq;

namespace CoursesManager.Tests.Courses
{
    [TestFixture]
    public class CourseManagerViewModelTests
    {
        private Mock<ICourseRepository> _mockedCourseRepository;
        private Mock<IDialogService> _mockedDialogService;
        private Mock<IMessageBroker> _mockedMessageBroker;
        private Mock<INavigationService> _mockedNavigationService;
        private CoursesManagerViewModel _viewModel;
        List<Course> _courses;

        [SetUp]
        public void Setup()
        {
            _mockedCourseRepository = new Mock<ICourseRepository>();
            _mockedMessageBroker = new Mock<IMessageBroker>();
            _mockedDialogService = new Mock<IDialogService>();
            _mockedNavigationService = new Mock<INavigationService>();

            _courses = new()
            {
                new Course
                {
                    Id = 1,
                    Name = "Basiscursus Wiskunde",
                    Code = "WIS101",
                    Category = "Wetenschap",
                    Description = "Inleidende cursus over fundamentele wiskundige concepten.",
                    Participants = 25,
                    IsActive = true,
                    IsPayed = false,
                    StartDate = new DateTime(2024, 1, 10),
                    EndDate = new DateTime(2024, 1, 15),
                    DateCreated = DateTime.Now,
                },
                new Course
                {
                    Id = 2,
                    Name = "Introductie tot Kunstgeschiedenis",
                    Code = "KUN201",
                    Category = "Kunst & Cultuur",
                    Description = "Een breed overzicht van kunststromingen door de eeuwen heen.",
                    Participants = 15,
                    IsActive = false,
                    IsPayed = true,
                    StartDate = new DateTime(2024, 2, 1),
                    EndDate = new DateTime(2024, 2, 10),
                    DateCreated = DateTime.Now.AddDays(-10),
                },
                new Course
                {
                    Id = 3,
                    Name = "Geavanceerde Programmeertechnieken",
                    Code = "IT301",
                    Category = "Informatica",
                    Description = "Verdiepende cursus over softwareontwerp en patroongebruik.",
                    Participants = 30,
                    IsActive = true,
                    IsPayed = true,
                    StartDate = new DateTime(2024, 3, 5),
                    EndDate = new DateTime(2024, 3, 20),
                    DateCreated = DateTime.Now.AddDays(-30),
                },
                new Course
                {
                    Id = 4,
                    Name = "Basis Spaans voor Beginners",
                    Code = "TAL101",
                    Category = "Talen",
                    Description = "Eerste stappen in de Spaanse taal, gericht op conversatie.",
                    Participants = 20,
                    IsActive = true,
                    IsPayed = false,
                    StartDate = new DateTime(2024, 4, 12),
                    EndDate = new DateTime(2024, 4, 25),
                    DateCreated = DateTime.Now.AddDays(-5),
                },
                new Course
                {
                    Id = 5,
                    Name = "Gezonde Voeding en Leefstijl",
                    Code = "VOE401",
                    Category = "Gezondheid",
                    Description = "Inzicht in voedingsprincipes en hoe een gezonde leefstijl vol te houden.",
                    Participants = 10,
                    IsActive = false,
                    IsPayed = false,
                    StartDate = new DateTime(2024, 5, 3),
                    EndDate = new DateTime(2024, 5, 15),
                    DateCreated = DateTime.Now.AddDays(-20),
                }
            };

            _mockedCourseRepository.Setup(repository => repository.GetAll()).Returns(_courses);

            _viewModel = new CoursesManagerViewModel(
                _mockedCourseRepository.Object,
                _mockedMessageBroker.Object,
                _mockedDialogService.Object,
                _mockedNavigationService.Object
            );
        }

        [Test]
        public void Should_Load_Courses_On_Initialization()
        {
            Assert.That(_courses, Has.Count.EqualTo(_viewModel.Courses.Count));
            Assert.That(_courses, Has.Count.EqualTo(_viewModel.FilteredCourses.Count));
        }

        [Test]
        public async Task Should_Filter_Courses_Based_On_SearchText()
        {
            // Filter by "WIS101"
            _viewModel.SearchText = "WIS101";
            await Task.Delay(50);
            Assert.That(_viewModel.FilteredCourses, Has.Count.EqualTo(1));
            Assert.That(
                _viewModel.FilteredCourses,
                Has.All.Matches<Course>(c => c.GenerateFilterString().Contains(_viewModel.SearchText, StringComparison.CurrentCultureIgnoreCase))
            );

            // Filter by "Basis"
            _viewModel.SearchText = "Basis";
            await Task.Delay(50);
            Assert.That(_viewModel.FilteredCourses, Has.Count.EqualTo(2));
            Assert.That(
                _viewModel.FilteredCourses,
                Has.All.Matches<Course>(c => c.GenerateFilterString().Contains(_viewModel.SearchText, StringComparison.CurrentCultureIgnoreCase))
            );
        }

        [Test]
        public async Task Should_Toggle_Course_Active_Status_When_IsToggled_Changes()
        {
            _viewModel.IsToggled = true;
            await Task.Delay(50);

            Assert.Multiple(() =>
            {
                Assert.That(_viewModel.IsToggled, Is.True);
                Assert.That(_viewModel.FilteredCourses, Has.Count.EqualTo(3));
            });
            Assert.That(_viewModel.FilteredCourses.All(c => c.IsActive), Is.True);

            _viewModel.IsToggled = false;
            await Task.Delay(50);

            Assert.Multiple(() =>
            {
                Assert.That(_viewModel.IsToggled, Is.False);
                Assert.That(_viewModel.FilteredCourses, Has.Count.EqualTo(2));
                Assert.That(_viewModel.FilteredCourses.All(c => c.IsActive), Is.False);
            });
        }

        [Test]
        public void Should_Navigate_To_Course_Overview_When_CourseOptionCommand_Executed()
        {
            var testCourse = _viewModel.Courses.First();
            _viewModel.CourseOptionCommand.Execute(testCourse);

            _mockedNavigationService.Verify(nav => nav.NavigateTo<CourseOverViewViewModel>(), Times.Once);
        }

        [Test]
        public async Task Should_Show_Dialog_And_Reload_Courses_When_AddCourseCommand_Executed()
        {
            // Setup dialog result to return a successful new course
            var newCourse = new Course
            {
                Id = 6,
                Name = "Inleiding Psychologie",
                Code = "PSY202",
                Category = "Mens & Gedrag",
                Description = "Een basiscursus over de belangrijkste theorieën en stromingen binnen de psychologie.",
                Participants = 18,
                IsActive = true,
                IsPayed = true,
                StartDate = new DateTime(2024, 6, 1),
                EndDate = new DateTime(2024, 6, 10),
                DateCreated = DateTime.Now.AddDays(-7),
            };

            _mockedDialogService.Setup(ds => ds.ShowDialogAsync<CourseDialogViewModel, Course>())
                .ReturnsAsync(DialogResult<Course>.Builder().SetSuccess(newCourse, "Success").Build());

            Assert.That(_viewModel.Courses.Count, Is.EqualTo(5));

            _courses.Add(newCourse);
            _mockedCourseRepository.Setup(repo => repo.GetAll()).Returns(_courses);

            await Task.Run(() => _viewModel.AddCourseCommand.Execute(newCourse));
            await Task.Delay(100);

            Assert.That(_viewModel.Courses, Has.Count.EqualTo(6));
            Assert.That(_viewModel.Courses.Any(c => c == newCourse));
        }
    }
}
