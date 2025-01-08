using CoursesManager.MVVM.Messages;
using CoursesManager.MVVM.Navigation;
using CoursesManager.UI.Service;
using CoursesManager.UI.ViewModels;
using Moq;

namespace CoursesManager.Tests
{
    [TestFixture]
    public class ConfigurationViewModelTests
    {
        private Mock<IConfigurationService> _configurationServiceMock;
        private ConfigurationViewModel _viewModel;
        private Mock<IMessageBroker> _messageBrokerMock;
        private Mock<INavigationService> _navigationServiceMock;

        [SetUp]
        public void SetUp()
        {
            _configurationServiceMock = new Mock<IConfigurationService>();
            _messageBrokerMock = new Mock<IMessageBroker>();
            _navigationServiceMock = new Mock<INavigationService>();
            

            // mock voor de DB gegevens
            _configurationServiceMock
                .Setup(cs => cs.GetDatabaseParameters())
                .Returns(new Dictionary<string, string>
                {
                    { "Server", "localhost" },
                    { "Port", "3306" },
                    { "User", "root" },
                    { "Password", "password" },
                    { "Database", "test_db" }
                });

            // Mock voor mail gegevens
            _configurationServiceMock
                .Setup(cs => cs.GetMailParameters())
                .Returns(new Dictionary<string, string>
                {
                    { "Server", "smtp.test.com" },
                    { "Port", "587" },
                    { "User", "admin" },
                    { "Password", "password" }
                });

            _viewModel = new ConfigurationViewModel(_configurationServiceMock.Object, _messageBrokerMock.Object, _navigationServiceMock.Object);
        }

        [Test]
        public void InitializeSettings_ShouldLoadParametersFromConfigurationService()
        {
            //act 
            _viewModel.InitializeSettings();

            //assert
            Assert.That(_viewModel.DbServer, Is.EqualTo("localhost"));
            Assert.That(_viewModel.DbPort, Is.EqualTo("3306"));
            Assert.That(_viewModel.DbUser, Is.EqualTo("root"));
            Assert.That(_viewModel.DbPassword, Is.EqualTo("password"));
            Assert.That(_viewModel.DbName, Is.EqualTo("test_db"));

            Assert.That(_viewModel.MailServer, Is.EqualTo("smtp.test.com"));
            Assert.That(_viewModel.MailPort, Is.EqualTo("587"));
            Assert.That(_viewModel.MailUser, Is.EqualTo("admin"));
            Assert.That(_viewModel.MailPassword, Is.EqualTo("password"));
        }

        [Test]
        public void InitializeSettings_ShouldHandleEmptyDatabaseAndMailParameters()
        {
            // Arrange
            _configurationServiceMock
                .Setup(cs => cs.GetDatabaseParameters())
                .Returns(new Dictionary<string, string>());

            _configurationServiceMock
                .Setup(cs => cs.GetMailParameters())
                .Returns(new Dictionary<string, string>());

            // Act
            _viewModel.InitializeSettings();

            // Assert
            Assert.That(_viewModel.DbServer, Is.EqualTo(string.Empty));
            Assert.That(_viewModel.DbPort, Is.EqualTo(string.Empty));
            Assert.That(_viewModel.DbUser, Is.EqualTo(string.Empty));
            Assert.That(_viewModel.DbPassword, Is.EqualTo(string.Empty));
            Assert.That(_viewModel.DbName, Is.EqualTo(string.Empty));

            Assert.That(_viewModel.MailServer, Is.EqualTo(string.Empty));
            Assert.That(_viewModel.MailPort, Is.EqualTo(string.Empty));
            Assert.That(_viewModel.MailUser, Is.EqualTo(string.Empty));
            Assert.That(_viewModel.MailPassword, Is.EqualTo(string.Empty));
        }

        [Test]
        public void ValidateAndSave_ShouldCallSaveEnvSettingsWithCorrectParameters()
        {
            // Arrange
            _viewModel.DbServer = "localhost";
            _viewModel.DbPort = "3306";
            _viewModel.DbUser = "root";
            _viewModel.DbPassword = "password";
            _viewModel.DbName = "test_db";

            _viewModel.MailServer = "smtp.test.com";
            _viewModel.MailPort = "587";
            _viewModel.MailUser = "admin";
            _viewModel.MailPassword = "password";

            // Act
            _viewModel.ValidateAndSave();

            // Assert
            _configurationServiceMock.Verify(cs => cs.SaveEnvSettings(
                It.Is<Dictionary<string, string>>(dbParams =>
                    dbParams["Server"] == "localhost" &&
                    dbParams["Port"] == "3306" &&
                    dbParams["User"] == "root" &&
                    dbParams["Password"] == "password" &&
                    dbParams["Database"] == "test_db"
                ),
                It.Is<Dictionary<string, string>>(mailParams =>
                    mailParams["Server"] == "smtp.test.com" &&
                    mailParams["Port"] == "587" &&
                    mailParams["User"] == "admin" &&
                    mailParams["Password"] == "password"
                )
            ), Times.Once);
        }

        [Test]
        public void ValidateAndSave_ShouldShowMessageBox_WhenSettingsAreInvalid()
        {
            // Arrange
            _viewModel.DbServer = string.Empty; 

            // Act
            _viewModel.ValidateAndSave();

            // Assert
            _configurationServiceMock.Verify(cs => cs.SaveEnvSettings(It.IsAny<Dictionary<string, string>>(), It.IsAny<Dictionary<string, string>>()), Times.Never,
                "SaveEnvSettings mag niet worden aangeroepen als de instellingen ongeldig zijn.");
        }

        [Test]
        public void ValidateAndSave_ShouldHandleException_WhenSaveEnvSettingsThrows()
        {
            // Arrange
            _viewModel.DbServer = "localhost";
            _viewModel.DbPort = "3306";
            _viewModel.DbUser = "root";
            _viewModel.DbPassword = "password";
            _viewModel.DbName = "test_db";
            _viewModel.MailServer = "smtp.test.com";
            _viewModel.MailPort = "587";
            _viewModel.MailUser = "admin";
            _viewModel.MailPassword = "password";

            _configurationServiceMock
                .Setup(cs => cs.SaveEnvSettings(It.IsAny<Dictionary<string, string>>(), It.IsAny<Dictionary<string, string>>()))
                .Throws(new Exception("Test Exception"));

            // Act & Assert
            Assert.DoesNotThrow(() => _viewModel.ValidateAndSave(), "ValidateAndSave mag niet crashen als SaveEnvSettings een uitzondering gooit.");
        }

        [Test]
        public void ValidateAndSave_ShouldShowErrorMessage_WhenValidateSettingsFails()
        {
            // Arrange
            _viewModel.DbServer = "localhost";
            _viewModel.DbPort = "3306";
            _viewModel.DbUser = "root";
            _viewModel.DbPassword = "password";
            _viewModel.DbName = "test_db";
            _viewModel.MailServer = "smtp.test.com";
            _viewModel.MailPort = "587";
            _viewModel.MailUser = "admin";
            _viewModel.MailPassword = "password";

            _configurationServiceMock
                .Setup(cs => cs.ValidateSettings())
                .Returns(false);

            // Act
            _viewModel.ValidateAndSave();

            // Assert
            _configurationServiceMock.Verify(cs => cs.SaveEnvSettings(It.IsAny<Dictionary<string, string>>(), It.IsAny<Dictionary<string, string>>()), Times.Once);
            Assert.That(INavigationService.CanNavigate, Is.False, "CanNavigate moet false zijn als ValidateSettings faalt.");
        }

        [Test]
        public void ValidateAndSave_ShouldSetCanNavigateToTrue_WhenSettingsAreValid()
        {
            // Arrange
            _viewModel.DbServer = "localhost";
            _viewModel.DbPort = "3306";
            _viewModel.DbUser = "root";
            _viewModel.DbPassword = "password";
            _viewModel.DbName = "test_db";
            _viewModel.MailServer = "smtp.test.com";
            _viewModel.MailPort = "587";
            _viewModel.MailUser = "admin";
            _viewModel.MailPassword = "password";

            _configurationServiceMock
                .Setup(cs => cs.ValidateSettings())
                .Returns(true);

            // Act
            _viewModel.ValidateAndSave();

            // Assert
            Assert.That(INavigationService.CanNavigate, Is.True, "CanNavigate moet true zijn als de instellingen geldig zijn.");
        }

        [Test]
        public void SaveCommand_CanExecute_ShouldReturnTrue_WhenAllFieldsAreValid()
        {
            // Arrange
            _viewModel.DbServer = "localhost";
            _viewModel.DbPort = "3306";
            _viewModel.DbUser = "root";
            _viewModel.DbPassword = "password";
            _viewModel.DbName = "test_db";
            _viewModel.MailServer = "smtp.test.com";
            _viewModel.MailPort = "587";
            _viewModel.MailUser = "admin";
            _viewModel.MailPassword = "password";

            // Act
            var canExecute = _viewModel.SaveCommand.CanExecute(null);

            // Assert
            Assert.That(canExecute, Is.True, "SaveCommand.CanExecute moet true retourneren als alle velden geldig zijn.");
        }



        [Test]
        public void SaveCommand_ShouldShowMessageBox_WhenFieldsAreInvalid()
        {
            // Arrange
            _viewModel.DbServer = string.Empty; 

            // Act
            _viewModel.SaveCommand.Execute(null);

            // Assert
            _configurationServiceMock.Verify(cs => cs.SaveEnvSettings(It.IsAny<Dictionary<string, string>>(), It.IsAny<Dictionary<string, string>>()), Times.Never,
                "SaveEnvSettings mag niet worden aangeroepen als de instellingen ongeldig zijn.");
        }

    }
}
