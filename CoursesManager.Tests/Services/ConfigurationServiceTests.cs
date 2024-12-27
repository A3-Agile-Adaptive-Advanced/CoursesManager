using CoursesManager.MVVM.Env;
using CoursesManager.UI.DataAccess;
using CoursesManager.UI.Models;
using CoursesManager.UI.Service;
using Moq;
using MySql.Data.MySqlClient;

namespace CoursesManager.Tests.Services
{
    [TestFixture]
    public class ConfigurationServiceTests
    {
        private Mock<IEncryptionService> _encryptionServiceMock;
        private ConfigurationService _configurationService;

        [SetUp]
        public void SetUp()
        {
            _encryptionServiceMock = new Mock<IEncryptionService>();
            _configurationService = new ConfigurationService(_encryptionServiceMock.Object);
        }

        [Test]
        public void SaveEnvSettings_ShouldEncryptAndSaveParameters()
        {
            // Arrange
            var dbParams = new Dictionary<string, string> { { "Server", "localhost" }, { "Database", "test" } };
            var mailParams = new Dictionary<string, string> { { "SmtpServer", "smtp.test.com" } };

            _encryptionServiceMock
                .Setup(enc => enc.Encrypt(It.IsAny<string>()))
                .Returns((string input) => "Encrypted_" + input);

            // Act
            _configurationService.SaveEnvSettings(dbParams, mailParams);

            // Assert
            _encryptionServiceMock.Verify(enc => enc.Encrypt(It.IsAny<string>()), Times.Exactly(2),
                "Encrypt moet twee keer worden aangeroepen (voor dbParams en mailParams).");
        }

        [Test]
        public void SafeDecrypt_ShouldHandleDecryptionErrorGracefully()
        {
            // Arrange
            _encryptionServiceMock
                .Setup(enc => enc.Decrypt(It.IsAny<string>()))
                .Throws(new Exception("Decryption error"));

            // Act
            var result = _configurationService.SafeDecrypt("InvalidEncryptedText");

            // Assert
            Assert.That(result, Is.EqualTo("InvalidEncryptedText"), "Bij fout tijdens decryptie moet de originele tekst worden geretourneerd.");
        }


        [Test]
        public void GetDecryptedEnvSettings_ShouldReturnDecryptedValues()
        {
            // Arrange
            EnvManager<EnvModel>.Values.ConnectionString = "Encrypted_DB";
            EnvManager<EnvModel>.Values.MailConnectionString = "Encrypted_Mail";

            _encryptionServiceMock
                .Setup(enc => enc.Decrypt("Encrypted_DB"))
                .Returns("Decrypted_DB");

            _encryptionServiceMock
                .Setup(enc => enc.Decrypt("Encrypted_Mail"))
                .Returns("Decrypted_Mail");

            // Act
            var result = _configurationService.GetDecryptedEnvSettings();

            // Assert
            Assert.That(result.ConnectionString, Is.EqualTo("Decrypted_DB"), "ConnectionString moet gedecrypteerd zijn.");
            Assert.That(result.MailConnectionString, Is.EqualTo("Decrypted_Mail"), "MailConnectionString moet gedecrypteerd zijn.");
        }

        [Test]
        public void GetDatabaseParameters_ShouldReturnParsedValues()
        {
            // Arrange
            var decryptedConnectionString = "Server=localhost;Database=test;User=root;Password=password";
            _encryptionServiceMock
                .Setup(enc => enc.Decrypt(It.IsAny<string>()))
                .Returns(decryptedConnectionString);

            EnvManager<EnvModel>.Values.ConnectionString = "Encrypted_DB";

            // Act
            var result = _configurationService.GetDatabaseParameters();

            // Assert
            Assert.That(result["Server"], Is.EqualTo("localhost"), "Server parameter moet 'localhost' zijn.");
            Assert.That(result["Database"], Is.EqualTo("test"), "Database parameter moet 'test' zijn.");
            Assert.That(result["User"], Is.EqualTo("root"), "User parameter moet 'root' zijn.");
            Assert.That(result["Password"], Is.EqualTo("password"), "Password parameter moet 'password' zijn.");
        }


        [Test]
        public void GetMailParameters_ShouldReturnParsedValues()
        {
            // Arrange
            var decryptedMailConnectionString = "SmtpServer=smtp.test.com;Port=587;User=admin;Password=mailpassword";
            _encryptionServiceMock
                .Setup(enc => enc.Decrypt(It.IsAny<string>()))
                .Returns(decryptedMailConnectionString);

            EnvManager<EnvModel>.Values.MailConnectionString = "Encrypted_Mail";

            // Act
            var result = _configurationService.GetMailParameters();

            // Assert
            Assert.That(result["SmtpServer"], Is.EqualTo("smtp.test.com"), "SmtpServer parameter moet 'smtp.test.com' zijn.");
            Assert.That(result["Port"], Is.EqualTo("587"), "Port parameter moet '587' zijn.");
            Assert.That(result["User"], Is.EqualTo("admin"), "User parameter moet 'admin' zijn.");
            Assert.That(result["Password"], Is.EqualTo("mailpassword"), "Password parameter moet 'mailpassword' zijn.");
        }

        // unhappy flows

        [Test]
        public void SaveEnvSettings_ShouldHandleEncryptionFailure()
        {
            // Arrange
            var dbParams = new Dictionary<string, string> { { "Server", "localhost" }, { "Database", "test" } };
            var mailParams = new Dictionary<string, string> { { "SmtpServer", "smtp.test.com" } };

            _encryptionServiceMock
                .Setup(enc => enc.Encrypt(It.IsAny<string>()))
                .Throws(new Exception("Encryption error"));

            // Act & Assert
            Assert.DoesNotThrow(() => _configurationService.SaveEnvSettings(dbParams, mailParams),
                "SaveEnvSettings mag niet crashen als Encrypt een uitzondering gooit.");
        }

        [Test]
        public void SafeDecrypt_ShouldReturnOriginalText_WhenDecryptionFails()
        {
            // Arrange
            var encryptedText = "InvalidEncryptedText";
            _encryptionServiceMock
                .Setup(enc => enc.Decrypt(encryptedText))
                .Throws(new Exception("Decryption error"));

            // Act
            var result = _configurationService.SafeDecrypt(encryptedText);

            // Assert
            Assert.That(result, Is.EqualTo(encryptedText), "Bij fout tijdens decryptie moet de originele tekst worden geretourneerd.");
        }

        [Test]
        public void ValidateSettings_ShouldReturnFalse_WhenEnvFileIsMissing()
        {
            // Arrange
            var envFilePath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
            if (File.Exists(envFilePath))
                File.Delete(envFilePath); // Zorg dat het bestand niet bestaat

            // Act
            var result = _configurationService.ValidateSettings();

            // Assert
            Assert.That(result, Is.False, ".env-bestand ontbreekt, dus ValidateSettings moet false retourneren.");
        }

        [Test]
        public void ValidateSettings_ShouldReturnFalse_WhenConnectionStringsAreEmpty()
        {
            // Arrange
            EnvManager<EnvModel>.Values.ConnectionString = "";
            EnvManager<EnvModel>.Values.MailConnectionString = "";

            // Act
            var result = _configurationService.ValidateSettings();

            // Assert
            Assert.That(result, Is.False, "ValidateSettings moet false retourneren als ConnectionString of MailConnectionString leeg is.");
        }


        [Test]
        public void ValidateSettings_ShouldReturnFalse_WhenConnectionStringIsInvalid()
        {
            // Arrange
            EnvManager<EnvModel>.Values.ConnectionString = "InvalidConnectionString";
            EnvManager<EnvModel>.Values.MailConnectionString = "ValidEncryptedMail";

            _encryptionServiceMock
                .Setup(enc => enc.Decrypt("ValidEncryptedMail"))
                .Returns("SmtpServer=smtp.test.com;Port=587;User=admin;Password=mailpassword");

            // Act
            var result = _configurationService.ValidateSettings();

            // Assert
            Assert.That(result, Is.False, "ValidateSettings moet false retourneren als ConnectionString ongeldig is.");
        }

        [Test]
        public void GetDatabaseParameters_ShouldReturnEmptyDictionary_WhenConnectionStringIsEmpty()
        {
            // Arrange
            EnvManager<EnvModel>.Values.ConnectionString = "";

            // Act
            var result = _configurationService.GetDatabaseParameters();

            // Assert
            Assert.That(result, Is.Empty, "GetDatabaseParameters moet een lege dictionary retourneren als ConnectionString leeg is.");
        }


        [Test]
        public void GetMailParameters_ShouldReturnEmptyDictionary_WhenMailConnectionStringIsEmpty()
        {
            // Arrange
            EnvManager<EnvModel>.Values.MailConnectionString = "";

            // Act
            var result = _configurationService.GetMailParameters();

            // Assert
            Assert.That(result, Is.Empty, "GetMailParameters moet een lege dictionary retourneren als MailConnectionString leeg is.");
        }


    }
}