using System.Collections.ObjectModel;
using System.Windows.Input;
using CoursesManager.MVVM.Commands;
using CoursesManager.MVVM.Data;
using CoursesManager.MVVM.Dialogs;
using CoursesManager.MVVM.Messages;
using CoursesManager.UI.Dialogs.ResultTypes;
using CoursesManager.UI.Dialogs.ViewModels;
using CoursesManager.UI.Messages;
using CoursesManager.UI.Models;
using CoursesManager.UI.Models.Repositories.CourseRepository;
using CoursesManager.UI.Models.Repositories.RegistrationRepository;
using CoursesManager.UI.Models.Repositories.StudentRepository;
using CoursesManager.UI.Utils;

namespace CoursesManager.UI.ViewModels
{
    public class StudentManagerViewModel : ViewModel
    {

        private readonly IDialogService _dialogService;
        private readonly IMessageBroker _messageBroker;
        private readonly IStudentRepository _studentRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IRegistrationRepository _registrationRepository;
        public ObservableCollection<Student> Students { get;  set; }
        public ObservableCollection<Student> FilteredStudentRecords { get; set; }
        public ObservableCollection<CourseStudentPayment> DisplayedCourses { get; private set; }

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

        #region Commands

        public ICommand AddStudentCommand { get; }
        public ICommand EditStudentCommand { get; }
        public ICommand DeleteStudentCommand { get; }
        public ICommand SearchCommand { get; }

        #endregion

        public StudentManagerViewModel(
            IDialogService dialogService,
            IStudentRepository studentRepository,
            ICourseRepository courseRepository,
            IRegistrationRepository registrationRepository, IMessageBroker messageBroker)
        {
            _messageBroker = messageBroker;
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _studentRepository = studentRepository ?? throw new ArgumentNullException(nameof(studentRepository));
            _courseRepository = courseRepository ?? throw new ArgumentNullException(nameof(courseRepository));
            _registrationRepository = registrationRepository ?? throw new ArgumentNullException(nameof(registrationRepository));

            // Initialize students
            LoadStudents();

            // Commands
            AddStudentCommand = new RelayCommand(OpenAddStudentPopup);
            EditStudentCommand = new RelayCommand<Student>(OpenEditStudentPopup, s => s != null);
            DeleteStudentCommand = new RelayCommand<Student>(OpenDeleteStudentPopup, s => s != null);
            SearchCommand = new RelayCommand(FilterStudentRecords);
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

        private void UpdateStudentCourses()
        {
            if (SelectedStudent == null)
            {
                DisplayedCourses = new ObservableCollection<CourseStudentPayment>();
                OnPropertyChanged(nameof(DisplayedCourses));
                return;
            }

            var registrations = _registrationRepository.GetAll()
                .Where(r => r.StudentID == SelectedStudent.Id)
                .ToList();

            var coursePayments = registrations.Select(r => new CourseStudentPayment(_courseRepository.GetById(r.CourseID), r)).ToList();

            DisplayedCourses = new ObservableCollection<CourseStudentPayment>(coursePayments);
            OnPropertyChanged(nameof(DisplayedCourses));
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
                    // Refresh the list or perform other actions
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
