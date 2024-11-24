using CoursesManager.UI.Models;

namespace CoursesManager.UI.Repositories.LocationRepository
{
    public class DummyLocationRepository : ILocationRepository
    {
        private readonly List<Location> _locations = new List<Location>();

        public List<Location> GetAll()
        {
            return _locations.ToList();
        }

        public Location GetById(int id)
        {
            return _locations.FirstOrDefault(l => l.Id == id);
        }

        public void Add(Location location)
        {
            location.DateCreated = DateTime.Now;
            _locations.Add(location);
        }

        public void Update(Location location)
        {
            var existingLocation = GetById(location.Id);
            if (existingLocation != null)
            {
                existingLocation.Name = location.Name;
                existingLocation.Address = location.Address;
                existingLocation.Capacity = location.Capacity;
            }
        }

        public void Delete(Location data)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            var location = GetById(id);
            if (location != null)
            {
                _locations.Remove(location);
            }
        }

        public void AddIfNew(Location courseLocation)
        {
            throw new NotImplementedException();
        }
    }
}