using CoursesManager.MVVM.Commands;
using CoursesManager.MVVM.Dialogs;
using CoursesManager.MVVM.Messages;
using CoursesManager.UI.Dialogs.ResultTypes;
using CoursesManager.UI.Dialogs.ViewModels;
using CoursesManager.UI.Enums;
using CoursesManager.UI.Models;
using CoursesManager.UI.Repositories.CourseRepository;
using CoursesManager.UI.Repositories.LocationRepository;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace CoursesManager.UI.ViewModels.Courses
{
    public class CourseDialogViewModel : DialogViewModel<Course>
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IDialogService _dialogService;
        private readonly ILocationRepository _locationRepository;
        private readonly IMessageBroker _messageBroker;

        private Course? OriginalCourse { get; }

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


        public ObservableCollection<Location> Locations { get; set; }

        public CourseDialogViewModel(ICourseRepository courseRepository, IDialogService dialogService,
            ILocationRepository locationRepository, IMessageBroker messageBroker, Course? course) : base(course)
        {
            _courseRepository = courseRepository ?? throw new ArgumentNullException(nameof(courseRepository));
            _locationRepository = locationRepository ?? throw new ArgumentNullException(nameof(locationRepository));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _messageBroker = messageBroker ?? throw new ArgumentNullException(nameof(messageBroker));

            IsStartAnimationTriggered = true;

            OriginalCourse = course;

            Locations = GetLocations();

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

            Course.Location = Locations.FirstOrDefault(l => l.Id == Course.LocationId);

            SaveCommand = new RelayCommand(ExecuteSave);
            CancelCommand = new RelayCommand(ExecuteCancel);
            UploadCommand = new RelayCommand(UploadImage);
        }

        private ObservableCollection<Location> GetLocations()
        {
            return new ObservableCollection<Location>(_locationRepository.GetAll());
        }

        private List<string> GetMissingFields()
        {
            var missingFields = new List<string>();

            if (Course == null) return missingFields;

            if (string.IsNullOrWhiteSpace(Course.Name)) missingFields.Add("Naam");
            if (string.IsNullOrWhiteSpace(Course.Code)) missingFields.Add("Code");
            if (Course.StartDate == default) missingFields.Add("Startdatum");
            if (Course.EndDate == default) missingFields.Add("Einddatum");
            if (Course.Location == null) missingFields.Add("Locatie");
            if (string.IsNullOrWhiteSpace(Course.Description)) missingFields.Add("Beschrijving");

            return missingFields;
        }

        private bool CanExecuteSave() =>
            Course is not null &&
            !string.IsNullOrWhiteSpace(Course.Name) &&
            !string.IsNullOrWhiteSpace(Course.Code) &&
            Course.StartDate != default &&
            Course.EndDate != default &&
            Course.Location is not null &&
            !string.IsNullOrWhiteSpace(Course.Description);
    


    protected override void InvokeResponseCallback(DialogResult<Course> dialogResult)
        {
            ResponseCallback?.Invoke(dialogResult);
        }

        private void ExecuteSave()
        {
            if (Course == null)
            {
                throw new InvalidOperationException("Cursusgegevens ontbreken. Opslaan is niet mogelijk.");
            }

            var missingFields = GetMissingFields();
            if (missingFields.Any())
            {
                var message = "De volgende velden ontbreken: " + string.Join(", ", missingFields);
                _messageBroker.Publish(new ToastNotificationMessage(
                    true,
                    message,
                    ToastType.Warning));
                return;
            }

            _ = OnSaveAsync();
        }


        private async Task OnSaveAsync()
        {
            try
            {
                if (Course == null)
                {
                    throw new InvalidOperationException("Course mag niet null zijn bij het uploaden van een afbeelding.");
                }

                if (Course.Location != null) Course.LocationId = Course.Location.Id;

                if (OriginalCourse == null)
                {
                    _courseRepository.Add(Course);
                }
                else
                {
                    _courseRepository.Update(Course);
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



        public async Task OnCancel()
        {
            var dialogResult = DialogResult<Course>.Builder()
                .SetCanceled("Wijzigingen geannuleerd door de gebruiker.")
                .Build();

            await TriggerEndAnimationAsync();

            InvokeResponseCallback(dialogResult);
        }

        private void ExecuteCancel() => _ = OnCancel();

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

              
                Course!.Image = ConvertImageToByteArray(bitmap);


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

    }
}