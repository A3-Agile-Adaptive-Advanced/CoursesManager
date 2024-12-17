using CoursesManager.MVVM.Dialogs;
using CoursesManager.UI.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using CoursesManager.UI.Dialogs.Enums;
using CoursesManager.UI.Repositories.RegistrationRepository;
using CoursesManager.UI.Repositories.StudentRepository;
using CoursesManager.UI.Repositories.CourseRepository;

namespace CoursesManager.UI.ViewModels.Students;

public class AddStudentViewModel : StudentViewModelBase, INotifyPropertyChanged
{
    public ObservableCollection<string> Courses { get; set; }
    public string SelectedCourse { get; set; } = string.Empty;

    public event EventHandler<Student>? StudentAdded;

    public AddStudentViewModel(
        Student? student,
        IStudentRepository studentRepository,
        ICourseRepository courseRepository,
        IRegistrationRepository registrationRepository,
        IDialogService dialogService)
        : base(studentRepository, courseRepository, registrationRepository, dialogService, new Student { Address = new Address() })
    {
        IsStartAnimationTriggered = true;
        Courses = new ObservableCollection<string>(_courseRepository.GetAll().Where(c => c.IsActive).Select(c => c.Name));
    }

    public async Task SaveAsync()
    {
        await OnSaveAsync();
    }

    protected override async Task OnSaveAsync()
    {
        if (!await ValidateFields())
        {
            return;
        }

        var course = _courseRepository.GetAll().FirstOrDefault(c => c.Name == SelectedCourse);
        var registration = new Registration
        {
            StudentId = Student.Id,
            Student = Student,
            CourseId = course!.Id,
            Course = course,
            RegistrationDate = DateTime.Now,
            PaymentStatus = false,
            IsActive = true
        };

        _studentRepository.Add(Student);
        _registrationRepository.Add(registration);

        await ShowDialogAsync(DialogType.Notify, "Student succesvol toegevoegd", "Succes");

        StudentAdded?.Invoke(this, Student);
        await TriggerEndAnimationAsync();
        InvokeResponseCallback(DialogResult<Student>.Builder().SetSuccess(Student, "Success").Build());
    }
}