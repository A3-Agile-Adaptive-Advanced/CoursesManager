using CoursesManager.MVVM.Commands;
using CoursesManager.MVVM.Data;
using CoursesManager.MVVM.Dialogs;
using CoursesManager.MVVM.Messages;
using CoursesManager.MVVM.Navigation;
using CoursesManager.UI.Messages;
using CoursesManager.UI.Models;
using CoursesManager.UI.ViewModels.Students;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CoursesManager.UI.Repositories.CourseRepository;
using CoursesManager.UI.Repositories.RegistrationRepository;
using CoursesManager.UI.Repositories.StudentRepository;
using CoursesManager.UI.Dialogs.ViewModels;
using CoursesManager.UI.Dialogs.ResultTypes;

namespace CoursesManager.UI.ViewModels.Courses
{
    internal class CourseOverViewViewModel : ViewModelWithNavigation
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IDialogService _dialogService;
        private readonly IMessageBroker _messageBroker;

        public ICommand ChangeCourseCommand { get; set; }
        public ICommand DeleteCourseCommand { get; set; }
        public ICommand CheckboxChangedCommand { get; }


        private readonly IStudentRepository _studentRepository;
        private readonly IRegistrationRepository _registrationRepository;

        private Course _currentCourse;
        public Course CurrentCourse
        {
            get => _currentCourse;
            private set => SetProperty(ref _currentCourse, value);
        }

        private ObservableCollection<Student> _students;
        public ObservableCollection<Student> Students
        {
            get => _students;
            private set => SetProperty(ref _students, value);
        }

        private ObservableCollection<CourseStudentPayment> _studentPayments;
        public ObservableCollection<CourseStudentPayment> StudentPayments
        {
            get => _studentPayments;
            private set => SetProperty(ref _studentPayments, value);
        }


        public CourseOverViewViewModel(IStudentRepository studentRepository, IRegistrationRepository registrationRepository, ICourseRepository courseRepository, IDialogService dialogService, IMessageBroker messageBroker, INavigationService navigationService) : base(navigationService)
        {
            _studentRepository = studentRepository ?? throw new ArgumentNullException(nameof(studentRepository));
            _registrationRepository = registrationRepository ?? throw new ArgumentNullException(nameof(registrationRepository));

            _courseRepository = courseRepository;
            _dialogService = dialogService;
            _messageBroker = messageBroker;

            ChangeCourseCommand = new RelayCommand(ChangeCourse);
            DeleteCourseCommand = new RelayCommand(DeleteCourse);
            CheckboxChangedCommand = new RelayCommand<CourseStudentPayment>(OnCheckboxChanged);


            LoadCourseData();
        }

        private void LoadCourseData()
        {
            CurrentCourse = (Course)GlobalCache.Instance.Get("Opened Course");

            if (CurrentCourse == null)
            {
                throw new InvalidOperationException("No course is currently opened. Ensure the course is loaded in the GlobalCache.");
            }

            Students = new ObservableCollection<Student>(
                _studentRepository.GetAll()
                    .Where(s => s.Courses != null && s.Courses.Any(c => c != null && c.ID == CurrentCourse.ID))
            );

            var registrations = _registrationRepository.GetAll()
                .Where(r => r.CourseID == CurrentCourse.ID)
                .ToList();

            var payments = registrations.Select(registration =>
            {
                var student = _studentRepository.GetById(registration.StudentID);
                if (student == null)
                {
                    Console.WriteLine($"Warning: Student with ID {registration.StudentID} not found for registration ID {registration.ID}.");
                    return null;
                }
                return new CourseStudentPayment(student, registration);
            }).Where(payment => payment != null);

            StudentPayments = new ObservableCollection<CourseStudentPayment>(payments);
        }

        private void OnCheckboxChanged(CourseStudentPayment payment)
        {
            if (payment == null || CurrentCourse == null) return;

            var existingRegistration = _registrationRepository.GetAll()
                .FirstOrDefault(r => r.CourseID == CurrentCourse.ID && r.StudentID == payment.Student?.Id);

            if (existingRegistration != null)
            {
                existingRegistration.PaymentStatus = payment.IsPaid;
                existingRegistration.IsAchieved = payment.IsAchieved;
                _registrationRepository.Update(existingRegistration);
            }
            else if (payment.IsPaid || payment.IsAchieved)
            {
                _registrationRepository.Add(new Registration
                {
                    StudentID = payment.Student?.Id ?? 0,
                    CourseID = CurrentCourse.ID,
                    PaymentStatus = payment.IsPaid,
                    IsAchieved = payment.IsAchieved,
                    RegistrationDate = DateTime.Now,
                    IsActive = true
                });
            }
            LoadCourseData();
        }

        private async void DeleteCourse()
        {

            await ExecuteWithOverlayAsync(async () =>
            {
                if (_courseRepository.HasActiveRegistrations(CurrentCourse))
                {
                    var result = await _dialogService.ShowDialogAsync<ErrorDialogViewModel, DialogResultType>(
                        new DialogResultType
                        {
                            DialogText = "Cursus heeft nog actieve registraties.",
                            DialogTitle = "Error"
                        });
                }
                else
                {
                    var result = await _dialogService.ShowDialogAsync<ConfirmationDialogViewModel, DialogResultType>(
                        new DialogResultType
                        {
                            DialogTitle = "Bevestiging",
                            DialogText = "Weet je zeker dat je deze cursus wilt verwijderen?"
                        });

                    if (result.Outcome == DialogOutcome.Success && result.Data is not null && result.Data.Result)
                    {
                        try
                        {
                            _courseRepository.SetInactive(CurrentCourse);

                            _messageBroker.Publish(new CoursesChangedMessage());
                            _navigationService.GoBackAndClearForward();
                        }
                        catch (Exception ex)
                        {
                            LogUtil.Error(ex.Message);
                            await _dialogService.ShowDialogAsync<ErrorDialogViewModel, DialogResultType>(
                                new DialogResultType
                                {
                                    DialogText = "Er is iets fout gegaan.",
                                    DialogTitle = "Error"
                                });
                        }
                    }
                }
            });
        }

        private async void ChangeCourse()
        {

            await ExecuteWithOverlayAsync(async () =>
            {
                var dialogResult = await _dialogService.ShowDialogAsync<CourseDialogViewModel, Course>(CurrentCourse);

                if (dialogResult.Outcome == DialogOutcome.Success)
                {
                    CurrentCourse = dialogResult.Data;
                }
            });
        }

        private async Task ExecuteWithOverlayAsync(Func<Task> action)
        {
            _messageBroker.Publish(new OverlayActivationMessage(true));
            try
            {
                await action();
            }
            finally
            {
                _messageBroker.Publish(new OverlayActivationMessage(false));
            }
        }

        private System.Windows.Visibility isImageVisible;

        public System.Windows.Visibility IsImageVisible { get => isImageVisible; set => SetProperty(ref isImageVisible, value); }

        private System.Windows.Visibility isPlaceholderVisible;

        public System.Windows.Visibility IsPlaceholderVisible { get => isPlaceholderVisible; set => SetProperty(ref isPlaceholderVisible, value); }
    }
}