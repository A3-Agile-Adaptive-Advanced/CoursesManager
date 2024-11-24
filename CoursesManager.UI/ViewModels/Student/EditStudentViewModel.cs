using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using CoursesManager.MVVM.Commands;
using CoursesManager.MVVM.Dialogs;
using CoursesManager.UI.Models;
using CoursesManager.UI.Dialogs.ResultTypes;
using CoursesManager.UI.Dialogs.ViewModels;
using CoursesManager.UI.Dialogs.Enums;
using CoursesManager.UI.Services;
using CoursesManager.UI.Repositories.RegistrationRepository;
using CoursesManager.UI.Repositories.StudentRepository;
using CoursesManager.UI.Repositories.CourseRepository;

namespace CoursesManager.UI.ViewModels
{
    public class EditStudentViewModel : DialogViewModel<Student>
    {
        private readonly IStudentRepository _studentRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IRegistrationRepository _registrationRepository;
        private readonly IDialogService _dialogService;
        private bool _isDialogOpen;

        public bool IsDialogOpen
        {
            get => _isDialogOpen;
            set => SetProperty(ref _isDialogOpen, value);
        }

        private bool _isEndAnimationTriggered;

        public bool IsEndAnimationTriggered
        {
            get => _isEndAnimationTriggered;
            set => SetProperty(ref _isEndAnimationTriggered, value);
        }

        private bool _isStartAnimationTriggered;

        public bool IsStartAnimationTriggered
        {
            get => _isStartAnimationTriggered;
            set => SetProperty(ref _isStartAnimationTriggered, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public EditStudentViewModel(
            IStudentRepository studentRepository,
            ICourseRepository courseRepository,
            IRegistrationRepository registrationRepository,
            IDialogService dialogService,
            Student? student)
            : base(student)
        {
            IsStartAnimationTriggered = true;
            _studentRepository = studentRepository ?? throw new ArgumentNullException(nameof(studentRepository));
            _courseRepository = courseRepository ?? throw new ArgumentNullException(nameof(courseRepository));
            _registrationRepository = registrationRepository ?? throw new ArgumentNullException(nameof(registrationRepository));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

            Student = student ?? throw new ArgumentNullException(nameof(student));
            StudentCopy = student.Copy();

            SelectableCourses = InitializeSelectableCourses();

            SaveCommand = new RelayCommand(async () => await OnSaveAsync());
            CancelCommand = new RelayCommand(OnCancel);
            ParentWindow = null;
        }
        public Student Student { get; }
        public Student StudentCopy { get; private set; }
        public ObservableCollection<SelectableCourse> SelectableCourses { get; private set; }
        public Window ParentWindow { get; set; }

        private ObservableCollection<SelectableCourse> InitializeSelectableCourses()
        {
            var registeredCourseIds = _registrationRepository.GetAll()
                .Where(r => r.StudentID == Student.Id)
                .Select(r => r.CourseID)
                .ToHashSet();

            var courses = _courseRepository.GetAll()
                .Select(course => new SelectableCourse
                {
                    ID = course.ID,
                    Name = course.Name,
                    IsSelected = registeredCourseIds.Contains(course.ID)
                })
                .ToList();

            return new ObservableCollection<SelectableCourse>(courses);
        }

        public async Task OnSaveAsync()
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

                IsEndAnimationTriggered = true;
                await Task.Delay(150);

                InvokeResponseCallback(DialogResult<Student>.Builder().SetSuccess(Student, "Success").Build());

            }
        }

        private void UpdateStudentDetails()
        {
            Student.FirstName = StudentCopy.FirstName;
            Student.Insertion = StudentCopy.Insertion;
            Student.LastName = StudentCopy.LastName;
            Student.Email = StudentCopy.Email;
            Student.PhoneNumber = StudentCopy.PhoneNumber;
            Student.PostCode = StudentCopy.PostCode;
            Student.Country = StudentCopy.Country;
            Student.City = StudentCopy.City;
            Student.StreetName = StudentCopy.StreetName;
            Student.HouseNumber = StudentCopy.HouseNumber;

            _studentRepository.Update(Student);
        }

        private void UpdateRegistrations()
        {
            var existingRegistrations = _registrationRepository.GetAll()
                .Where(r => r.StudentID == Student.Id)
                .ToList();

            // Delete unselected registrations
            foreach (var registration in existingRegistrations)
            {
                if (!SelectableCourses.Any(c => c.ID == registration.CourseID && c.IsSelected))
                {
                    _registrationRepository.Delete(registration.ID);
                }
            }

            // Add new registrations
            foreach (var course in SelectableCourses.Where(c => c.IsSelected))
            {
                if (!existingRegistrations.Any(r => r.CourseID == course.ID))
                {
                    _registrationRepository.Add(new Registration
                    {
                        StudentID = Student.Id,
                        CourseID = course.ID,
                        RegistrationDate = DateTime.Now,
                        IsActive = true
                    });
                }
            }
        }

        private async void OnCancel()
        {
            IsEndAnimationTriggered = true;
            await Task.Delay(150);
            var dialogResult = DialogResult<Student>.Builder()
                .SetCanceled("Wijzigingen zijn geannuleerd door de gebruiker.")
                .Build();

            InvokeResponseCallback(dialogResult);
        }

        private async Task<bool> ValidateFields()
        {
            if (ParentWindow == null)
            {
                await ShowDialogAsync(DialogType.Notify, "Parentvenster is niet ingesteld.", "Foutmelding");
                return false;
            }

            var errors = ValidationService.ValidateRequiredFields(ParentWindow);

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

                case DialogType.Confirmation:
                    SetIsDialogOpen(true);

                    var result = await _dialogService.ShowDialogAsync<ConfirmationDialogViewModel, DialogResultType>(
                        new DialogResultType()
                        {
                            DialogTitle = dialogTitle,
                            DialogText = message
                        });
                    
                    SetIsDialogOpen(false);
                    return result?.Data?.Result ?? false;

                default:
                    throw new ArgumentOutOfRangeException(nameof(dialogType), dialogType, null);
            }
        }

        protected override void InvokeResponseCallback(DialogResult<Student> dialogResult)
        {
            ResponseCallback?.Invoke(dialogResult);
        }
    }
}
