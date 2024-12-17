using CoursesManager.MVVM.Data;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace CoursesManager.UI.Models
{
    public class Course : IsObservable, ICopyable<Course>, IDataErrorInfo
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Participants { get; set; }
        public bool IsActive { get; set; }
        public bool IsPayed { get; set; }
        public string Category { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int LocationId { get; set; }
        public List<Registration> Registrations { get; set; }

        private Location? _location;

        public Location? Location
        {
            get => _location;
            set => SetProperty(ref _location, value);
        }
        public DateTime DateCreated { get; set; }
        public ObservableCollection<Student>? Students { get; set; }

        private byte[]? _image;
        public byte[]? Image { get => _image; set => SetProperty(ref _image, value); }

        public BitmapImage? ImageAsBitmap
        {
            get
            {
                if (Image == null || Image.Length == 0)
                    return null;

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
                Participants = this.Participants,
                IsActive = this.IsActive,
                IsPayed = this.IsPayed,
                Category = this.Category,
                StartDate = new DateTime(this.StartDate.Ticks),
                EndDate = new DateTime(this.EndDate.Ticks),
                LocationId = this.LocationId,
                Location = this.Location?.Copy(), 
                DateCreated = new DateTime(this.DateCreated.Ticks),
                Students = this.Students != null ? new ObservableCollection<Student>(this.Students) : null,
                Image = this.Image != null ? (byte[])this.Image.Clone() : null
            };
        }
    }
}
