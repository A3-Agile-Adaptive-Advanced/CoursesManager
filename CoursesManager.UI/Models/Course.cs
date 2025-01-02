using CoursesManager.MVVM.Data;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Media.Imaging;

namespace CoursesManager.UI.Models
{
    public class Course : IsObservable, ICopyable<Course>, IDataErrorInfo
    {
        private int _id;

        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private string _name = string.Empty;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private string _code = string.Empty;

        public string Code
        {
            get => _code;
            set => SetProperty(ref _code, value);
        }

        private string _description = string.Empty;

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public int Participants => Registrations?.Count ?? 0;

        private bool _isActive;

        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        public bool IsPayed => Registrations?.All(r => r.PaymentStatus) ?? true;

        private string _category = string.Empty;

        public string Category
        {
            get => _category;
            set => SetProperty(ref _category, value);
        }

        private DateTime _startDate;

        public DateTime StartDate
        {
            get => _startDate;
            set => SetProperty(ref _startDate, value);
        }

        private DateTime _endDate;

        public DateTime EndDate
        {
            get => _endDate;
            set => SetProperty(ref _endDate, value);
        }

        private int _locationId;

        public int LocationId
        {
            get => _locationId;
            set => SetProperty(ref _locationId, value);
        }

        private ObservableCollection<Registration>? _registrations;

        public ObservableCollection<Registration>? Registrations
        {
            get => _registrations;
            set
            {
                if (_registrations != null)
                {
                    _registrations.CollectionChanged -= RegistrationsChanged;
                    foreach (var registration in _registrations)
                    {
                        registration.PropertyChanged -= RegistrationPropertyChanged;
                    }
                }

                if (value != null)
                {
                    value.CollectionChanged += RegistrationsChanged;
                    foreach (var registration in value)
                    {
                        registration.PropertyChanged += RegistrationPropertyChanged;
                    }
                }

                SetProperty(ref _registrations, value);
                OnPropertyChanged(nameof(Participants));
                OnPropertyChanged(nameof(IsPayed));
                OnPropertyChanged(nameof(Students));
            }
        }

        private void RegistrationPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Registration.PaymentStatus))
            {
                OnPropertyChanged(nameof(IsPayed));
            }
        }

        private void RegistrationsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (Registration registration in e.NewItems)
                {
                    registration.PropertyChanged += RegistrationPropertyChanged;
                }
            }

            if (e.OldItems != null)
            {
                foreach (Registration registration in e.OldItems)
                {
                    registration.PropertyChanged -= RegistrationPropertyChanged;
                }
            }

            OnPropertyChanged(nameof(Participants));
            OnPropertyChanged(nameof(IsPayed));
            OnPropertyChanged(nameof(Students));
        }

        private Location? _location;

        public Location? Location
        {
            get => _location;
            set => SetProperty(ref _location, value);
        }

        private DateTime _dateCreated;

        public DateTime DateCreated
        {
            get => _dateCreated;
            set => SetProperty(ref _dateCreated, value);
        }

        public ObservableCollection<Student>? Students
        {
            get
            {
                if (Registrations is null) return null;

                var res = new ObservableCollection<Student>();

                foreach (var registration in Registrations)
                {
                    if (registration.Student is not null)
                    {
                        res.Add(registration.Student);
                    }
                }

                return res;
            }
        }

        private byte[]? _image;

        public byte[]? Image
        {
            get => _image;
            set => SetProperty(ref _image, value);
        }

        public BitmapImage? ImageAsBitmap
        {
            get
            {
                if (Image == null || Image.Length == 0)
                    return new BitmapImage();

                using (var stream = new MemoryStream(Image))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                    return bitmap;
                }
            }
        }

        #region Validation

        // Validation logic for IDataErrorInfo
        public string Error => null;

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(Name):
                        if (string.IsNullOrWhiteSpace(Name))
                            return "Cursusnaam is verplicht.";
                        break;

                    case nameof(Code):
                        if (string.IsNullOrWhiteSpace(Code))
                            return "Cursuscode is verplicht.";
                        break;

                    case nameof(Description):
                        if (string.IsNullOrWhiteSpace(Description))
                            return "Beschrijving is verplicht.";
                        break;

                    case nameof(StartDate):
                        if (StartDate == default)
                            return "Begindatum is verplicht.";
                        if (StartDate > EndDate)
                            return "Begindatum moet vóór de einddatum liggen.";
                        break;

                    case nameof(EndDate):
                        if (EndDate == default)
                            return "Einddatum is verplicht.";
                        if (EndDate < StartDate)
                            return "Einddatum moet ná de begindatum liggen.";
                        break;

                    case nameof(Location):
                        if (Location == null)
                            return "Locatie is verplicht.";
                        break;

                    case nameof(Image):
                        if (Image != null && Image.Length > 5_000_000) // 5 MB limiet
                            return "Afbeelding mag niet groter zijn dan 5 MB.";
                        break;
                }
                return null;
            }
        }

        #endregion Validation

        #region Methods

        public string GenerateFilterString()
        {
            var sb = new StringBuilder();

            sb.Append(Name);
            sb.Append(Code);
            sb.Append(Category);
            sb.Append(StartDate.ToString("yyyy-MM-dd"));

            if (Location != null)
            {
                sb.Append(Location.Name);
                sb.Append(Location.Address);
            }

            return sb.ToString().Replace(" ", string.Empty);
        }

        public Course Copy()
        {
            return new Course
            {
                Id = this.Id,
                Name = this.Name,
                Code = this.Code,
                Description = this.Description,
                IsActive = this.IsActive,
                Category = this.Category,
                StartDate = new DateTime(this.StartDate.Ticks),
                EndDate = new DateTime(this.EndDate.Ticks),
                LocationId = this.LocationId,
                Location = this.Location?.Copy(),
                DateCreated = new DateTime(this.DateCreated.Ticks),
                Image = this.Image != null ? (byte[])this.Image.Clone() : null
            };
        }

        public string ReplaceCoursePlaceholders(string template, Course course)
        {
            var placeholders = new Dictionary<string, string>
            {
                { "[Cursus naam]", course.Name },
                { "[Cursus code]", course.Code },
                { "[Cursus beschrijving]", course.Description },
                { "[Cursus categorie]", course.Category },
                { "[Cursus startdatum]", course.StartDate.ToString("dd-MM-yyyy") },
                { "[Cursus einddatum]", course.EndDate.ToString("dd-MM-yyyy") },
                { "[Cursus locatie naam]", course.Location?.Name  ?? "" },
                { "[Cursus locatie land]", course.Location?.Address?.Country  ?? "" },
                { "[Cursus locatie postcode]", course.Location?.Address?.ZipCode  ?? "" },
                { "[Cursus locatie stad]", course.Location?.Address?.City  ?? "" },
                { "[Cursus locatie straat]", course.Location?.Address?.Street  ?? "" },
                { "[Cursus locatie huisnummer]", course.Location?.Address?.HouseNumber  ?? "" },
                { "[Cursus locatie toevoeging]", course.Location?.Address?.HouseNumberExtension  ?? "" },
                { "[Betaal link]", "Betaal link"}
            };

            foreach (var placeholder in placeholders)
            {
                template = template.Replace(placeholder.Key, placeholder.Value ?? string.Empty);
            }

            return template;
        }

        #endregion Methods
    }
}