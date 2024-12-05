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
using CoursesManager.UI.Repositories;
using System.IO;

namespace CoursesManager.UI.ViewModels.Courses
{
    public class CourseDialogViewModel : DialogViewModel<Course>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IDialogService _dialogService;
        private readonly ILocationRepository _locationRepository;


        

        private bool _isDialogOpen;
        public bool IsDialogOpen
        {
            get => _isDialogOpen;
            set => SetProperty(ref _isDialogOpen, value);
        }

        private BitmapImage? _imageSource;
        public BitmapImage? ImageSource
        {
            get => _imageSource;
            set => SetProperty(ref _imageSource, value);
        }

        private Course? _course;
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

            IsStartAnimationTriggered = true;

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
            var allLocations = _locationRepository.GetAll();
            Locations = new ObservableCollection<Location>(allLocations);

            // Stel de geselecteerde locatie in
            SelectedLocation = Locations.FirstOrDefault(l => l.Id == Course.LocationId);

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
                if (Course == null)
                {
                    throw new InvalidOperationException("Cursusgegevens ontbreken. Opslaan is niet mogelijk.");
                }

                LogUtil.Log($"Attempting to save course: {Course.Name}, {Course.Code}, {Course.LocationId}");

                if (OriginalCourse == null)
                {
                    _courseRepository.Add(Course);
                    LogUtil.Log("Course added successfully via repository.");
                }
                else
                {
                    _courseRepository.Update(Course);
                    LogUtil.Log("Course updated successfully via repository.");
                }



                var successDialogResult = DialogResult<Course>.Builder()
                    .SetSuccess(
                        Course,
                        OriginalCourse == null ? "Cursus succesvol toegevoegd." : "Cursus succesvol bijgewerkt."
                    )
                    .Build();

                await TriggerEndAnimationAsync();

                InvokeResponseCallback(successDialogResult);
            }
            catch (Exception ex)
            {
                LogUtil.Error($"Error in OnSaveAsync: {ex.Message}");

                await _dialogService.ShowDialogAsync<ErrorDialogViewModel, DialogResultType>(new DialogResultType
                {
                    DialogText = "Er is iets fout gegaan. Probeer het later opnieuw.",
                    DialogTitle = "Fout"
                });
            }
        }



        public async void OnCancel()
        {
            var dialogResult = DialogResult<Course>.Builder()
                .SetCanceled("Wijzigingen geannuleerd door de gebruiker.")
                .Build();
            
            await TriggerEndAnimationAsync();

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
                
                var bitmap = new BitmapImage(new Uri(openDialog.FileName));

              
                Course.Image = ConvertImageToByteArray(bitmap);

                
                ImageSource = bitmap;
            }
        }



        public static byte[] ConvertImageToByteArray(BitmapImage image)
        {
            if (image == null) throw new ArgumentNullException(nameof(image));

            using (var memoryStream = new MemoryStream())
            {
                var encoder = new JpegBitmapEncoder
                {
                    QualityLevel = 90 
                };

                encoder.Frames.Add(BitmapFrame.Create(image));
                encoder.Save(memoryStream);

                return memoryStream.ToArray();
            }
        }

        private object selectedLocation;

        public object SelectedLocation { get => selectedLocation; set => SetProperty(ref selectedLocation, value); }

    }
}
