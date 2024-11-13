﻿using CoursesManager.MVVM.Commands;
using CoursesManager.MVVM.Dialogs;
using CoursesManager.UI.Models.Repositories.CourseRepository;
using CoursesManager.UI.Models.Repositories.RegistrationRepository;
using CoursesManager.UI.Models.Repositories.StudentRepository;
using CoursesManager.UI.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using CoursesManager.UI.Dialogs.ResultTypes;
using CoursesManager.UI.Dialogs.ViewModels;
using CoursesManager.UI.Services;
using CoursesManager.UI.Dialogs.Enums;

public class AddStudentViewModel : DialogViewModel<bool>, INotifyPropertyChanged
{
    private Student _student;
    private readonly IStudentRepository _studentRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IRegistrationRepository _registrationRepository;
    private readonly IDialogService _dialogService;

    public Student Student { get; set; }
    public ObservableCollection<string> Courses { get; set; }
    public string SelectedCourse { get; set; } = string.Empty;
    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public event EventHandler<Student>? StudentAdded;
    public Window ParentWindow { get; set; }

    public new event PropertyChangedEventHandler? PropertyChanged;

    public AddStudentViewModel
        (bool initial,
            IStudentRepository studentRepository,
            ICourseRepository courseRepository,
            IRegistrationRepository registrationRepository,
            IDialogService dialogService)
        : base(initial)
    {
        _studentRepository = studentRepository;
        _courseRepository = courseRepository;
        _registrationRepository = registrationRepository;
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

        Student = new Student();
        Courses = new ObservableCollection<string>(_courseRepository.GetAll().Select(c => c.Name));
        SaveCommand = new RelayCommand(Save);
        CancelCommand = new RelayCommand(Cancel);
        PropertyChanged = delegate { };
    }


    private async void Save()
    {
        if (!await ValidateFields())
        {
            return;
        }

        var course = _courseRepository.GetAll().FirstOrDefault(c => c.Name == SelectedCourse);

        var registration = new Registration
        {
            StudentID = Student.Id,
            Student = Student,
            CourseID = course!.ID,
            Course = course,
            RegistrationDate = DateTime.Now,
            PaymentStatus = false,
            IsActive = true,
            DateCreated = DateTime.Now
        };

        _studentRepository.Add(Student);
        _registrationRepository.Add(registration);

        await ShowDialogAsync(DialogType.Confirmation, "Student succesvol toegevoegd");

        var successDialogResult = DialogResult<bool>.Builder()
            .SetSuccess(true, "Student succesvol toegevoegd")
            .Build();
        StudentAdded?.Invoke(this, Student);
        CloseDialogWithResult(successDialogResult);
    }

    private async Task<bool> ValidateFields()
    {
        if (ParentWindow == null)
        {
            await ShowDialogAsync(DialogType.Error, "Parent window is not set.");
            return false;
        }

        var errors = ValidationService.ValidateRequiredFields(ParentWindow);

        var existingEmails = _studentRepository.GetAll().Select(s => s.Email);
        var emailError = ValidationService.ValidateUniqueField(Student.Email, existingEmails, "Het emailadres");
        if (emailError != null)
        {
            errors.Add(emailError);
        }

        var course = _courseRepository.GetAll().FirstOrDefault(c => c.Name == SelectedCourse);
        if (course == null)
        {
            errors.Add("Geselecteerde cursus niet gevonden");
        }

        if (errors.Any())
        {
            await ShowDialogAsync(DialogType.Error, string.Join("\n", errors));
            return false;
        }

        return true;
    }

    private async Task<bool> ShowDialogAsync(DialogType dialogType, string message)
    {
        switch (dialogType)
        {
            case DialogType.Error:
                await _dialogService.ShowDialogAsync<ConfirmationDialogViewModel, ConfirmationDialogResultType>(
                    new ConfirmationDialogResultType
                    {
                        DialogTitle = "Foutmelding",
                        DialogText = message
                    });
                return true;
            case DialogType.Confirmation:
                var result = await _dialogService.ShowDialogAsync<YesNoDialogViewModel, YesNoDialogResultType>(
                    new YesNoDialogResultType
                    {
                        DialogTitle = "Bevestiging",
                        DialogText = message
                    });
                return result?.Data?.Result ?? false;
            default:
                throw new ArgumentOutOfRangeException(nameof(dialogType), dialogType, null);
        }
    }

    private void Cancel()
    {
        var dialogResult = DialogResult<bool>.Builder()
            .SetSuccess(false, "Operation cancelled")
            .Build();
        CloseDialogWithResult(dialogResult);
    }
    
    protected override void InvokeResponseCallback(DialogResult<bool> dialogResult)
    {
        ResponseCallback?.Invoke(dialogResult);
    }
}