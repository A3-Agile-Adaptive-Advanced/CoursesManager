using CoursesManager.UI.Helpers;
using System.Windows.Controls;

using System.Windows;

/// <summary>
/// Static service providing validation utilities for UI controls.
/// Encapsulates validation logic to ensure consistency and reusability.
/// </summary>
public static class ValidationService
{
    /// <summary>
    /// Validates required fields within the specified parent UI element.
    /// Also checks for email uniqueness and specific formats.
    /// </summary>
    /// <param name="parent">The parent UI element containing controls to validate.</param>
    /// <param name="existingEmails">A list of existing emails to check for duplicates.</param>
    /// <returns>A list of validation error messages, or an empty list if all fields are valid.</returns>
    public static List<string> ValidateRequiredFields(DependencyObject parent, IEnumerable<string> existingEmails)
    {
        var errors = new List<string>();

        foreach (var control in GetAllControls(parent))
        {
            // Check if the control is marked as required.
            if (ValidationProperties.GetIsRequired(control))
            {
                string controlName = GetControlName(control);

                switch (control)
                {
                    case TextBox textBox:
                        ValidateTextBox(textBox, controlName, existingEmails, errors);
                        break;
                    case ComboBox comboBox when comboBox.SelectedItem == null:
                        errors.Add($"{controlName} is verplicht.");
                        break;
                    case DatePicker datePicker:
                        if (ValidationProperties.GetIsDate(control) && !IsValidDate(datePicker.SelectedDate))
                        {
                            errors.Add($"{controlName} is geen geldige datum.");
                        }
                        break;
                }
            }
        }
        return errors;
    }

    /// <summary>
    /// Validates email uniqueness against a list of existing emails.
    /// </summary>
    /// <param name="email">The email to validate.</param>
    /// <param name="existingEmails">A list of existing emails.</param>
    /// <returns>An error message if the email exists; otherwise, null.</returns>
    public static string ValidateUniqueEmail(string email, IEnumerable<string> existingEmails)
    {
        return existingEmails.Contains(email) ? "Het emailadres bestaat al." : null;
    }

    /// <summary>
    /// Checks whether the given email has a valid format.
    /// </summary>
    /// <param name="email">The email to validate.</param>
    /// <returns>True if the email format is valid; otherwise, false.</returns>
    public static bool IsValidEmail(string email)
    {
        return !string.IsNullOrWhiteSpace(email) && email.Contains("@");
    }

    /// <summary>
    /// Checks whether the given string is a valid phone number.
    /// </summary>
    /// <param name="number">The phone number to validate.</param>
    /// <returns>True if the phone number is valid; otherwise, false.</returns>
    public static bool IsPhoneNumber(string number)
    {
        return !string.IsNullOrWhiteSpace(number) && number.All(c => char.IsDigit(c) || c == '-');
    }

    /// <summary>
    /// Validates a generic unique field against existing values.
    /// </summary>
    /// <typeparam name="T">The type of the field being validated.</typeparam>
    /// <param name="value">The field value to check.</param>
    /// <param name="existingValues">A collection of existing values to check against.</param>
    /// <param name="fieldName">The name of the field being validated.</param>
    /// <returns>An error message if the value exists; otherwise, null.</returns>
    public static string ValidateUniqueField<T>(T value, IEnumerable<T> existingValues, string fieldName)
    {
        return existingValues.Contains(value) ? $"{fieldName} bestaat al." : null;
    }

    /// <summary>
    /// Validates if the given date is valid.
    /// </summary>
    /// <param name="date">The date to validate.</param>
    /// <returns>True if the date is valid; otherwise, false.</returns>
    private static bool IsValidDate(DateTime? date)
    {
        return date.HasValue && date.Value != default;
    }

    /// <summary>
    /// Retrieves all controls within the specified parent UI element.
    /// </summary>
    /// <param name="parent">The parent UI element.</param>
    /// <returns>An enumerable collection of child UI elements.</returns>
    private static IEnumerable<DependencyObject> GetAllControls(DependencyObject parent)
    {
        if (parent == null) yield break;

        var queue = new Queue<DependencyObject>();
        queue.Enqueue(parent);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            yield return current;

            foreach (var child in LogicalTreeHelper.GetChildren(current).OfType<DependencyObject>())
            {
                queue.Enqueue(child);
            }
        }
    }

    /// <summary>
    /// Retrieves the display name of a control for validation messages.
    /// </summary>
    /// <param name="control">The control to retrieve the name for.</param>
    /// <returns>The display name of the control.</returns>
    private static string GetControlName(DependencyObject control)
    {
        return control is FrameworkElement element && !string.IsNullOrEmpty(element.Tag?.ToString())
            ? element.Tag.ToString()
            : "Field";
    }

    /// <summary>
    /// Validates a TextBox control based on its properties.
    /// </summary>
    /// <param name="textBox">The TextBox control to validate.</param>
    /// <param name="controlName">The name of the control.</param>
    /// <param name="existingEmails">List of existing emails to check for duplicates.</param>
    /// <param name="errors">The list to add validation error messages to.</param>
    private static void ValidateTextBox(TextBox textBox, string controlName, IEnumerable<string> existingEmails, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(textBox.Text))
        {
            errors.Add($"{controlName} is verplicht.");
        }
        else if (ValidationProperties.GetIsEmail(textBox) && !IsValidEmail(textBox.Text))
        {
            errors.Add($"{controlName} is geen geldig e-mailadres.");
        }
        else if (ValidationProperties.GetIsPhoneNumber(textBox) && !IsPhoneNumber(textBox.Text))
        {
            errors.Add($"{controlName} is geen geldig telefoonnummer.");
        }
        else if (ValidationProperties.GetIsEmail(textBox) && existingEmails.Contains(textBox.Text))
        {
            errors.Add($"{controlName} bestaat al.");
        }
    }
}