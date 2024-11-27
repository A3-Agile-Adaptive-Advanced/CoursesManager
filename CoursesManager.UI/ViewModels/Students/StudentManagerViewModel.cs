using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Printing;
using System.Windows;
using System.Windows.Input;
using CoursesManager.MVVM.Commands;
using CoursesManager.MVVM.Data;
using CoursesManager.MVVM.Dialogs;
using CoursesManager.MVVM.Messages;
using CoursesManager.MVVM.Navigation;
using CoursesManager.UI.Dialogs.ResultTypes;
using CoursesManager.UI.Dialogs.ViewModels;
using CoursesManager.UI.Messages;
using CoursesManager.UI.Models;
using CoursesManager.UI.Repositories.CourseRepository;
using CoursesManager.UI.Repositories.RegistrationRepository;
using CoursesManager.UI.Repositories.StudentRepository;

namespace CoursesManager.UI.ViewModels.Students
{
    public class StudentManagerViewModel : ViewModelWithNavigation
    {
        private readonly IDialogService _dialogService;
        private readonly IMessageBroker _messageBroker;
        private readonly IStudentRepository _studentRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IRegistrationRepository _registrationRepository;
        public ObservableCollection<Student> Students { get; set; }
        public ObservableCollection<Student> FilteredStudentRecords { get; set; }

        private string _searchText;

        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        private Student _selectedStudent;

        public Student SelectedStudent
        {
            get => _selectedStudent;
            set
            {
                if (SetProperty(ref _selectedStudent, value))
                {
                    UpdateStudentCourses();
                }
            }
        }

        private bool _isDialogOpen;

        public bool IsDialogOpen
        {
            get => _isDialogOpen;
            set => SetProperty(ref _isDialogOpen, value);
        }

        private ObservableCollection<CourseStudentPayment> _coursePaymentList;

        public ObservableCollection<CourseStudentPayment> CoursePaymentList
        {
            get => _coursePaymentList;
            set => SetProperty(ref _coursePaymentList, value);
        }

        #region Commands

        public ICommand AddStudentCommand { get; }
        public ICommand EditStudentCommand { get; }
        public ICommand DeleteStudentCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand StudentDetailCommand { get; }
        public ICommand CheckboxChangedCommand { get; }


        #endregion Commands

        public StudentManagerViewModel(
            IDialogService dialogService,
            IStudentRepository studentRepository,
            ICourseRepository courseRepository,
            IRegistrationRepository registrationRepository,
            IMessageBroker messageBroker,
            INavigationService navigationService) : base(navigationService)
        {
            _messageBroker = messageBroker;
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _studentRepository = studentRepository ?? throw new ArgumentNullException(nameof(studentRepository));
            _courseRepository = courseRepository ?? throw new ArgumentNullException(nameof(courseRepository));
            _registrationRepository = registrationRepository ?? throw new ArgumentNullException(nameof(registrationRepository));
            _navigationService = navigationService;
            CoursePaymentList = new ObservableCollection<CourseStudentPayment>();

            // Initialize Students
            LoadStudents();

            // Commands
            AddStudentCommand = new RelayCommand(OpenAddStudentPopup);
            EditStudentCommand = new RelayCommand<Student>(OpenEditStudentPopup, s => s != null);
            DeleteStudentCommand = new RelayCommand<Student>(OpenDeleteStudentPopup, s => s != null);
            SearchCommand = new RelayCommand(FilterStudentRecords);
            StudentDetailCommand = new RelayCommand(OpenStudentDetailViewModel);
            CheckboxChangedCommand = new RelayCommand<CourseStudentPayment>(OnCheckboxChanged);
            ViewTitle = "Cursisten beheer";
        }

        public void LoadStudents()
        {
            Students = new ObservableCollection<Student>(_studentRepository.GetAll());
            FilteredStudentRecords = new ObservableCollection<Student>(Students);
        }

        private void FilterStudentRecords()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FilteredStudentRecords = new ObservableCollection<Student>(Students);
            }
            else
            {
                var searchTerm = SearchText.Trim().ToLower();
                var filtered = Students.Where(s => s.TableFilter().ToLower().Contains(searchTerm)).ToList();
                FilteredStudentRecords = new ObservableCollection<Student>(filtered);
            }
            OnPropertyChanged(nameof(FilteredStudentRecords));
        }
        private void OnCheckboxChanged(CourseStudentPayment payment)
        {
            if (payment == null || SelectedStudent == null) return;

            var existingRegistration = _registrationRepository.GetAll()
                .FirstOrDefault(r => r.CourseID == payment.Course?.ID && r.StudentID == SelectedStudent.Id);

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
                    StudentID = SelectedStudent.Id,
                    CourseID = payment.Course?.ID ?? 0,
                    PaymentStatus = payment.IsPaid,
                    IsAchieved = payment.IsAchieved,
                    RegistrationDate = DateTime.Now,
                    IsActive = true
                });
            }
            UpdateStudentCourses();
        }

        private void UpdateStudentCourses()
        {
            if (SelectedStudent == null) return;

            var registrations = _registrationRepository.GetAll().Where(r => r.StudentID == SelectedStudent.Id);
            CoursePaymentList.Clear();

            foreach (var registration in registrations)
            {
                if (registration.Course != null)
                {
                    CoursePaymentList.Add(new CourseStudentPayment(registration.Course, registration));
                }
            }
            OnPropertyChanged(nameof(CoursePaymentList));
        }


        private async void OpenAddStudentPopup()
        {
            await ExecuteWithOverlayAsync(async () =>
            {
                var dialogResult = await _dialogService.ShowDialogAsync<AddStudentViewModel, bool>(true);

                if (dialogResult?.Data == true && dialogResult.Outcome == DialogOutcome.Success)
                {
                    LoadStudents();
                }
            });
        }

        private async void OpenEditStudentPopup(Student student)
        {
            if (student == null)
                await ExecuteWithOverlayAsync(async () =>
                {
                    {
                        await _dialogService.ShowDialogAsync<NotifyDialogViewModel, DialogResultType>(
                            new DialogResultType
                            {
                                DialogTitle = "Error",
                                DialogText = "Geen student geselecteerd om te bewerken."
                            });
                        return;
                    }
                });

            if (student == null) return;
            await ExecuteWithOverlayAsync(async () =>
            {
                var dialogResult = await _dialogService.ShowDialogAsync<EditStudentViewModel, Student>(student);

                if (dialogResult?.Outcome == DialogOutcome.Success)
                {
                    LoadStudents();
                }
            });
        }

        private async void OpenDeleteStudentPopup(Student student)
        {
            if (student == null) return;

            await ExecuteWithOverlayAsync(async () =>
            {
                var confirmation = await _dialogService.ShowDialogAsync<ConfirmationDialogViewModel, DialogResultType>(
                    new DialogResultType
                    {
                        DialogTitle = "Bevestiging",
                        DialogText = "Wilt u deze cursist verwijderen?"
                    });

                if (confirmation?.Data?.Result == true)
                {
                    student.Is_deleted = true;
                    student.date_deleted = DateTime.Now;
                    _studentRepository.Update(student);
                    await _dialogService.ShowDialogAsync<NotifyDialogViewModel, DialogResultType>(
                        new DialogResultType
                        {
                            DialogTitle = "Informatie",
                            DialogText = "Cursist succesvol verwijderd."
                        });

                    LoadStudents();
                }
            });
        }

        private void OpenStudentDetailViewModel()
        {
            if (_navigationService == null)
            {
                throw new InvalidOperationException("Navigation service is not initialized.");
            }

            _navigationService.NavigateTo<StudentDetailViewModel>(SelectedStudent);
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
    }
}