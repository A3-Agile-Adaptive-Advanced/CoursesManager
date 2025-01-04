using CoursesManager.MVVM.Commands;
using CoursesManager.MVVM.Data;
using CoursesManager.MVVM.Dialogs;
using CoursesManager.MVVM.Mail;
using CoursesManager.MVVM.Messages;
using CoursesManager.MVVM.Navigation;
using CoursesManager.UI.Dialogs.ResultTypes;
using CoursesManager.UI.Dialogs.ViewModels;
using CoursesManager.UI.Enums;
using CoursesManager.UI.Mailing;
using CoursesManager.UI.Models;
using CoursesManager.UI.Repositories.CourseRepository;
using CoursesManager.UI.Repositories.RegistrationRepository;
using CoursesManager.UI.Repositories.StudentRepository;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using CoursesManager.MVVM.Exceptions;
using System.Linq;
using System.Text;

namespace CoursesManager.UI.ViewModels.Courses
{
    public class CourseOverViewViewModel : ViewModelWithNavigation
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IDialogService _dialogService;
        private readonly IMessageBroker _messageBroker;
        private readonly IMailProvider _mailProvider;
        public ICommand ChangeCourseCommand { get; set; }
        public ICommand DeleteCourseCommand { get; set; }
        public ICommand CheckboxChangedCommand { get; }

        public ICommand CertificateMailCommand { get; set; }
        public ICommand PaymentMailCommand { get; set; }
        public ICommand StartCourseMailCommand { get; set; }

        private readonly IStudentRepository _studentRepository;
        private readonly IRegistrationRepository _registrationRepository;

        private Course _currentCourse;

        public Course CurrentCourse
        {
            get => _currentCourse;
            set => SetProperty(ref _currentCourse, value);
        }

        private bool _isPaid;

        public bool IsPaid
        {
            get => _isPaid;
            set => SetProperty(ref _isPaid, value);
        }

        private bool _hasStarted;

        public bool HasStarted
        {
            get => _hasStarted;
            set => SetProperty(ref _hasStarted, value);
        }

        private bool _isFinished;

        public bool IsFinished
        {
            get => _isFinished;
            set => SetProperty(ref _isFinished, value);
        }

        private ObservableCollection<Student> _students;

        private ObservableCollection<CourseStudentPayment> _studentPayments;

        public ObservableCollection<CourseStudentPayment> StudentPayments
        {
            get => _studentPayments;
            set => SetProperty(ref _studentPayments, value);
        }

        public CourseOverViewViewModel(IStudentRepository studentRepository,
            IRegistrationRepository registrationRepository, ICourseRepository courseRepository,
            IDialogService dialogService, IMessageBroker messageBroker, INavigationService navigationService,
            IMailProvider mailProvider) : base(navigationService)
        {
            _studentRepository = studentRepository ?? throw new ArgumentNullException(nameof(studentRepository));
            _registrationRepository =
                registrationRepository ?? throw new ArgumentNullException(nameof(registrationRepository));

            _courseRepository = courseRepository;
            _dialogService = dialogService;
            _messageBroker = messageBroker;
            _mailProvider = mailProvider;

            ViewTitle = "Cursus details";
            CurrentCourse = GlobalCache.Instance.Get("Opened Course") as Course;

            ChangeCourseCommand = new RelayCommand(ChangeCourse);
            DeleteCourseCommand = new RelayCommand(DeleteCourse);
            CheckboxChangedCommand = new RelayCommand<CourseStudentPayment>(OnCheckboxChanged);
            PaymentMailCommand = new RelayCommand(SendPaymentMail);
            StartCourseMailCommand = new RelayCommand(SendStartCourseMail);
            CertificateMailCommand = new RelayCommand(SendCertificateMail);

            LoadCourseData();

        }

        private void LoadCourseData()
        {
            CurrentCourse = _courseRepository.GetById(CurrentCourse.Id);

            if (CurrentCourse != null)
            {
                SetupEmailButtons();

                var registrations = CurrentCourse.Registrations;

                var payments = registrations.Select(registration =>
                {
                    var student = _studentRepository.GetById(registration.StudentId);
                    if (student == null)
                    {
                        _messageBroker.Publish(new ToastNotificationMessage(true,
                            "Er is een fout opgetreden, neem contact op met de beheerder", ToastType.Error, false));
                        return null;
                    }

                    return new CourseStudentPayment(student, registration);
                }).Where(payment => payment != null);

                StudentPayments = new ObservableCollection<CourseStudentPayment>(payments);
            }
            else
            {
                _messageBroker.Publish(new ToastNotificationMessage(true,
                    "Er is een fout opgetreden, neem contact op met de beheerder", ToastType.Error, false));
            }
        }

        // Making sure the correct email buttons are shown to the user, never do we need all 3 to be shown.
        // For cleaner look this method will determine which buttons are visible to the user.
        // HasStarted will display the 'send course start mail' button
        // IsPaid will show the 'send payment mail' button
        // IsFinished will display the 'send certificates' button
        private void SetupEmailButtons()
        {
            if (CurrentCourse.Students != null)
            {
                IsPaid = !CurrentCourse.IsPayed;
                HasStarted = false;
                IsFinished = false;
                if ((CurrentCourse.StartDate - DateTime.Now).TotalDays <= 7 && CurrentCourse.StartDate > DateTime.Now)
                {
                    HasStarted = true;
                }

                if (CurrentCourse.EndDate <= DateTime.Now)
                {
                    HasStarted = false;
                    IsPaid = false;
                    IsFinished = true;
                }
            }
        }

        private void OnCheckboxChanged(CourseStudentPayment payment)
        {
            if (payment == null || CurrentCourse == null) return;

            var existingRegistration = CurrentCourse.Registrations
                .FirstOrDefault(r => r.StudentId == payment.Student.Id);

            if (existingRegistration != null)
            {
                if (!payment.IsPaid)
                {
                    CurrentCourse.IsPayed = payment.IsPaid;
                }

                existingRegistration.PaymentStatus = payment.IsPaid;
                existingRegistration.IsAchieved = payment.IsAchieved;
                _registrationRepository.Update(existingRegistration);
            }
            else if (payment.IsPaid || payment.IsAchieved)
            {
                _registrationRepository.Add(new Registration
                {
                    StudentId = payment.Student?.Id ?? 0,
                    CourseId = CurrentCourse.Id,
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

            await ExecuteWithOverlayAsync(_messageBroker, async () =>
            {
                if (_registrationRepository.GetAllRegistrationsByCourse(CurrentCourse).Any())
                {
                    _messageBroker.Publish(new ToastNotificationMessage(true, "Cursus heeft nog actieve registraties.",
                        ToastType.Error, false));
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
                            _courseRepository.Delete(CurrentCourse);

                            //_messageBroker.Publish(new CoursesChangedMessage());
                            _navigationService.GoBackAndClearForward();
                        }
                        catch (Exception ex)
                        {
                            LogUtil.Error(ex.Message);
                            _messageBroker.Publish(new ToastNotificationMessage(true,
                                "Er is een fout opgetreden, neem contact op met de beheerder", ToastType.Error, false));
                        }
                    }
                }
            });
        }

        #region MailMethods

        private async void SendPaymentMail()
        {
            await SendEmailAsync(
                () => _mailProvider.SendPaymentNotifications(CurrentCourse)
            );
        }

        private async void SendStartCourseMail()
        {
            await SendEmailAsync(
                () => _mailProvider.SendCourseStartNotifications(CurrentCourse)
            );
        }

        private async void SendCertificateMail()
        {
            await SendEmailAsync(
                () => _mailProvider.SendCertificates(CurrentCourse)
            );
        }

        // Due to the nature of similarity between the send mail commands this task is created to reduce duplication.
        // For user feedback a permanent message is sent to show that sending is still being done.
        // This is important because with larger address lists the task can take a while to complete.
        private async Task SendEmailAsync(Func<Task<List<MailResult>>> sendEmailTask)
        {
            _messageBroker.Publish(new ToastNotificationMessage(true, "E-mails versturen", ToastType.Info, true));
            List<MailResult> mailResults = new();

            try
            {
                mailResults = await sendEmailTask();
            }
            catch (DataAccessException)
            {
                _messageBroker.Publish(new ToastNotificationMessage(true,
                    "Er is een fout opgetreden, neem contact op met de systeembeheerder.", ToastType.Error, false));
            }
            catch (InvalidOperationException)
            {
                _messageBroker.Publish(new ToastNotificationMessage(true,
                    "Er zijn geen studenten aangemeld bij deze cursus", ToastType.Error, false));
            }

            CheckMailOutcome(mailResults);
        }

        private void CheckMailOutcome(List<MailResult> mailResults)
        {
            if (!mailResults.Any())
            {
                _messageBroker.Publish(new ToastNotificationMessage(true,
                    "De huidige studenten met afgeronde status \n hebben al een mail ontvangen", ToastType.Warning, false));
                return;
            }
            var failedEmails = new StringBuilder();
            foreach (var mailResult in mailResults.Where(r => r.Outcome != MailOutcome.Success))
            {
                failedEmails.Append(mailResult.MailMessage?.To.FirstOrDefault()?.Address ?? mailResult.StudentName);
                failedEmails.Append("; ");
            }

            if (failedEmails.Length > 0)
            {
                _messageBroker.Publish(new ToastNotificationMessage(true,
                    $"De volgende emails zijn niet verstuurd: {failedEmails}", ToastType.Error, false));
                return;
            }

            _messageBroker.Publish(new ToastNotificationMessage(true, "Email(s) zijn succesvol verstuurd",
                ToastType.Confirmation, false));
        }


        #endregion

        private async void ChangeCourse()
        {
            await ExecuteWithOverlayAsync(_messageBroker, async () =>
            {
                var dialogResult = await _dialogService.ShowDialogAsync<CourseDialogViewModel, Course>(CurrentCourse);


                if (dialogResult.Outcome == DialogOutcome.Success)
                {
                    _messageBroker.Publish(new CoursesChangedMessage());
                    CurrentCourse = dialogResult.Data;
                }

            });
        }
    }
}