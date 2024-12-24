﻿using System.Reflection;
using CoursesManager.UI.Models;
using CoursesManager.MVVM.Dialogs;
using Moq;
using CoursesManager.UI.Dialogs.ResultTypes;
using CoursesManager.UI.Dialogs.ViewModels;
using System.Windows;
using System.Windows.Controls;
using CoursesManager.UI.Helpers;
using CoursesManager.UI.Repositories.RegistrationRepository;
using CoursesManager.UI.Repositories.StudentRepository;
using CoursesManager.UI.Repositories.CourseRepository;
using CoursesManager.UI.ViewModels.Students;
using CoursesManager.UI.Services;

namespace CoursesManager.Tests.Students                                                                 
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
        private Student _student;
        private Student addedStudent;

        [SetUp]
        public void SetUp()
        {
            _mockStudentRepository = new Mock<IStudentRepository>();
            _mockCourseRepository = new Mock<ICourseRepository>();
            _mockRegistrationRepository = new Mock<IRegistrationRepository>();
            _mockDialogService = new Mock<IDialogService>();

            _student = new Student();
            addedStudent = new Student
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "s@s.com",
                Phone = "1234567890",
                Address = new Address
                {
                    HouseNumber = "123",
                    Street = "Main St",
                    City = "City",
                    ZipCode = "1234AB",
                    Country = "Country",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                }
            };

            _mockCourseRepository
                .Setup(repo => repo.GetAll())
                .Returns(new List<Course>
                {
                    new Course { Id = 1, Name = "Math", IsActive = true },
                    new Course { Id = 2, Name = "Science", IsActive = true}
                });

            _mockRegistrationRepository
                .Setup(repo => repo.GetAll())
                .Returns(new List<Registration>
                {
                    new Registration { StudentId = 1, CourseId = 1 }
                });

            _mockStudentRepository
                .Setup(repo => repo.GetAll())
                .Returns(new List<Student>
                {
                    new Student { Email = "existing@example.com" }
                });

            _viewModel = new AddStudentViewModel(
                _student,
                studentRepository: _mockStudentRepository.Object,
                courseRepository: _mockCourseRepository.Object,
                registrationRepository: _mockRegistrationRepository.Object,
                dialogService: _mockDialogService.Object
            );
        }

        [Test]
        public async Task Save_ValidStudent_AddsStudentAndRegistration()
        {
            // Arrange
            BuildWindowForTestScenario(
                firstName: addedStudent.FirstName,
                lastName: addedStudent.LastName,
                email: addedStudent.Email,
                phone: addedStudent.Phone,
                dateOfBirth: new DateTime(1990, 1, 1),
                houseNumber: addedStudent.Address?.HouseNumber ?? string.Empty,
                zipCode: addedStudent.Address?.ZipCode ?? string.Empty,
                city: addedStudent.Address?.City ?? string.Empty,
                country: addedStudent.Address?.Country ?? string.Empty,
                street: addedStudent.Address?.Street ?? string.Empty,
                selectedCourse: "Math"
            );
            _viewModel.SelectedCourse = "Math";

            // Act
            await InvokeProtectedMethodAsync(_viewModel, "OnSaveAsync");

            // Assert
            _mockStudentRepository.Verify(repo => repo.Add(It.IsAny<Student>()), Times.Once);
            _mockRegistrationRepository.Verify(repo => repo.Add(It.IsAny<Registration>()), Times.Once);
            _mockDialogService.Verify(service =>
                    service.ShowDialogAsync<NotifyDialogViewModel, DialogResultType>(
                        It.Is<DialogResultType>(result => result.DialogText == "Student succesvol toegevoegd")),
                Times.Once);
        }

        [Test]
        public async Task Save_InvalidEmailFormat_ShowsError()
        {
            // Arrange
            BuildWindowForTestScenario(
                firstName: addedStudent.FirstName,
                lastName: addedStudent.LastName,
                email: "Invalid",
                phone: addedStudent.Phone,
                dateOfBirth: new DateTime(1990, 1, 1),
                houseNumber: addedStudent.Address?.HouseNumber ?? string.Empty,
                zipCode: addedStudent.Address?.ZipCode ?? string.Empty,
                city: addedStudent.Address?.City ?? string.Empty,
                country: addedStudent.Address?.Country ?? string.Empty,
                street: addedStudent.Address?.Street ?? string.Empty,
                selectedCourse: "Math"
            );
            _viewModel.SelectedCourse = "Math";

            // Act
            await InvokeProtectedMethodAsync(_viewModel, "OnSaveAsync");

            // Assert
            _mockStudentRepository.Verify(repo => repo.Add(It.IsAny<Student>()), Times.Never);
            _mockRegistrationRepository.Verify(repo => repo.Add(It.IsAny<Registration>()), Times.Never);
            _mockDialogService.Verify(service =>
                    service.ShowDialogAsync<NotifyDialogViewModel, DialogResultType>(
                        It.Is<DialogResultType>(result => result.DialogTitle == "Foutmelding")),
                Times.Once);
            _mockDialogService.Verify(service =>
                    service.ShowDialogAsync<NotifyDialogViewModel, DialogResultType>(
                        It.Is<DialogResultType>(result => result.DialogText == "Email is geen geldig e-mailadres.")),
                Times.Once);
        }

        [Test]
        public async Task Save_InvalidCourse_ShowsError()
        {
            // Arrange
            BuildWindowForTestScenario(
                firstName: addedStudent.FirstName,
                lastName: addedStudent.LastName,
                email: addedStudent.Email,
                phone: addedStudent.Phone,
                dateOfBirth: new DateTime(1990, 1, 1),
                houseNumber: addedStudent.Address?.HouseNumber ?? string.Empty,
                zipCode: addedStudent.Address?.ZipCode ?? string.Empty,
                city: addedStudent.Address?.City ?? string.Empty,
                country: addedStudent.Address?.Country ?? string.Empty,
                street: addedStudent.Address?.Street ?? string.Empty,
                selectedCourse: "InvalidCourse"
            );
            _viewModel.SelectedCourse = "InvalidCourse";

            // Act
            await InvokeProtectedMethodAsync(_viewModel, "OnSaveAsync");

            // Assert
            _mockStudentRepository.Verify(repo => repo.Add(It.IsAny<Student>()), Times.Never);
            _mockRegistrationRepository.Verify(repo => repo.Add(It.IsAny<Registration>()), Times.Never);
            _mockDialogService.Verify(service =>
                    service.ShowDialogAsync<NotifyDialogViewModel, DialogResultType>(
                        It.Is<DialogResultType>(result => result.DialogTitle == "Foutmelding")),
                Times.Once);
            _mockDialogService.Verify(service =>
                    service.ShowDialogAsync<NotifyDialogViewModel, DialogResultType>(
                        It.Is<DialogResultType>(result => result.DialogText == "Cursus is verplicht.")),
                Times.Once);
        }

        [Test]
        public async Task Save_DuplicateEmail_ShowsError()
        {
            // Arrange
            var existingEmails = new List<string> { "existing@example.com" };
            BuildWindowForTestScenario(
                firstName: addedStudent.FirstName,
                lastName: addedStudent.LastName,
                email: "existing@example.com",
                phone: addedStudent.Phone,
                dateOfBirth: new DateTime(1990, 1, 1),
                houseNumber: addedStudent.Address?.HouseNumber ?? string.Empty,
                zipCode: addedStudent.Address?.ZipCode ?? string.Empty,
                city: addedStudent.Address?.City ?? string.Empty,
                country: addedStudent.Address?.Country ?? string.Empty,
                street: addedStudent.Address?.Street ?? string.Empty,
                selectedCourse: "Math"
            );
            _viewModel.SelectedCourse = "Math";

            // Act
            var validationErrors = ValidationService.ValidateRequiredFields(_viewModel.ParentWindow, existingEmails);
            var errorMessage = string.Join("\n", validationErrors);

            _mockDialogService
                .Setup(ds => ds.ShowDialogAsync<NotifyDialogViewModel, DialogResultType>(
                    It.Is<DialogResultType>(result => result.DialogText == errorMessage)))
                .ReturnsAsync(DialogResult<DialogResultType>.Builder().SetFailure(errorMessage).Build());

            await InvokeProtectedMethodAsync(_viewModel, "OnSaveAsync");

            // Assert
            _mockStudentRepository.Verify(repo => repo.Add(It.IsAny<Student>()), Times.Never);
            _mockRegistrationRepository.Verify(repo => repo.Add(It.IsAny<Registration>()), Times.Never);
            _mockDialogService.Verify(service =>
                    service.ShowDialogAsync<NotifyDialogViewModel, DialogResultType>(
                        It.Is<DialogResultType>(result => result.DialogTitle == "Foutmelding")),
                Times.Once);
            _mockDialogService.Verify(service =>
                    service.ShowDialogAsync<NotifyDialogViewModel, DialogResultType>(
                        It.Is<DialogResultType>(result => result.DialogText == errorMessage)),
                Times.Once);
        }

        [Test]
        public async Task Save_AllFieldsEmpty_ShowsError()
        {
            // Arrange
            BuildWindowForTestScenario(
                firstName: "",
                lastName: "",
                email: "",
                phone: "",
                dateOfBirth: default,
                houseNumber: "",
                zipCode: "",
                city: "",
                country: "",
                street: "",
                selectedCourse: ""
            );

            // Act
            await InvokeProtectedMethodAsync(_viewModel, "OnSaveAsync");

            // Assert
            _mockStudentRepository.Verify(repo => repo.Add(It.IsAny<Student>()), Times.Never);
            _mockRegistrationRepository.Verify(repo => repo.Add(It.IsAny<Registration>()), Times.Never);
            _mockDialogService.Verify(service =>
                    service.ShowDialogAsync<NotifyDialogViewModel, DialogResultType>(
                        It.Is<DialogResultType>(result => result.DialogTitle == "Foutmelding")),
                Times.Once);
            _mockDialogService.Verify(service =>
                    service.ShowDialogAsync<NotifyDialogViewModel, DialogResultType>(
                        It.Is<DialogResultType>(result => result.DialogText.Contains("is verplicht"))),
                Times.AtLeastOnce);
        }

        private void BuildWindowForTestScenario(
            string firstName,
            string lastName,
            string email,
            string phone,
            DateTime dateOfBirth,
            string houseNumber,
            string zipCode,
            string city,
            string country,
            string street,
            string selectedCourse)
        {
            var firstNameTextBox = new TextBox
            {
                Name = "FirstName",
                Text = firstName,
                Tag = "Voornaam"
            };
            ValidationProperties.SetIsRequired(firstNameTextBox, true);

            var lastNameTextBox = new TextBox
            {
                Name = "LastName",
                Text = lastName,
                Tag = "Achternaam"
            };
            ValidationProperties.SetIsRequired(lastNameTextBox, true);

            var emailTextBox = new TextBox
            {
                Name = "Email",
                Text = email,
                Tag = "Email"
            };
            ValidationProperties.SetIsRequired(emailTextBox, true);
            ValidationProperties.SetIsEmail(emailTextBox, true);

            var phoneTextBox = new TextBox
            {
                Name = "Phone",
                Text = phone,
                Tag = "Telefoonnummer"
            };
            ValidationProperties.SetIsRequired(phoneTextBox, true);
            ValidationProperties.SetIsPhoneNumber(phoneTextBox, true);

            var datePicker = new DatePicker
            {
                Name = "Date",
                SelectedDate = dateOfBirth,
                Tag = "Geboortedatum"
            };
            ValidationProperties.SetIsRequired(datePicker, true);
            ValidationProperties.SetIsDate(datePicker, true);

            var houseNumberTextBox = new TextBox
            {
                Name = "HouseNumber",
                Text = houseNumber,
                Tag = "Huisnummer"
            };
            ValidationProperties.SetIsRequired(houseNumberTextBox, true);

            var zipCodeTextBox = new TextBox
            {
                Name = "ZipCode",
                Text = zipCode,
                Tag = "Postcode"
            };
            ValidationProperties.SetIsRequired(zipCodeTextBox, true);

            var cityTextBox = new TextBox
            {
                Name = "City",
                Text = city,
                Tag = "Stad"
            };
            ValidationProperties.SetIsRequired(cityTextBox, true);

            var countryTextBox = new TextBox
            {
                Name = "Country",
                Text = country,
                Tag = "Land"
            };
            ValidationProperties.SetIsRequired(countryTextBox, true);

            var streetTextBox = new TextBox
            {
                Name = "Street",
                Text = street,
                Tag = "Straat"
            };
            ValidationProperties.SetIsRequired(streetTextBox, true);

            var courseComboBox = new ComboBox
            {
                Name = "Course",
                ItemsSource = _viewModel.Courses,
                SelectedItem = selectedCourse,
                Tag = "Cursus"
            };
            ValidationProperties.SetIsRequired(courseComboBox, true);

            var window = new Window
            {
                Content = new StackPanel
                {
                    Children =
                    {
                        firstNameTextBox,
                        lastNameTextBox,
                        emailTextBox,
                        phoneTextBox,
                        datePicker,
                        houseNumberTextBox,
                        zipCodeTextBox,
                        cityTextBox,
                        countryTextBox,
                        streetTextBox,
                        courseComboBox
                    }
                }
            };
            _viewModel.ParentWindow = window;
        }

        private async Task InvokeProtectedMethodAsync(object instance, string methodName)
        {
            var method = instance.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (method != null)
            {
                var task = (Task?)method.Invoke(instance, null);
                if (task != null)
                {
                    await task;
                }
            }
        }
    }
}