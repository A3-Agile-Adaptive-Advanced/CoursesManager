using CoursesManager.UI.DataAccess;
using CoursesManager.UI.Models;

namespace CoursesManager.UI.Repositories.LocationRepository;

public class LocationRepository : BaseRepository, ILocationRepository
{
    private readonly LocationDataAccess _locationDataAccess;

    private readonly List<Location> _allLocations;

    public LocationRepository()
    {
        _locationDataAccess = new LocationDataAccess();
        _allLocations = new();
    }

    public List<Location> GetAll()
    {
        if (_allLocations.Count == 0 || ShouldRefresh)
        {
            return RefreshAll();
        }

        return _allLocations;
    }

    public List<Location> RefreshAll()
    {
        lock (_allLocations)
        {
            _lastUpdated = DateTime.Now;

            _allLocations.Clear();

            _allLocations.AddRange(_locationDataAccess.GetAllWithAddresses());

            return _allLocations;
        }
    }

    public Location? GetById(int id)
    {
        return _allLocations.First(l => l.Id == id);
    }

    public void Add(Location data)
    {
        lock (_allLocations)
        {
            ArgumentNullException.ThrowIfNull(data);

            _locationDataAccess.Add(data);
            _allLocations.Add(data);
        }
    }

    public void Update(Location data)
    {
        lock (_allLocations)
        {
            ArgumentNullException.ThrowIfNull(data);

            _locationDataAccess.Update(data);

            if (!_allLocations.Contains(data))
            {
                var existing = _allLocations.FirstOrDefault(l => l.Id == data.Id);
                if (existing is not null)
                {
                    existing.Id = data.Id;
                    existing.Name = data.Name;
                    existing.Address = data.Address;
                }
                else
                {
                    _allLocations.Add(data);
                }
            }
        }
    }

    public void Delete(Location data)
    {
        ArgumentNullException.ThrowIfNull(data);

        Delete(data.Id);
    }

    public void Delete(int id)
    {
        lock (_allLocations)
        {
            _locationDataAccess.Delete(id);

            _allLocations.RemoveAll(loc => loc.Id == id);
        }
    }
}