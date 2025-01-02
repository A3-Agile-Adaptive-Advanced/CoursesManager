using CoursesManager.MVVM.Data;

namespace CoursesManager.UI.Models
{
    public class Address : IsObservable
    {
        private int _id;

        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private string _country = string.Empty;

        public string Country
        {
            get => _country;
            set => SetProperty(ref _country, value);
        }

        private string _zipCode = string.Empty;

        public string ZipCode
        {
            get => _zipCode;
            set => SetProperty(ref _zipCode, value);
        }

        private string _city = string.Empty;

        public string City
        {
            get => _city;
            set => SetProperty(ref _city, value);
        }

        private string _street = string.Empty;

        public string Street
        {
            get => _street;
            set => SetProperty(ref _street, value);
        }

        private string _houseNumber = string.Empty;

        public string HouseNumber
        {
            get => _houseNumber;
            set => SetProperty(ref _houseNumber, value);
        }

        private string? _houseNumberExtension;

        public string? HouseNumberExtension
        {
            get => _houseNumberExtension;
            set => SetProperty(ref _houseNumberExtension, value);
        }

        private DateTime _updatedAt;

        public DateTime UpdatedAt
        {
            get => _updatedAt;
            set => SetProperty(ref _updatedAt, value);
        }

        private DateTime _createdAt;

        public DateTime CreatedAt
        {
            get => _createdAt;
            set => SetProperty(ref _createdAt, value);
        }
    }
}