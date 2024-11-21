using CoursesManager.MVVM.Commands;
using CoursesManager.MVVM.Dialogs;
using CoursesManager.UI.Models.Repositories.CourseRepository;
using CoursesManager.UI.Models.Repositories.RegistrationRepository;
using CoursesManager.UI.Models.Repositories.StudentRepository;
using CoursesManager.UI.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using CoursesManager.UI.Dialogs.ResultTypes;
using CoursesManager.UI.Dialogs.ViewModels;
using CoursesManager.UI.Services;
using CoursesManager.UI.Dialogs.Enums;
using CoursesManager.UI.Messages;
using CoursesManager.UI.ViewModels;
using CoursesManager.MVVM.Messages;
using System.Windows.Media.Animation;

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

    public AddStudentViewModel
        (bool initial,
            IStudentRepository studentRepository,
            ICourseRepository courseRepository,
            IRegistrationRepository registrationRepository,
            IDialogService dialogService)
        : base(initial)
    {
        IsStartAnimationTriggered = true;
        _studentRepository = studentRepository;
        _courseRepository = courseRepository;
        _registrationRepository = registrationRepository;
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

        Student = new Student();
        Courses = new ObservableCollection<string>(_courseRepository.GetAll().Select(c => c.Name));
        SaveCommand = new RelayCommand(async () => await Save());
        CancelCommand = new RelayCommand(Cancel);
    }

    public async Task Save()
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

        await ShowDialogAsync(DialogType.Notify, "Student succesvol toegevoegd", "Succes");

        StudentAdded?.Invoke(this, Student);
        IsEndAnimationTriggered = true;
        await Task.Delay(150);
        InvokeResponseCallback(DialogResult<bool>.Builder().SetSuccess(true, "Success").Build());
    }

    public async Task<bool> ValidateFields()
    {
        if (ParentWindow == null)
        {
            await ShowDialogAsync(DialogType.Notify, "Parent window is not set.", "Foutmelding");
            return false;
        }

        var parentContent = ParentWindow.Content as DependencyObject;
        if (parentContent == null)
        {
            await ShowDialogAsync(DialogType.Notify, "Parent window content is invalid.", "Foutmelding");
            return false;
        }

        // Validate required fields
        var errors = ValidationService.ValidateRequiredFields(parentContent);


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
            await ShowDialogAsync(DialogType.Notify, string.Join("\n", errors), "Foutmelding");
            return false;
        }

        return true;
    }

    private async Task<bool> ShowDialogAsync(DialogType dialogType, string message, string dialogTitle)
    {
        void SetIsDialogOpen(bool value)
        {
            if (Application.Current?.Dispatcher?.CheckAccess() == true)
            {
                IsDialogOpen = value;
            }
            else
            {
                Application.Current?.Dispatcher?.Invoke(() => IsDialogOpen = value);
            }
        }

        switch (dialogType)
        {
            case DialogType.Notify:
                SetIsDialogOpen(true);

                await _dialogService.ShowDialogAsync<NotifyDialogViewModel, DialogResultType>(
                    new DialogResultType
                    {
                        DialogTitle = dialogTitle,
                        DialogText = message
                    });

                SetIsDialogOpen(false);
                return true;

            default:
                throw new ArgumentOutOfRangeException(nameof(dialogType), dialogType, null);
        }
    }

    private async void Cancel()   
    {

        IsEndAnimationTriggered = true;
        await Task.Delay(150);
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