using CoursesManager.MVVM.Dialogs;
using CoursesManager.UI.Models;
using CoursesManager.UI.Dialogs.Enums;
using CoursesManager.UI.Repositories.RegistrationRepository;
using CoursesManager.UI.Repositories.StudentRepository;
using CoursesManager.UI.Repositories.CourseRepository;

namespace CoursesManager.UI.ViewModels.Students
{
    public class EditStudentViewModel : StudentViewModelBase
    {
        public EditStudentViewModel(
            IStudentRepository studentRepository,
            ICourseRepository courseRepository,
            IRegistrationRepository registrationRepository,
            IDialogService dialogService,
            Student? student)
            : base(studentRepository, courseRepository, registrationRepository, dialogService, student)
        {
            IsStartAnimationTriggered = true;
        }

        protected override async Task OnSaveAsync()
        {
            if (!await ValidateFields())
            {
                return;
            }

            var result = await ShowDialogAsync(DialogType.Confirmation, "Wilt u de wijzigingen opslaan?", "Bevestiging");
            if (result)
            {
                UpdateStudentDetails();
                UpdateRegistrations();
                await ShowDialogAsync(DialogType.Notify, "Cursist succesvol opgeslagen.", "Succes");

                await TriggerEndAnimationAsync();

                InvokeResponseCallback(DialogResult<Student>.Builder().SetSuccess(Student, "Success").Build());
            }
        }

        private void UpdateStudentDetails()
        {
            Student.Id = StudentCopy.Id;
            Student.FirstName = StudentCopy.FirstName;
            Student.Insertion = StudentCopy.Insertion;
            Student.LastName = StudentCopy.LastName;
            Student.Email = StudentCopy.Email;
            Student.Phone = StudentCopy.Phone;
            if (Student.Address != null && StudentCopy.Address != null)
            {
                Student.Address.ZipCode = StudentCopy.Address.ZipCode;
                Student.Address.Country = StudentCopy.Address.Country;
                Student.Address.City = StudentCopy.Address.City;
                Student.Address.Street = StudentCopy.Address.Street;
                Student.Address.HouseNumber = StudentCopy.Address.HouseNumber;
            }

            _studentRepository.Update(Student);
        }

        private void UpdateRegistrations()
        {
            var existingRegistrations = _registrationRepository.GetAll()
                .Where(r => r.StudentId == Student.Id)
                .ToList();

            // Delete unselected registrations
            foreach (var registration in existingRegistrations)
            {
                if (!SelectableCourses.Any(c => c.Id == registration.CourseId && c.IsSelected))
                {
                    _registrationRepository.Delete(registration.Id);
                }
            }

            // Add new registrations
            foreach (var course in SelectableCourses.Where(c => c.IsSelected))
            {
                if (!existingRegistrations.Any(r => r.CourseId == course.Id))
                {
                    _registrationRepository.Add(new Registration
                    {
                        StudentId = Student.Id,
                        CourseId = course.Id,
                        RegistrationDate = DateTime.Now,
                        IsActive = true
                    });
                }
            }
        }

        public async Task SaveAsync()
        {
            await OnSaveAsync();
        }
    }
}