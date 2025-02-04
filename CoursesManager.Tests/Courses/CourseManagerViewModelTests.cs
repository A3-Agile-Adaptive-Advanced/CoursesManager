﻿using System.Collections.ObjectModel;
using CoursesManager.MVVM.Dialogs;
using CoursesManager.MVVM.Messages;
using CoursesManager.MVVM.Navigation;
using CoursesManager.UI.Models;
using CoursesManager.UI.Repositories.CourseRepository;
using CoursesManager.UI.ViewModels;
using CoursesManager.UI.ViewModels.Courses;
using Moq;
using System.Diagnostics;

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
        ObservableCollection<Course> _courses;

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
                    IsActive = true,
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
                    IsActive = false,
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
                    IsActive = true,
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
                    IsActive = true,
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
                    IsActive = false,
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
            Assert.That(_viewModel.Courses, Has.Count.EqualTo(_courses.Count));
        }

        //[Test]
        //public async Task Should_Filter_Courses_Based_On_SearchTerm()
        //{
        //    // Filter by "WIS101"
        //    _viewModel.SearchTerm = "WIS101";
        //    await Task.Delay(500);
        //    Assert.That(_viewModel.FilteredCourses, Has.Count.EqualTo(1));
        //    Assert.That(
        //        _viewModel.FilteredCourses,
        //        Has.All.Matches<Course>(c =>
        //            c.GenerateFilterString().Contains(
        //                _viewModel.SearchTerm,
        //                StringComparison.CurrentCultureIgnoreCase
        //            )
        //        )
        //    );

        //    // Filter by "Basis"
        //    _viewModel.SearchTerm = "Basis";
        //    await Task.Delay(500);
        //    Assert.That(_viewModel.FilteredCourses, Has.Count.EqualTo(2));
        //    Assert.That(
        //        _viewModel.FilteredCourses,
        //        Has.All.Matches<Course>(c =>
        //            c.GenerateFilterString().Contains(
        //                _viewModel.SearchTerm,
        //                StringComparison.CurrentCultureIgnoreCase
        //            )
        //        )
        //    );

        //    //    // Reset SearchTerm
        //    //    _viewModel.SearchTerm = String.Empty;
        //    //}
        //}

        [Test]
        public async Task Should_Return_Empty_When_SearchTerm_Does_Not_Match_Any_Course()
        {
            _viewModel.SearchTerm = "NonExistentCourseName";
            await Task.Delay(50);

            Assert.That(_viewModel.FilteredCourses, Is.Empty);
        }

        [Test]
        public async Task Should_Toggle_Course_Active_Status_When_IsSwitchToggled_Changes()
        {
            // Toggling to true should filter active courses only
            _viewModel.IsSwitchToggled = true;
            await Task.Delay(50);

            var activeCourses = _courses.Where(c => c.IsActive).ToList();
            Assert.That(_viewModel.FilteredCourses, Has.Count.EqualTo(activeCourses.Count));
            Assert.That(_viewModel.FilteredCourses.All(c => c.IsActive), Is.True);


            // Toggle back to false should show only inactive courses
            _viewModel.IsSwitchToggled = false;
            await Task.Delay(50);

            var inactiveCourses = _courses.Where(c => c.IsActive == false).ToList();
            Assert.That(_viewModel.FilteredCourses, Has.Count.EqualTo(inactiveCourses.Count));
            Assert.That(_viewModel.FilteredCourses.All(c => !c.IsActive), Is.True);
        }

        [Test]
        public void Should_Navigate_To_Course_Overview_When_CourseOptionCommand_Executed()
        {
            var testCourse = _viewModel.Courses.First();
            _viewModel.CourseOptionCommand.Execute(testCourse);

            _mockedNavigationService.Verify(nav => 
                nav.NavigateTo<CourseOverViewViewModel>(),
                Times.Once
            );
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
                IsActive = true,
                StartDate = new DateTime(2024, 6, 1),
                EndDate = new DateTime(2024, 6, 10),
                DateCreated = DateTime.Now.AddDays(-7),
            };

            _mockedDialogService
                .Setup(ds => ds.ShowDialogAsync<CourseDialogViewModel, Course>())
                .ReturnsAsync(DialogResult<Course>.Builder().SetSuccess(newCourse, "Success").Build());

            Assert.That(_viewModel.Courses.Count, Is.EqualTo(5));

            _courses.Add(newCourse);
            _mockedCourseRepository.Setup(repo => repo.GetAll()).Returns(_courses);

            await Task.Run(() => _viewModel.AddCourseCommand.Execute(newCourse));
            await Task.Delay(50);

            Assert.That(_viewModel.Courses, Has.Count.EqualTo(6));
            Assert.That(_viewModel.Courses.Any(c => c == newCourse));
        }

        [Test]
        public async Task Should_Not_Add_Course_When_Dialog_Is_Cancelled()
        {
            _mockedDialogService
                .Setup(ds => ds.ShowDialogAsync<CourseDialogViewModel, Course>())
                .ReturnsAsync(DialogResult<Course>.Builder().SetFailure("User cancelled").Build());

            var originalCount = _viewModel.Courses.Count;

            await Task.Run(() => _viewModel.AddCourseCommand.Execute(null));
            await Task.Delay(50);

            Assert.That(_viewModel.Courses.Count, Is.EqualTo(originalCount));
        }

        [Test]
        public async Task Should_Not_Add_Course_When_User_Cancels_In_Dialog()
        {
            _mockedDialogService
                .Setup(ds => ds.ShowDialogAsync<CourseDialogViewModel, Course>())
                .ReturnsAsync(DialogResult<Course>.Builder().SetFailure("User cancelled").Build());

            var originalCount = _viewModel.Courses.Count;

            _viewModel.AddCourseCommand.Execute(null);
            await Task.Delay(50);

            Assert.That(_viewModel.Courses.Count, Is.EqualTo(originalCount));
        }

        [Test]
        public async Task Should_Not_Add_Course_When_Dialog_Returns_Null_Course()
        {
            _mockedDialogService
                .Setup(ds => ds.ShowDialogAsync<CourseDialogViewModel, Course>())
                .ReturnsAsync(DialogResult<Course>.Builder().SetSuccess(null, "Weird success").Build());

            var originalCount = _viewModel.Courses.Count;

            _viewModel.AddCourseCommand.Execute(null);
            await Task.Delay(50);

            Assert.That(_viewModel.Courses.Count, Is.EqualTo(originalCount));
        }

        [Test]
        public void Should_Throw_Exception_When_Repository_Throws_On_GetAll()
        {
            var brokenRepo = new Mock<ICourseRepository>();
            brokenRepo.Setup(repo => repo.GetAll()).Throws(new Exception("Database down!"));

            Assert.Throws<Exception>(() =>
            {
                var badViewModel = new CoursesManagerViewModel(
                    brokenRepo.Object,
                    _mockedMessageBroker.Object,
                    _mockedDialogService.Object,
                    _mockedNavigationService.Object
                );
            });
        }
    }
}
