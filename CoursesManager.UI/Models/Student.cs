using CoursesManager.MVVM.Data;
using System;
using System.Collections.ObjectModel;

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

        private int? _addressId;
        public int? AddressId
        {
            get => _addressId;
            set => SetProperty(ref _addressId, value);
        }

        private ObservableCollection<Course>? _courses;
        public ObservableCollection<Course>? Courses
        {
            get => _courses;
            set => SetProperty(ref _courses, value);
        }

        private ObservableCollection<Registration>? _registrations;
        public ObservableCollection<Registration>? Registrations
        {
            get => _registrations;
            set => SetProperty(ref _registrations, value);
        }

        private DateTime _dateOfBirth;
        public DateTime DateOfBirth
        {
            get => _dateOfBirth;
            set => SetProperty(ref _dateOfBirth, value);
        }

        private string? _insertion;
        public string? Insertion
        {
            get => _insertion;
            set => SetProperty(ref _insertion, value);
        }

        public string TableFilter()
        {
            return $"{FirstName}{LastName}{Email}".Replace(" ", "");
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
                Courses = this.Courses,
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
    }
}
