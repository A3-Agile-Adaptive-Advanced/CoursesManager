using System.Collections.ObjectModel;
using CoursesManager.UI.DataAccess;
using CoursesManager.UI.Models;
using CoursesManager.UI.Repositories.AddressRepository;
using CoursesManager.UI.Repositories.Base;

namespace CoursesManager.UI.Repositories.LocationRepository;

public class LocationRepository : BaseRepository<Location>, ILocationRepository
{
    private readonly LocationDataAccess _locationDataAccess;

    private readonly ObservableCollection<Location> _locations;

    private const string Cachekey = "locationsCache";

    private static readonly object SharedLock = new();

    private readonly IAddressRepository _addressRepository;

    public LocationRepository(LocationDataAccess locationDataAccess, IAddressRepository addressRepository)
    {
        _locationDataAccess = locationDataAccess;
        _addressRepository = addressRepository;

        try
        {
            _locations = GlobalCache.Instance.Get(Cachekey) as ObservableCollection<Location> ?? SetupCache(Cachekey);
        }
        catch
        {
            _locations = SetupCache(Cachekey);
        }
        finally
        {
            GetAll();
        }
    }

    private Location JoinWithAddress(Location location)
    {
        location.Address = _addressRepository.GetById(location.AddressId);
        return location;
    }

    public ObservableCollection<Location> GetAll()
    {
        lock (SharedLock)
        {
            if (_locations.Count == 0)
            {
                _locationDataAccess.GetAll().ForEach(l => _locations.Add(JoinWithAddress(l)));
            }

            return _locations;
        }
    }

    public Location? GetById(int id)
    {
        lock (SharedLock)
        {
            var item = _locations.FirstOrDefault(l => l.Id == id);

            if (item is null)
            {
                item = _locationDataAccess.GetById(id);

                if (item is not null) _locations.Add(JoinWithAddress(item));
            }

            return item;
        }
    }

    public void Add(Location location)
    {
        lock (SharedLock)
        {
            ArgumentNullException.ThrowIfNull(location);

            _locationDataAccess.Add(location);
            _locations.Add(location);
        }
    }

    public void Update(Location location)
    {
        lock (SharedLock)
        {
            ArgumentNullException.ThrowIfNull(location);

            _locationDataAccess.Update(location);

            var item = _locations.FirstOrDefault(l => l.Id == location.Id) ?? throw new InvalidOperationException($"Location with id: {location.Id} does not exist.");

            OverwriteItemInPlace(item, location);
        }
    }

    public void Delete(Location location)
    {
        ArgumentNullException.ThrowIfNull(location);

        Delete(location.Id);
    }

    public void Delete(int id)
    {
        lock (SharedLock)
        {
            _locationDataAccess.Delete(id);

            var item = _locations.FirstOrDefault(l => l.Id == id) ?? throw new InvalidOperationException($"Location with id: {id} does not exist.");

            _locations.Remove(item);
        }
    }
}