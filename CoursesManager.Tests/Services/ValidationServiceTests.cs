using CoursesManager.UI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CoursesManager.UI.Helpers;
using NUnit.Framework;

namespace CoursesManager.Tests.Services
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class ValidationServiceTests
    {
        [Test]
        public void ValidateRequiredFields_ShouldReturnErrors_WhenRequiredFieldsAreEmpty()
        {
            // Arrange
            var parent = new StackPanel();
            var textBox = new TextBox { Tag = "Email" };
            ValidationProperties.SetIsRequired(textBox, true);
            parent.Children.Add(textBox);

            // Act
            var errors = ValidationService.ValidateRequiredFields(parent, new List<string>());

            // Assert
            Assert.That(errors.FirstOrDefault(), Is.EqualTo("Email is verplicht."));
        }

        [Test]
        public void ValidateRequiredFields_ShouldReturnErrors_WhenEmailIsInvalid()
        {
            // Arrange
            var parent = new StackPanel();
            var textBox = new TextBox { Tag = "Email", Text = "invalidemail" };
            ValidationProperties.SetIsRequired(textBox, true);
            ValidationProperties.SetIsEmail(textBox, true);
            parent.Children.Add(textBox);

            // Act
            var errors = ValidationService.ValidateRequiredFields(parent, new List<string>());

            // Assert
            Assert.That(errors.FirstOrDefault(), Is.EqualTo("Email is geen geldig e-mailadres."));
        }

        [Test]
        public void ValidateRequiredFields_ShouldReturnErrors_WhenPhoneNumberIsInvalid()
        {
            // Arrange
            var parent = new StackPanel();
            var textBox = new TextBox { Tag = "Phone", Text = "invalidphone" };
            ValidationProperties.SetIsRequired(textBox, true);
            ValidationProperties.SetIsPhoneNumber(textBox, true);
            parent.Children.Add(textBox);

            // Act
            var errors = ValidationService.ValidateRequiredFields(parent, new List<string>());

            // Assert
            Assert.That(errors.FirstOrDefault(), Is.EqualTo("Phone is geen geldig telefoonnummer."));
        }

        [Test]
        public void ValidateRequiredFields_ShouldReturnErrors_WhenComboBoxIsNotSelected()
        {
            // Arrange
            var parent = new StackPanel();
            var comboBox = new ComboBox { Tag = "Course" };
            ValidationProperties.SetIsRequired(comboBox, true);
            parent.Children.Add(comboBox);

            // Act
            var errors = ValidationService.ValidateRequiredFields(parent, new List<string>());

            // Assert
            Assert.That(errors.FirstOrDefault(), Is.EqualTo("Course is verplicht."));
        }

        [Test]
        public void ValidateRequiredFields_ShouldReturnErrors_WhenDatePickerHasInvalidDate()
        {
            // Arrange
            var parent = new StackPanel();
            var datePicker = new DatePicker { Tag = "Date" };
            ValidationProperties.SetIsRequired(datePicker, true);
            ValidationProperties.SetIsDate(datePicker, true);
            parent.Children.Add(datePicker);

            // Act
            var errors = ValidationService.ValidateRequiredFields(parent, new List<string>());

            // Assert
            Assert.That(errors.FirstOrDefault(), Is.EqualTo("Date is geen geldige datum."));
        }

        [Test]
        public void ValidateUniqueEmail_ShouldReturnError_WhenEmailExists()
        {
            // Arrange
            var existingEmails = new List<string> { "test@example.com" };
            var email = "test@example.com";

            // Act
            var error = ValidationService.ValidateUniqueEmail(email, existingEmails);

            // Assert
            Assert.That(error, Is.EqualTo("Het emailadres bestaat al."));
        }

        [Test]
        public void ValidateUniqueField_ShouldReturnError_WhenFieldExists()
        {
            // Arrange
            var existingValues = new List<string> { "ExistingValue" };
            var value = "ExistingValue";
            var fieldName = "Field";

            // Act
            var error = ValidationService.ValidateUniqueField(value, existingValues, fieldName);

            // Assert
            Assert.That(error, Is.EqualTo("Field bestaat al."));
        }
    }
}