﻿using CoursesManager.MVVM.Commands;
using CoursesManager.MVVM.Dialogs;
using CoursesManager.UI.Models.Repositories.CourseRepository;
using CoursesManager.UI.Models.Repositories.RegistrationRepository;
using CoursesManager.UI.Models.Repositories.StudentRepository;
using CoursesManager.UI.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Input;

public class AddStudentViewModel : DialogViewModel<bool>, INotifyPropertyChanged
{
    private Student _student;
    private readonly IStudentRepository _studentRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IRegistrationRepository _registrationRepository;
    public event EventHandler<Student> StudentAdded;

    public Student Student
    {
        get => _student;
        set
        {
            _student = value;
            OnPropertyChanged(nameof(Student));
        }
    }

    public ObservableCollection<string> Courses { get; set; }
    public string SelectedCourse { get; set; }

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public event PropertyChangedEventHandler PropertyChanged;

    // Default constructor
    public AddStudentViewModel() : this(new StudentRepository(), new CourseRepository(), new RegistrationRepository())
    {
    }

    // Parameterized constructor for testing
    public AddStudentViewModel(IStudentRepository studentRepository, ICourseRepository courseRepository, IRegistrationRepository registrationRepository) : base(false)
    {
        _studentRepository = studentRepository;
        _courseRepository = courseRepository;
        _registrationRepository = registrationRepository;
        Student = new Student();
        Courses = new ObservableCollection<string>(_courseRepository.GetAll().Select(c => c.CourseName));
        SaveCommand = new RelayCommand(Save);
        CancelCommand = new RelayCommand(Cancel);
        PropertyChanged = delegate { };
    }

    private bool FieldsValidations()
    {
        return !string.IsNullOrEmpty(Student.FirstName) &&
               !string.IsNullOrEmpty(Student.LastName) &&
               !string.IsNullOrEmpty(Student.Email) &&
               !string.IsNullOrEmpty(Student.PhoneNumber) &&
               !string.IsNullOrEmpty(Student.PostCode) &&
               !string.IsNullOrEmpty(SelectedCourse) &&
               IsValidEmail(Student.Email) &&
               IsNumber(Student.PhoneNumber) &&
               IsUniqueEmail(Student.Email);
    }

    private void Save()
    {
        if (!FieldsValidations())
        {
            throw new Exception("Invalid student data");
        }
        _studentRepository.Add(Student);

        var course = _courseRepository.GetAll().FirstOrDefault(c => c.CourseName == SelectedCourse);
        if (course != null)
        {
            var registration = new Registration
            {
                StudentID = Student.Id,
                Student = Student,
                CourseID = course.ID,
                Course = course,
                RegistrationDate = DateTime.Now,
                PaymentStatus = false,
                IsActive = true,
                DateCreated = DateTime.Now
            };

            _registrationRepository.Add(registration);
        }
        else
        {
            throw new Exception("Selected course not found");
        }

        var dialogResult = DialogResult<bool>.Builder()
            .SetSuccess(true, "Student succesvol toegevoegd")
            .Build();
        System.Diagnostics.Debug.WriteLine($"DialogResult created with message: {dialogResult.OutcomeMessage}");

        StudentAdded?.Invoke(this, Student);
        CloseDialogWithResult(dialogResult);
    }

    private void Cancel()
    {
        var dialogResult = DialogResult<bool>.Builder()
            .SetSuccess(false, "Operation cancelled")
            .Build();
        CloseDialogWithResult(dialogResult);
    }

    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private bool IsNumber(string phoneNumber)
    {
        return Regex.IsMatch(phoneNumber, @"^\d+$");
    }

    private bool IsUniqueEmail(string email)
    {
        return !_studentRepository.EmailExists(email);
    }

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected override void InvokeResponseCallback(DialogResult<bool> dialogResult)
    {
        ResponseCallback?.Invoke(dialogResult);
    }
}