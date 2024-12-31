using NUnit.Framework;
using CoursesManager.UI.Service;
using System;

namespace CoursesManager.Tests.Services
{
    [TestFixture]
    public class EncryptionServiceTests
    {
        private EncryptionService _encryptionService;

        [SetUp]
        public void Setup()
        {
            // Genereert een vaste sleutel voor consistentie
            string testKey = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("1234567890123456"));
            _encryptionService = new EncryptionService(testKey);
        }

        [Test]
        public void EncryptDecrypt_ShouldReturnOriginalText()
        {
            // Arrange
            string originalText = "Hele mooie encryptie data";

            // Act
            string encryptedText = _encryptionService.Encrypt(originalText);
            string decryptedText = _encryptionService.Decrypt(encryptedText);

            // Assert
            Assert.That(decryptedText, Is.EqualTo(originalText));
        }

        [Test]
        public void Encrypt_ShouldThrowException_WhenPlainTextIsNull()
        {
            // Arrange
            string plainText = null;

            // Act & Assert
            // Als _encryptionService.Encrypt een exceptie krijgt vangt Assert.Throws<ArgumentException>(() deze op.
            var ex = Assert.Throws<ArgumentException>(() => _encryptionService.Encrypt(plainText));
            Assert.That(ex.ParamName, Is.EqualTo("plainText"));
        }

        [Test]
        public void Decrypt_ShouldThrowException_WhenEncryptedTextIsNull()
        {
            // Arrange
            string encryptedText = null;

            // Act & Assert
            // Als _encryptionService.Decrypt een exceptie krijgt vangt Assert.Throws<ArgumentException>(() deze op.
            var ex = Assert.Throws<ArgumentException>(() => _encryptionService.Decrypt(encryptedText));
            Assert.That(ex.ParamName, Is.EqualTo("encryptedText"));
        }

        [Test]
        public void Decrypt_ShouldReturnOriginalText_WhenEncryptedTextIsInvalid()
        {
            // Arrange
            string invalidEncryptedText = "InvalidEncryptedString";

            // Act
            string result = _encryptionService.Decrypt(invalidEncryptedText);

            // Assert
            Assert.That(result, Is.EqualTo(invalidEncryptedText));
        }

        [Test]
        public void EncryptDecrypt_ShouldHandleLongText()
        {
            // Arrange
            string originalText = new string('A', 1000); // String van 1000 keer A

            // Act
            string encryptedText = _encryptionService.Encrypt(originalText);
            string decryptedText = _encryptionService.Decrypt(encryptedText);

            // Assert
            Assert.That(decryptedText, Is.EqualTo(originalText));
        }
    }
}
