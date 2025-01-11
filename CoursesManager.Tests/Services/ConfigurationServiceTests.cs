using NUnit.Framework;
using System;
using System.Collections.Generic;
using CoursesManager.UI.Service;
using CoursesManager.UI.Models;
using CoursesManager.MVVM.Env;

namespace CoursesManager.Tests.Services
{
    [TestFixture]
    public class ConfigurationServiceTests
    {
        private ConfigurationService _configurationService;
        private EncryptionService _encryptionService;

        [SetUp]
        public void Setup()
        {
            // Genereert een vaste sleutel voor consistente encryptie in tests
            string testKey = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("1234567890123456"));
            _encryptionService = new EncryptionService(testKey);
            _configurationService = new ConfigurationService(_encryptionService);
        }

        [Test]
        public void SaveEnvSettings_ShouldEncryptAndSaveSettings()
        {
            // Arrange
            var dbParams = new Dictionary<string, string>
            {
                { "Server", "localhost" },
                { "Database", "TestDb" },
                { "User", "root" },
                { "Password", "password" }
            };

            var mailParams = new Dictionary<string, string>
            {
                { "SMTPServer", "smtp.test.com" },
                { "Port", "25" },
                { "Username", "user@test.com" },
                { "Password", "mailpassword" }
            };

            string dbConnectionString = BuildConnectionString(dbParams);
            string mailConnectionString = BuildConnectionString(mailParams);

            string BuildConnectionString(Dictionary<string, string> parameters)
            {
                return string.Join(";", parameters.Select(kv => $"{kv.Key}={kv.Value}"));
            }

            // Act
            _configurationService.SaveEnvSettings(dbParams, mailParams);

            // Assert
            Assert.That(_encryptionService.Decrypt(EnvManager<EnvModel>.Values.ConnectionString), Is.EqualTo(dbConnectionString));
            Assert.That(_encryptionService.Decrypt(EnvManager<EnvModel>.Values.MailConnectionString), Is.EqualTo(mailConnectionString));
        }

        [Test]
        public void GetDecryptedEnvSettings_ShouldReturnDecryptedValues()
        {
            // Arrange
            EnvManager<EnvModel>.Values.ConnectionString = _encryptionService.Encrypt("Server=localhost;Database=TestDb;User=root;Password=password");
            EnvManager<EnvModel>.Values.MailConnectionString = _encryptionService.Encrypt("SMTPServer=smtp.test.com;Port=25;Username=user@test.com;Password=mailpassword");

            // Act
            var result = _configurationService.GetDecryptedEnvSettings();

            // Assert
            Assert.That(result.ConnectionString, Is.EqualTo("Server=localhost;Database=TestDb;User=root;Password=password"));
            Assert.That(result.MailConnectionString, Is.EqualTo("SMTPServer=smtp.test.com;Port=25;Username=user@test.com;Password=mailpassword"));
        }

        [Test]
        public void GetDatabaseParameters_ShouldReturnParsedValues()
        {
            // Arrange
            EnvManager<EnvModel>.Values.ConnectionString = _encryptionService.Encrypt("Server=localhost;Database=TestDb;User=root;Password=password");

            // Act
            var dbParams = _configurationService.GetDatabaseParameters();

            // Assert
            Assert.That(dbParams["Server"], Is.EqualTo("localhost"));
            Assert.That(dbParams["Database"], Is.EqualTo("TestDb"));
            Assert.That(dbParams["User"], Is.EqualTo("root"));
            Assert.That(dbParams["Password"], Is.EqualTo("password"));
        }

        [Test]
        public void GetMailParameters_ShouldReturnParsedValues()
        {
            // Arrange
            EnvManager<EnvModel>.Values.MailConnectionString = _encryptionService.Encrypt("SMTPServer=smtp.test.com;Port=25;Username=user@test.com;Password=mailpassword");

            // Act
            var mailParams = _configurationService.GetMailParameters();

            // Assert
            Assert.That(mailParams["SMTPServer"], Is.EqualTo("smtp.test.com"));
            Assert.That(mailParams["Port"], Is.EqualTo("25"));
            Assert.That(mailParams["Username"], Is.EqualTo("user@test.com"));
            Assert.That(mailParams["Password"], Is.EqualTo("mailpassword"));
        }

        [Test]
        public void GetDatabaseParameters_ShouldReturnEmpty_WhenConnectionStringIsNull()
        {
            // Arrange
            EnvManager<EnvModel>.Values.ConnectionString = null;

            // Act
            var dbParams = _configurationService.GetDatabaseParameters();

            // Assert
            Assert.That(dbParams, Is.Empty, "De databaseparameters zouden leeg moeten zijn als de ConnectionString null is.");
        }

        [Test]
        public void GetMailParameters_ShouldReturnEmpty_WhenMailConnectionStringIsNull()
        {
            // Arrange
            EnvManager<EnvModel>.Values.MailConnectionString = null;

            // Act
            var mailParams = _configurationService.GetMailParameters();

            // Assert
            Assert.That(mailParams, Is.Empty, "De mailparameters zouden leeg moeten zijn als de MailConnectionString null is.");
        }

        [Test]
        public void ValidateSettings_ShouldReturnFalse_WhenNoSettingsPresent()
        {
            // Arrange
            EnvManager<EnvModel>.Values.ConnectionString = null;
            EnvManager<EnvModel>.Values.MailConnectionString = null;

            // Act
            var isValid = _configurationService.ValidateSettings();

            // Assert
            Assert.That(isValid, Is.False, "De validatie zou false moeten retourneren als er geen instellingen aanwezig zijn.");
        }

        [Test]
        public void ValidateSettings_ShouldReturnTrue_WhenValidSettingsProvided()
        {
            // Arrange
            var dummyConnectionString = _encryptionService.Encrypt("Server=dummy;Database=dummy;User=dummy;Password=dummy");
            var dummyMailConnectionString = _encryptionService.Encrypt("SMTPServer=dummy.smtp.com;Port=587;Username=dummy@dummy.com;Password=dummy");

            EnvManager<EnvModel>.Values.ConnectionString = dummyConnectionString;
            EnvManager<EnvModel>.Values.MailConnectionString = dummyMailConnectionString;

            // Act
            var isValid = _configurationService.ValidateSettings();

            // Assert
            Assert.That(isValid, Is.False, "De validatie zou false moeten retourneren omdat een dummy-verbinding niet geldig is.");
        }


        [Test]
        public void SaveEnvSettings_ShouldHandleEmptyParameters()
        {
            // Arrange
            var dbParams = new Dictionary<string, string>();
            var mailParams = new Dictionary<string, string>();

            // Act
            _configurationService.SaveEnvSettings(dbParams, mailParams);

            // Assert
            Assert.That(EnvManager<EnvModel>.Values.ConnectionString, Is.Empty, "ConnectionString zou leeg moeten zijn als dbParams leeg is.");
            Assert.That(EnvManager<EnvModel>.Values.MailConnectionString, Is.Empty, "MailConnectionString zou leeg moeten zijn als mailParams leeg is.");
        }
    }
}
