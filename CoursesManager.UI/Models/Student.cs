using CoursesManager.MVVM.Data;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;

namespace CoursesManager.UI.Models
{
    public class Student : ViewModel, ICopyable<Student>
    {
        public int Id { get; set; }

        private string _firstName;

        public string FirstName
        {
            get => _firstName;
            set => SetProperty(ref _firstName, value);
        }

        private string _lastName;

        public string LastName
        {
            get => _lastName;
            set => SetProperty(ref _lastName, value);
        }

        private string _email;

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        private string _phone;

        public string Phone
        {
            get => _phone;
            set => SetProperty(ref _phone, value);
        }

        private bool _isDeleted;

        public bool IsDeleted
        {
            get => _isDeleted;
            set => SetProperty(ref _isDeleted, value);
        }

        private DateTime? _deletedAt;

        public DateTime? DeletedAt
        {
            get => _deletedAt;
            set => SetProperty(ref _deletedAt, value);
        }

        private DateTime _createdAt;

        public DateTime CreatedAt
        {
            get => _createdAt;
            set => SetProperty(ref _createdAt, value);
        }

        private DateTime _updatedAt;

        public DateTime UpdatedAt
        {
            get => _updatedAt;
            set => SetProperty(ref _updatedAt, value);
        }

        private int _addressId;

        public int AddressId
        {
            get => _addressId;
            set => SetProperty(ref _addressId, value);
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
                OnPropertyChanged(nameof(Courses));
            }
        }

        public ObservableCollection<Course>? Courses
        {
            get
            {
                if (Registrations is null) return null;

                var res = new ObservableCollection<Course>();

                foreach (var registration in Registrations)
                {
                    if (registration.Course is not null)
                    {
                        res.Add(registration.Course);
                    }
                }

                return res;
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

            OnPropertyChanged(nameof(Courses));
        }

        private void RegistrationPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Registration.Course))
            {
                OnPropertyChanged(nameof(Courses));
            }
        }

        private DateTime _dateOfBirth;

        public DateTime DateOfBirth
        {
            get => _dateOfBirth;
            set => SetProperty(ref _dateOfBirth, value.Date);
        }

        private string? _insertion;

        public string? Insertion
        {
            get => _insertion;
            set => SetProperty(ref _insertion, value);
        }

        private Address? _address;

        public Address? Address
        {
            get => _address;
            set => SetProperty(ref _address, value);
        }

        public Student Copy()
        {
            return new Student
            {
                Id = this.Id,
                FirstName = this.FirstName,
                Insertion = this.Insertion,
                LastName = this.LastName,
                Email = this.Email,
                Phone = this.Phone,
                IsDeleted = this.IsDeleted,
                AddressId = this.AddressId,
                Registrations = this.Registrations,
                DateOfBirth = this.DateOfBirth,
                Address = this.Address != null ? new Address
                {
                    Id = this.Address.Id,
                    Country = this.Address.Country,
                    ZipCode = this.Address.ZipCode,
                    City = this.Address.City,
                    Street = this.Address.Street,
                    HouseNumber = this.Address.HouseNumber
                } : null
            };
        }

        public string ReplaceStudentPlaceholders(string template, Student student)
        {
            var fullName = $"{student.FirstName} {(string.IsNullOrWhiteSpace(student.Insertion) ? "" : student.Insertion + " ")}{student.LastName}".Trim();

            var placeholders = new Dictionary<string, string>
            {
                { "[Cursist naam]", fullName },
                { "[Cursist email]", student.Email },
                { "[Cursist telefoonnummer]", student.Phone},
                { "[Cursist geboortedatum]", student.DateOfBirth.ToString("dd-MM-yyyy") },
                { "[Cursist adres land]", student.Address?.Country ?? ""},
                { "[Cursist adres postcode]", student.Address?.ZipCode ?? ""},
                { "[Cursist adres stad]", student.Address?.City ?? ""},
                { "[Cursist adres straat]", student.Address?.Street ?? ""},
                { "[Cursist adres huisnummer]", student.Address?.HouseNumber ?? ""},
                { "[Cursist adres toevoeging]", student.Address?.HouseNumberExtension ?? ""}
            };

            foreach (var placeholder in placeholders)
            {
                template = template.Replace(placeholder.Key, placeholder.Value ?? string.Empty);
            }

            return template;
        }

        public string GenerateFilterString()
        {
            var sb = new StringBuilder();

            sb.Append(FirstName);
            sb.Append(Insertion ?? "");
            sb.Append(LastName);
            sb.Append(Email);

            return sb.ToString().Replace(" ", "");
        }
    }
}