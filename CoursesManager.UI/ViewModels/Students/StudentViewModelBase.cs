using System.Collections.ObjectModel;

using System.Windows;
using System.Windows.Input;
using CoursesManager.MVVM.Commands;
using CoursesManager.MVVM.Dialogs;
using CoursesManager.UI.Models;
using CoursesManager.UI.Dialogs.ResultTypes;
using CoursesManager.UI.Dialogs.ViewModels;
using CoursesManager.UI.Dialogs.Enums;
using CoursesManager.UI.Repositories.CourseRepository;
using CoursesManager.UI.Repositories.RegistrationRepository;
using CoursesManager.UI.Repositories.StudentRepository;
using CoursesManager.UI.Services;

namespace CoursesManager.UI.ViewModels.Students
{
    public abstract class StudentViewModelBase : DialogViewModel<Student>
    {
        protected readonly IStudentRepository _studentRepository;
        protected readonly ICourseRepository _courseRepository;
        protected readonly IRegistrationRepository _registrationRepository;
        protected readonly IDialogService _dialogService;

        public Student Student { get; protected set; }
        public Student StudentCopy { get; protected set; }
        public ObservableCollection<SelectableCourse> SelectableCourses { get; protected set; }
        public ICommand SaveCommand { get; protected set; }
        public ICommand CancelCommand { get; protected set; }
        public Window ParentWindow { get; set; }

        protected StudentViewModelBase(
            IStudentRepository studentRepository,
            ICourseRepository courseRepository,
            IRegistrationRepository registrationRepository,
            IDialogService dialogService,
            Student? student)
            : base(student)
        {
            _studentRepository = studentRepository ?? throw new ArgumentNullException(nameof(studentRepository));
            _courseRepository = courseRepository ?? throw new ArgumentNullException(nameof(courseRepository));
            _registrationRepository = registrationRepository ?? throw new ArgumentNullException(nameof(registrationRepository));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

            Student = student ?? new Student { Address = new Address() };
            StudentCopy = Student.Copy();
            if (StudentCopy.Address == null)
            {
                StudentCopy.Address = new Address();
            }
            SelectableCourses = InitializeSelectableCourses();

            SaveCommand = new RelayCommand(async () => await OnSaveAsync());
            CancelCommand = new RelayCommand(OnCancel);
        }

        protected ObservableCollection<SelectableCourse> InitializeSelectableCourses()
        {
            var registrations = _registrationRepository.GetAll() ?? new List<Registration>();
            var registeredCourseIds = registrations
                .Where(r => r.StudentId == Student.Id)
                .Select(r => r.CourseId)
                .ToHashSet();

            var courses = _courseRepository.GetAll() ?? new List<Course>();
            var selectableCourses = courses
                .Where(course => course.IsActive)
                .Select(course => new SelectableCourse
                {
                    Id = course.Id,
                    Name = course.Name,
                    IsSelected = registeredCourseIds.Contains(course.Id)
                })
                .ToList();

            return new ObservableCollection<SelectableCourse>(selectableCourses);
        }


        protected async Task<bool> ValidateFields()
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

        protected async Task<bool> ShowDialogAsync(DialogType dialogType, string message, string dialogTitle)
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

        protected async void OnCancel()
        {
            await TriggerEndAnimationAsync();
            var dialogResult = DialogResult<Student>.Builder()
                .SetCanceled("Wijzigingen zijn geannuleerd door de gebruiker.")
                .Build();

            InvokeResponseCallback(dialogResult);
        }

        protected abstract Task OnSaveAsync();

        protected override void InvokeResponseCallback(DialogResult<Student> dialogResult)
        {
            ResponseCallback?.Invoke(dialogResult);
        }
    }
}