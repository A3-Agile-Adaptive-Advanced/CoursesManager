﻿using CoursesManager.UI.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CoursesManager.UI.Services
{
    public static class ValidationService
    {
        public static List<string> ValidateRequiredFields(DependencyObject parent)
        {
            var errors = new List<string>();

            foreach (var control in GetAllControls(parent))
            {
                if (ValidationProperties.GetIsRequired(control))
                {
                    string controlName = GetControlName(control);

                    if (control is TextBox textBox)
                    {
                        if (string.IsNullOrWhiteSpace(textBox.Text))
                        {
                            errors.Add($"{controlName} is verplicht.");
                        }
                        else if (ValidationProperties.GetIsEmail(control) && !IsValidEmail(textBox.Text))
                        {
                            errors.Add($"{controlName} is geen geldig e-mailadres.");
                        }
                        else if (ValidationProperties.GetIsPhoneNumber(control) && !IsPhoneNumber(textBox.Text))
                        {
                            errors.Add($"{controlName} is geen geldig telefoonnummer.");
                        }
                    }
                    else if (control is ComboBox comboBox && comboBox.SelectedItem == null)
                    {
                        errors.Add($"{controlName} is verplicht.");
                    }
                }
            }
            return errors;
        }


        public static bool IsValidEmail(string email)
        {
            return !string.IsNullOrWhiteSpace(email) && email.Contains("@");
        }

        public static bool IsNumber(string number)
        {
            return !string.IsNullOrWhiteSpace(number) && number.All(char.IsDigit);
        }
        public static bool IsPhoneNumber(string number)
        {
            return !string.IsNullOrWhiteSpace(number) && number.All(c => char.IsDigit(c) || c == '-');
        }
        public static string ValidateUniqueEmail(string email, IEnumerable<string> existingEmails)
        {
            if (existingEmails.Contains(email))
            {
                return "Het emailadres bestaat al.";
            }
            return null;
        }
        public static string ValidateUniqueField<T>(T value, IEnumerable<T> existingValues, string fieldName)
        {
            if (existingValues.Contains(value))
            {
                return $"{fieldName} bestaat al.";
            }
            return null;
        }

        private static IEnumerable<DependencyObject> GetAllControls(DependencyObject parent)
        {
            if (parent == null) yield break;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                yield return child;

                foreach (var descendant in GetAllControls(child))
                {
                    yield return descendant;
                }
            }
        }

        private static string GetControlName(DependencyObject control)
        {
            if (control is FrameworkElement element && !string.IsNullOrEmpty(element.Tag?.ToString()))
            {
                return element.Tag.ToString();
            }
            return "Field";
        }
    }
}