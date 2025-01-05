using System.Collections.ObjectModel;
using System.Windows.Input;
using CoursesManager.MVVM.Commands;
using CoursesManager.MVVM.Data;
using CoursesManager.MVVM.Dialogs;
using CoursesManager.MVVM.Messages;
using CoursesManager.MVVM.Navigation;
using CoursesManager.UI.Dialogs.ResultTypes;
using CoursesManager.UI.Dialogs.ViewModels;
using CoursesManager.UI.Models;
using CoursesManager.UI.Repositories.CourseRepository;
using CoursesManager.UI.Repositories.RegistrationRepository;
using CoursesManager.UI.Repositories.StudentRepository;

namespace CoursesManager.UI.ViewModels.Students
{
    /// <summary>
    /// ViewModel responsible for managing a list of students.
    /// Supports operations such as searching, toggling between active and deleted students, and navigation.
    /// required to manage the "StudentManagerView".
    /// </summary>
    public class StudentManagerViewModel : ViewModelWithNavigation
    {
        private readonly IDialogService _dialogService;
        private readonly IMessageBroker _messageBroker;
        private readonly IStudentRepository _studentRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IRegistrationRepository _registrationRepository;
        public ObservableCollection<Student> Students { get; set; }
        public ObservableCollection<Student> FilteredStudentRecords { get; set; }
        public ICommand EditStudentCommand { get; }

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

        private ObservableCollection<CourseStudentPayment> _coursePaymentList;

        public ObservableCollection<CourseStudentPayment> CoursePaymentList
        {
            get => _coursePaymentList;
            set => SetProperty(ref _coursePaymentList, value);
        }

        private bool _isToggled;

        public bool IsToggled
        {
            get => _isToggled;
            set
            {
                if (SetProperty(ref _isToggled, value))
                {
                    FilterStudentRecords();
                }
            }
        }

        #region Commands

        public ICommand AddStudentCommand { get; }
        public ICommand DeleteStudentCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand StudentDetailCommand { get; }
        public ICommand CheckboxChangedCommand { get; }
        public ICommand ToggleIsDeletedCommand { get; }


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
            _registrationRepository =
                registrationRepository ?? throw new ArgumentNullException(nameof(registrationRepository));
            _navigationService = navigationService;
            CoursePaymentList = new ObservableCollection<CourseStudentPayment>();

            // Initialize Students
            LoadStudents();
            IsToggled = true;
            // Commands
            AddStudentCommand = new RelayCommand(OpenAddStudentPopup);
            EditStudentCommand = new RelayCommand<Student>(OpenEditStudentPopup, s => s != null);
            DeleteStudentCommand = new RelayCommand<Student>(OpenDeleteStudentPopup, s => s != null);
            SearchCommand = new RelayCommand(FilterStudentRecords);
            StudentDetailCommand = new RelayCommand(OpenStudentDetailViewModel);
            CheckboxChangedCommand = new RelayCommand<CourseStudentPayment>(OnCheckboxChanged);
            ToggleIsDeletedCommand = new RelayCommand(() => FilterStudentRecords());
            ViewTitle = "Cursisten beheer";
        }

        public void LoadStudents()
        {
            Students = new ObservableCollection<Student>(_studentRepository.GetNotDeletedStudents() ??
                                                         new List<Student>());
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

            if (!IsToggled)
            {
                List<Student> filtered = new List<Student>();
                var students = _studentRepository.GetAll();
                foreach (var student in students)
                {
                    if (student.IsDeleted)
                    {
                        filtered.Add(student);
                    }
                }

                FilteredStudentRecords = new ObservableCollection<Student>(filtered);

            }

            OnPropertyChanged(nameof(FilteredStudentRecords));
        }

        private void OnCheckboxChanged(CourseStudentPayment payment)
        {
            if (payment == null || SelectedStudent == null) return;

            var existingRegistration = _registrationRepository.GetAllRegistrationsByStudent(SelectedStudent).FirstOrDefault(r => r.CourseId == payment.Course?.Id);

            if (existingRegistration != null)
            {
                try
                {
                    // Update de velden IsPaid of IsAchieved zodra deze gewijzigd worden.
                    existingRegistration.PaymentStatus = payment.IsPaid;
                    existingRegistration.IsAchieved = payment.IsAchieved;
                    _registrationRepository.Update(existingRegistration);
                }
                catch
                (Exception ex)
                {
                    throw new Exception("No registration found");
                }
            }
            UpdateStudentCourses();
            _messageBroker.Publish(new StudentPaymentUpdatedMessage());
        }


        private void UpdateStudentCourses()
        {
            if (SelectedStudent == null) return;

            var registrations = _registrationRepository.GetAllRegistrationsByStudent(SelectedStudent);

            var payments = registrations
                .Where(registration => registration.Course != null)
                .Select(registration => new CourseStudentPayment(registration.Course, registration))
                .ToList();

            CoursePaymentList = new ObservableCollection<CourseStudentPayment>(payments);
            OnPropertyChanged(nameof(CoursePaymentList));
        }

        private async void OpenAddStudentPopup()
        {
            await ExecuteWithOverlayAsync(_messageBroker, async () =>
            {
                var dialogResult = await _dialogService.ShowDialogAsync<AddStudentViewModel, Student>(new Student());

                if (dialogResult?.Outcome == DialogOutcome.Success)
                {
                    LoadStudents();
                }
            });
        }

        private async void OpenEditStudentPopup(Student student)
        {
            if (student == null)
                await ExecuteWithOverlayAsync(_messageBroker, async () =>
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
            await ExecuteWithOverlayAsync(_messageBroker, async () =>
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

            await ExecuteWithOverlayAsync(_messageBroker, async () =>
            {
                var confirmation = await _dialogService.ShowDialogAsync<ConfirmationDialogViewModel, DialogResultType>(
                    new DialogResultType
                    {
                        DialogTitle = "Bevestiging",
                        DialogText = "Wilt u deze cursist verwijderen?"
                    });

                if (confirmation?.Data?.Result == true)
                {
                    student.IsDeleted = true;
                    student.DeletedAt = DateTime.Now;
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
    }
}