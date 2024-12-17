using Moq;
using CoursesManager.UI.Models;
using CoursesManager.MVVM.Dialogs;
using CoursesManager.UI.Dialogs.ResultTypes;
using CoursesManager.UI.Dialogs.ViewModels;
using System.Windows;
using System.Windows.Controls;
using CoursesManager.UI.Repositories.RegistrationRepository;
using CoursesManager.UI.Repositories.StudentRepository;
using CoursesManager.UI.Repositories.CourseRepository;
using CoursesManager.UI.ViewModels.Students;
using CoursesManager.UI.Helpers;
using CoursesManager.UI.Services;
using System.Windows.Media;

namespace CoursesManager.Tests
{
    [TestFixture]
    [Apartment(System.Threading.ApartmentState.STA)]
    public class EditStudentViewModelTests
    {
        private Mock<IStudentRepository> _studentRepositoryMock;
        private Mock<ICourseRepository> _courseRepositoryMock;
        private Mock<IRegistrationRepository> _registrationRepositoryMock;
        private Mock<IDialogService> _dialogServiceMock;
        private EditStudentViewModel _viewModel;
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

            _studentRepositoryMock = new Mock<IStudentRepository>();
            _courseRepositoryMock = new Mock<ICourseRepository>();
            _registrationRepositoryMock = new Mock<IRegistrationRepository>();
            _dialogServiceMock = new Mock<IDialogService>();

            _courseRepositoryMock.Setup(repo => repo.GetAll())
                .Returns(new List<Course>
                {
                    new Course { Id = 1, Name = "Math", IsActive = true},
                    new Course { Id = 2, Name = "Science", IsActive = true }
                });

            _registrationRepositoryMock.Setup(repo => repo.GetAll())
                .Returns(new List<Registration>
                {
                    new Registration { StudentId = 1, CourseId = 1 }
                });

            _viewModel = new EditStudentViewModel(
                _studentRepositoryMock.Object,
                _courseRepositoryMock.Object,
                _registrationRepositoryMock.Object,
                _dialogServiceMock.Object,
                _student);
        }

        [Test]
        public async Task OnSaveAsync_ShouldUpdateStudentAndRegistrations_WhenValidationSucceeds()
        {
            // Arrange
            BuildWindowForTestScenario(
                firstName: "John",
                lastName: "Doe",
                email: "john.doe@example.com",
                phone: "1234567890",
                dateOfBirth: new DateTime(1990, 1, 1),
                houseNumber: "123",
                zipCode: "1234AB",
                city: "City",
                country: "Country",
                street: "Main St",
                selectedCourse: "Science"
            );

            _viewModel.SelectableCourses.First(c => c.Id == 2).IsSelected = true;

            var confirmationResult = DialogResult<DialogResultType>.Builder()
                .SetSuccess(new DialogResultType { Result = true }, "Confirmed")
                .Build();

            _dialogServiceMock
                .Setup(ds => ds.ShowDialogAsync<ConfirmationDialogViewModel, DialogResultType>(
                    It.Is<DialogResultType>(result => result.DialogText == "Wilt u de wijzigingen opslaan?")))
                .ReturnsAsync(confirmationResult);

            _dialogServiceMock
                .Setup(ds => ds.ShowDialogAsync<NotifyDialogViewModel, DialogResultType>(It.IsAny<DialogResultType>()))
                .ReturnsAsync(DialogResult<DialogResultType>.Builder().SetSuccess(new DialogResultType(), "Notification").Build());

            // Act
            await _viewModel.SaveAsync();

            // Assert
            _dialogServiceMock.Verify(service =>
                    service.ShowDialogAsync<ConfirmationDialogViewModel, DialogResultType>(
                        It.Is<DialogResultType>(result => result.DialogText == "Wilt u de wijzigingen opslaan?")),
                Times.Once);

            _studentRepositoryMock.Verify(repo => repo.Update(It.IsAny<Student>()), Times.Once);
            _registrationRepositoryMock.Verify(repo => repo.Add(It.Is<Registration>(r => r.CourseId == 2)), Times.Once);

            _dialogServiceMock.Verify(service =>
                    service.ShowDialogAsync<NotifyDialogViewModel, DialogResultType>(
                        It.Is<DialogResultType>(result => result.DialogText == "Cursist succesvol opgeslagen.")),
                Times.Once);
        }

        [Test]
        public async Task OnSaveAsync_ShouldNotSave_WhenValidationFails()
        {
            // Arrange
            BuildWindowForTestScenario(
                firstName: "",
                lastName: "",
                email: "invalid-email",
                phone: "",
                dateOfBirth: new DateTime(1990, 1, 1),
                houseNumber: "",
                zipCode: "",
                city: "",
                country: "",
                street: "",
                selectedCourse: "Math"
            );

            var validationErrors = ValidationService.ValidateRequiredFields(_viewModel.ParentWindow);
            var errorMessage = string.Join("\n", validationErrors);

            _dialogServiceMock
                .Setup(ds => ds.ShowDialogAsync<NotifyDialogViewModel, DialogResultType>(
                    It.Is<DialogResultType>(result => result.DialogText == errorMessage)))
                .ReturnsAsync(DialogResult<DialogResultType>.Builder().SetFailure(errorMessage).Build());

            // Act
            await _viewModel.SaveAsync();

            // Assert
            _studentRepositoryMock.Verify(repo => repo.Update(It.IsAny<Student>()), Times.Never);

            _dialogServiceMock.Verify(service =>
                    service.ShowDialogAsync<NotifyDialogViewModel, DialogResultType>(
                        It.Is<DialogResultType>(result => result.DialogTitle == "Foutmelding")),
                Times.Once);

            _dialogServiceMock.Verify(service =>
                    service.ShowDialogAsync<NotifyDialogViewModel, DialogResultType>(
                        It.Is<DialogResultType>(result => result.DialogText == errorMessage)),
                Times.Once);

            _dialogServiceMock.VerifyNoOtherCalls();
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
                ItemsSource = _viewModel.SelectableCourses.Select(c => c.Name),
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
    }
}