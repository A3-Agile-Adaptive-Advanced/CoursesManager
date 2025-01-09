using CoursesManager.MVVM.Dialogs;
using CoursesManager.UI.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using CoursesManager.UI.Dialogs.Enums;
using CoursesManager.UI.Repositories.RegistrationRepository;
using CoursesManager.UI.Repositories.StudentRepository;
using CoursesManager.UI.Repositories.CourseRepository;

namespace CoursesManager.UI.ViewModels.Students;
/// <summary>
/// ViewModel responsible for handling the addition of a new student.
/// required to manage the "AddStudentPopup" dialog.
/// </summary>
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

    /// <summary>
    /// Handles the logic for saving a new student to the repository.
    /// </summary>
    protected override async Task OnSaveAsync()
    {
        if (!await ValidateFields())
        {
            return;
        }

        var course = _courseRepository.GetAll().FirstOrDefault(c => c.Name == SelectedCourse);

        // First we have to add the new student so we able to add the new registration
        _studentRepository.Add(Student);

        // Retrieve the newly added student by email for further operations.
        var newStudent = _studentRepository.GetAll().FirstOrDefault(s => s.Email == Student.Email);

        var registration = new Registration
        {
            StudentId = newStudent.Id,
            Student = newStudent,
            CourseId = course!.Id,
            Course = course,
            RegistrationDate = DateTime.Now,
            PaymentStatus = false,
            IsActive = true
        };

        // Add the registration to the repository.
        _registrationRepository.Add(registration);

        // Trigger the StudentAdded event for any listeners.
        StudentAdded?.Invoke(this, Student);

        // Trigger the end animation to close the dialog gracefully.
        await TriggerEndAnimationAsync();

        // Return a success result to the caller.
        InvokeResponseCallback(DialogResult<Student>.Builder().SetSuccess(Student, "Success").Build());
    }
}