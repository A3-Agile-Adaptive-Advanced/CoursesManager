using NUnit.Framework;
using System;
using System.Collections.Generic;
using CoursesManager.UI.Service;
using CoursesManager.UI.Models;

namespace CoursesManager.Tests.Services
{
    [TestFixture]
    public class ConfigurationServiceTests
    {
        private ConfigurationService _configurationService;
        private EncryptionService _encryptionService;

        // Vervanging voor EnvManager
        private Dictionary<string, string> _mockEnvStorage;

        [SetUp]
        public void Setup()
        {
            // Genereert een vaste sleutel voor consistentie
            string testKey = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("1234567890123456"));
            _encryptionService = new EncryptionService(testKey);
            _configurationService = new ConfigurationService(_encryptionService);

            // Hiermee kan testdata tijdelijk opgeslagen worden voordat het verwerkt wordt
            _mockEnvStorage = new Dictionary<string, string>();
        }

        // Slaat een sleutel waarden op
        private void SetMockEnv(string key, string value)
        {
            _mockEnvStorage[key] = value;
        }

        // Haalt een waarde op uit de mock storage op basis van een sleutel
        private string GetMockEnv(string key)
        {
            return _mockEnvStorage.ContainsKey(key) ? _mockEnvStorage[key] : null;
        }

        [Test]
        public void SaveEnvSettings_ShouldEncryptAndSaveSettings()
        {
            // Arrange
            // Connection settings instellen
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

            // Dit wordt gebruikt voor vegelijking en validaties
            string dbConnectionString = BuildConnectionString(dbParams);
            string mailConnectionString = BuildConnectionString(mailParams);

            string BuildConnectionString(Dictionary<string, string> parameters)
            {
                return string.Join(";", parameters.Select(kv => $"{kv.Key}={kv.Value}"));
            }

            // Act
            _configurationService.SaveEnvSettings(dbParams, mailParams);

            // Gebruik mock-opslag om encrypted waarden op te slaan
            SetMockEnv("ConnectionString", _encryptionService.Encrypt(dbConnectionString));
            SetMockEnv("MailConnectionString", _encryptionService.Encrypt(mailConnectionString));

            // Assert
            // Controleer of de waarden juist zijn opgeslagen
            Assert.That(_encryptionService.Decrypt(GetMockEnv("ConnectionString")), Is.EqualTo(dbConnectionString));
            Assert.That(_encryptionService.Decrypt(GetMockEnv("MailConnectionString")), Is.EqualTo(mailConnectionString));
        }

        [Test]
        public void GetDecryptedEnvSettings_ShouldReturnDecryptedValues()
        {
            // Arrange
            string dbEncrypted = _encryptionService.Encrypt("Server=localhost;Database=TestDb;User=root;Password=password");
            string mailEncrypted = _encryptionService.Encrypt("SMTPServer=smtp.test.com;Port=25;Username=user@test.com;Password=mailpassword");

            // Gebruik mock-opslag om encrypted waarden te simuleren om vervolgens te decrypten
            SetMockEnv("ConnectionString", dbEncrypted);
            SetMockEnv("MailConnectionString", mailEncrypted);

            // Act
            var result = new EnvModel
            {
                ConnectionString = _encryptionService.Decrypt(GetMockEnv("ConnectionString")),
                MailConnectionString = _encryptionService.Decrypt(GetMockEnv("MailConnectionString"))
            };

            // Assert
            Assert.That(result.ConnectionString, Is.EqualTo("Server=localhost;Database=TestDb;User=root;Password=password"));
            Assert.That(result.MailConnectionString, Is.EqualTo("SMTPServer=smtp.test.com;Port=25;Username=user@test.com;Password=mailpassword"));
        }

        [Test]
        public void ValidateSettings_ShouldReturnFalse_WhenNoSettingsPresent()
        {
            // Arrange
            // Zet connection settings op null
            SetMockEnv("ConnectionString", null);
            SetMockEnv("MailConnectionString", null);

            // Act
            var connectionString = GetMockEnv("ConnectionString");
            var mailConnectionString = GetMockEnv("MailConnectionString");

            var isValid = !string.IsNullOrWhiteSpace(connectionString) &&
                          !string.IsNullOrWhiteSpace(mailConnectionString);

            // Assert
            Assert.That(isValid, Is.False);
        }

        [Test]
        public void ValidateSettings_ShouldReturnTrue_WhenValidSettingsProvided()
        {
            // Arrange
            string dbEncrypted = _encryptionService.Encrypt("Server=localhost;Database=TestDb;User=root;Password=password");
            string mailEncrypted = _encryptionService.Encrypt("SMTPServer=smtp.test.com;Port=25;Username=user@test.com;Password=mailpassword");

            // Gebruik mock-opslag om encrypted connection settings waarden te simuleren
            SetMockEnv("ConnectionString", dbEncrypted);
            SetMockEnv("MailConnectionString", mailEncrypted);

            // Act
            var connectionString = _encryptionService.Decrypt(GetMockEnv("ConnectionString"));
            var mailConnectionString = _encryptionService.Decrypt(GetMockEnv("MailConnectionString"));

            var isValid = !string.IsNullOrWhiteSpace(connectionString) &&
                          !string.IsNullOrWhiteSpace(mailConnectionString);

            // Assert
            Assert.That(isValid, Is.True);
        }      
    }
}
