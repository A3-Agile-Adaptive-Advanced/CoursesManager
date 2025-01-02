using CoursesManager.MVVM.Data;

namespace CoursesManager.UI.Models
{
    public class Location : ICopyable<Location>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Address? Address { get; set; }
        public int AddressId { get; set; }
        public Location Copy()
        {
            return new Location
            {
                Id = Id,
                Name = Name,
                Address = Address
            };
        }
    }
}
