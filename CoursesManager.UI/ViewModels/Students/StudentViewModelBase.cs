using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using CoursesManager.MVVM.Commands;
using CoursesManager.MVVM.Dialogs;
using CoursesManager.UI.Dialogs.Enums;
using CoursesManager.UI.Dialogs.ResultTypes;
using CoursesManager.UI.Dialogs.ViewModels;
using CoursesManager.UI.Models;
using CoursesManager.UI.Repositories.CourseRepository;
using CoursesManager.UI.Repositories.RegistrationRepository;
using CoursesManager.UI.Repositories.StudentRepository;
using CoursesManager.UI.Services;

namespace CoursesManager.UI.ViewModels.Students
{
    /// <summary>
    /// The base class for all student-related ViewModels.
    /// It implements common functionality such as saving, canceling, and initializing selectable courses.
    /// This follows the MVVM pattern to separate concerns and ensure a clean architecture.
    /// </summary>
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
        public ICommand EditStudentCommand { get; }

        private Window? _parentWindow;

        /// <summary>
        /// Gets or sets the parent window for the current ViewModel.
        /// This property ensures that the parent window is always determined, even if not explicitly set:
        /// - If _parentWindow is not set, it attempts to find the currently active window.
        /// - As a fallback, it uses the application's main window.
        /// 
        /// This approach ensures that modal dialogs or UI-related actions always have a valid parent window,
        /// aligning with the MVVM pattern by abstracting away direct dependencies on specific window instances.
        /// </summary>
        /// 
        public Window ParentWindow
        {
            get => _parentWindow ?? Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive) ?? Application.Current.MainWindow;
            set => _parentWindow = value;
        }

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

        /// <summary>
        /// Validates that all required fields are properly filled out.
        /// Utilizes ValidationService to ensure no code duplication and encapsulates validation logic.
        /// </summary>
        protected async Task<bool> ValidateFields()
        {
            if (ParentWindow == null)
            {
                await ShowDialogAsync(DialogType.Notify, "Parentvenster is niet ingesteld.", "Foutmelding");
                return false;
            }

            var existingEmails = _studentRepository.GetAll().Select(s => s.Email);
            var errors = ValidationService.ValidateRequiredFields(ParentWindow, existingEmails);

            if (errors.Any())
            {
                await ShowDialogAsync(DialogType.Notify, string.Join("\n", errors), "Foutmelding");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Displays a dialog of the specified type with a custom message.
        /// Abstracts dialog display logic to centralize it in one method.
        /// </summary>
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

        /// This method must be implemented by subclasses to provide specific save logic.
        protected abstract Task OnSaveAsync();

        /// Invokes the response callback, ensuring the consumer of this ViewModel is notified.
        protected override void InvokeResponseCallback(DialogResult<Student> dialogResult)
        {
            ResponseCallback?.Invoke(dialogResult);
        }
    }
}