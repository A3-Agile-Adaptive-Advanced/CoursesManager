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
using System.Windows.Input;
using CoursesManager.MVVM.Exceptions;

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
            private set => SetProperty(ref _currentCourse, value);
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
            private set => SetProperty(ref _studentPayments, value);
        }

        public CourseOverViewViewModel(IStudentRepository studentRepository, IRegistrationRepository registrationRepository, ICourseRepository courseRepository, IDialogService dialogService, IMessageBroker messageBroker, INavigationService navigationService, IMailProvider mailProvider) : base(navigationService)
        {
            _studentRepository = studentRepository ?? throw new ArgumentNullException(nameof(studentRepository));
            _registrationRepository = registrationRepository ?? throw new ArgumentNullException(nameof(registrationRepository));

            _courseRepository = courseRepository;
            _dialogService = dialogService;
            _messageBroker = messageBroker;
            _mailProvider = mailProvider;

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
            CurrentCourse = GlobalCache.Instance.Get("Opened Course") as Course;

            if (CurrentCourse != null)
            {
                SetupEmailButtons();

                var registrations = _registrationRepository.GetAll()
                    .Where(r => r.CourseId == CurrentCourse.Id)
                    .ToList();

                var payments = registrations.Select(registration =>
                {
                    var student = _studentRepository.GetById(registration.StudentId);
                    if (student == null)
                    {
                        _messageBroker.Publish(new ToastNotificationMessage(true, "Er is een fout opgetreden, neem contact op met de beheerder", ToastType.Error));
                        return null;
                    }
                    return new CourseStudentPayment(student, registration);
                }).Where(payment => payment != null);

                StudentPayments = new ObservableCollection<CourseStudentPayment>(payments);
            }
            else
            {
                _messageBroker.Publish(new ToastNotificationMessage(true, "Er is een fout opgetreden, neem contact op met de beheerder", ToastType.Error));
            }
        }

        //Making sure the correct email buttons are shown to the user.
        //HasStarted will display the 'send course start mail' button
        //IsPaid will show the 'send payment mail' button
        //IsFinished will display the 'send certificates' button
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

            var existingRegistration = _registrationRepository.GetAll()
                .FirstOrDefault(r => r.CourseId == CurrentCourse.Id && r.StudentId == payment.Student?.Id);

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
                    _messageBroker.Publish(new ToastNotificationMessage(true, "Cursus heeft nog actieve registraties.", ToastType.Error));
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
                            _messageBroker.Publish(new ToastNotificationMessage(true, "Er is een fout opgetreden, neem contact op met de beheerder", ToastType.Error));
                        }
                    }
                }
            });
        }

        private async void SendPaymentMail()
        {
            List<MailResult> mailResults = new ();
            try
            {
                mailResults = await _mailProvider.SendPaymentNotifications(CurrentCourse);
            }
            catch (DataAccessException)
            {
                _messageBroker.Publish(new ToastNotificationMessage(true, "Er is een fout opgetreden, neem contact op met de systeembeheerder.", ToastType.Error));
            }

            CheckMailOutcome(mailResults);
        }

        private async void SendStartCourseMail()
        {
            List<MailResult> mailResults = new();
            try
            {
                mailResults = await _mailProvider.SendCourseStartNotifications(CurrentCourse);
            }
            catch (DataAccessException)
            {
                _messageBroker.Publish(new ToastNotificationMessage(true, "Er is een fout opgetreden, neem contact op met de systeembeheerder.", ToastType.Error));
            }
            CheckMailOutcome(mailResults);
        }

        private async void SendCertificateMail()
        {
            List<MailResult> mailResults = new();
            try
            {
                mailResults = await _mailProvider.SendCertificates(CurrentCourse);
            }
            catch (DataAccessException)
            {
                _messageBroker.Publish(new ToastNotificationMessage(true, "Er is een fout opgetreden, neem contact op met de systeembeheerder.", ToastType.Error));
            }
            CheckMailOutcome(mailResults);
        }

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

        private void CheckMailOutcome(List<MailResult> mailResults)
        {
            string failedEmails = string.Empty;
            foreach (MailResult mailResult in mailResults)
            {
                if (!(mailResult.Outcome == MailOutcome.Success))
                {
                    if (mailResult.MailMessage != null)
                    {
                        failedEmails += mailResult.MailMessage.To.First().Address + "; ";
                    }
                    else
                    {
                        failedEmails += mailResult.StudentName + "; ";
                    }

                }
            }

            if (failedEmails.Length > 0)
            {
                _messageBroker.Publish(new ToastNotificationMessage(true, $"De volgende emails zijn niet verstuurd: {failedEmails}", ToastType.Error));
            }
            else
            {
                _messageBroker.Publish(new ToastNotificationMessage(true, "Email(s) zijn succesvol verstuurd", ToastType.Confirmation));
            }
        }
    }
}