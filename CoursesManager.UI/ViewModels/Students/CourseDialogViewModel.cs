using CoursesManager.MVVM.Commands;
using CoursesManager.MVVM.Dialogs;
using CoursesManager.UI.Models;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using CoursesManager.UI.Dialogs.ResultTypes;
using CoursesManager.UI.Dialogs.ViewModels;
using CoursesManager.UI.Repositories.CourseRepository;
using CoursesManager.UI.Repositories.LocationRepository;
using CoursesManager.UI.Messages;

namespace CoursesManager.UI.ViewModels.Students
{
    class CourseDialogViewModel : DialogViewModel<Course>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IDialogService _dialogService;
        private readonly ILocationRepository _locationRepository;
        private BitmapImage? _imageSource;
        private Course? _course;

        private bool _isDialogOpen;
        public bool IsDialogOpen
        {
            get => _isDialogOpen;
            set => SetProperty(ref _isDialogOpen, value);
        }

        public BitmapImage? ImageSource
        {
            get => _imageSource;
            set => SetProperty(ref _imageSource, value);
        }

        public Course? Course
        {
            get => _course;
            set => SetProperty(ref _course, value);
        }

        public ICommand SaveCommand { get; }

        public ICommand CancelCommand { get; }

        public ICommand UploadCommand { get; }
        public ObservableCollection<string> Courses
        {
            get; set;
        }

        public ObservableCollection<Location> Locations { get; set; }

        public CourseDialogViewModel(ICourseRepository courseRepository, IDialogService dialogService, ILocationRepository locationRepository, Course? course) : base(course)
        {
            _courseRepository = courseRepository ?? throw new ArgumentNullException(nameof(courseRepository));
            _locationRepository = locationRepository ?? throw new ArgumentNullException(nameof(locationRepository));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

            Course = course != null
                   ? course.Copy()
                    : new Course
                    {
                        Name = string.Empty,
                        Code = string.Empty,
                        Description = string.Empty,
                        Location = null,
                        IsActive = false,
                        StartDate = DateTime.Now,
                        EndDate = DateTime.Now,
                        Image = null,

                    };

            OriginalCourse = course;
            Locations = new ObservableCollection<Location>(locationRepository.GetAll());

            Courses = new ObservableCollection<string>(_courseRepository.GetAll().Select(c => c.Name));
            SaveCommand = new RelayCommand(ExecuteSave, CanExecuteSave);
            CancelCommand = new RelayCommand(OnCancel);
            UploadCommand = new RelayCommand(UploadImage);
        }

        private bool CanExecuteSave()
        {
            return !string.IsNullOrWhiteSpace(Course!.Name)
                       && !string.IsNullOrWhiteSpace(Course.Code)
                       && Course.StartDate != default
                       && Course.EndDate != default
                       && Course.Location != null
                       && !string.IsNullOrWhiteSpace(Course.Description);
        }


        private Course? OriginalCourse { get; }
        protected override void InvokeResponseCallback(DialogResult<Course> dialogResult)
        {
            ResponseCallback.Invoke(dialogResult);
        }

        private async void ExecuteSave()
        {
            try
            {
                await OnSaveAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private async Task OnSaveAsync()
        {
            try
            {
                if (OriginalCourse == null)
                {
                    _courseRepository.Add(Course!);
                }
                else
                {
                    _courseRepository.Update(Course!);
                }

                var successDialogResult = DialogResult<Course>.Builder()
                    .SetSuccess(Course!, OriginalCourse == null ? "Cursus succesvol toegevoegd" : "Cursus succesvol bijgewerkt")
                    .Build();


                throw new Exception(
                    "Hey bas, misschien handig dit ook toe te passen als je dingen met de repo doet, was ik ook nog vergeten. Als er iets mis gaat met de communicatie met de database gaat het stuk.");

                InvokeResponseCallback(successDialogResult);
            }
            catch (Exception ex)
            {
                LogUtil.Error(ex.Message);
                await _dialogService.ShowDialogAsync<ErrorDialogViewModel, DialogResultType>(new DialogResultType
                {
                    DialogText = "Er is iets fout gegaan.",
                    DialogTitle = "Error"
                });
            }
        }

        public void OnCancel()
        {
            var dialogResult = DialogResult<Course>.Builder()
                .SetCanceled("Wijzigingen geannuleerd door de gebruiker.")
                .Build();

            InvokeResponseCallback(dialogResult);
        }

        private void UploadImage()
        {
            var openDialog = new OpenFileDialog
            {
                Filter = "Image Files|*.bmp;*.jpg;*.png",
                FilterIndex = 1
            };

            if (openDialog.ShowDialog() == true)
            {
                Course!.Image = new BitmapImage(new Uri(openDialog.FileName));
            }
        }
    }
}
