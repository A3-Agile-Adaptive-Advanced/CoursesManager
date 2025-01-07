using System.Collections.ObjectModel;
using CoursesManager.UI.DataAccess;
using CoursesManager.UI.Models;
using CoursesManager.UI.Repositories.Base;

namespace CoursesManager.UI.Repositories.AddressRepository;

public class AddressRepository : BaseRepository<Address>, IAddressRepository
{
    private readonly AddressDataAccess _addressDataAccess;

    private readonly ObservableCollection<Address> _addresses;

    private const string Cachekey = "addressesCache";

    private static readonly object SharedLock = new();

    public AddressRepository(AddressDataAccess addressDataAccess)
    {
        _addressDataAccess = addressDataAccess;

        try
        {
            _addresses = GlobalCache.Instance.Get(Cachekey) as ObservableCollection<Address> ?? SetupCache(Cachekey);
        }
        catch
        {
            _addresses = SetupCache(Cachekey);
        }
        finally
        {
            GetAll();
        }
    }

    public ObservableCollection<Address> GetAll()
    {
        lock (SharedLock)
        {
            if (_addresses.Count == 0)
            {
                _addressDataAccess.GetAll().ForEach(_addresses.Add);
            }

            return new ObservableCollection<Address>(_addresses);
        }
    }

    public Address? GetById(int id)
    {
        lock (SharedLock)
        {
            var item = _addresses.FirstOrDefault(a => a.Id == id);

            if (item is null)
            {
                item = _addressDataAccess.GetById(id);

                if (item is not null) _addresses.Add(item);
            }

            return item;
        }
    }

    public void Add(Address address)
    {
        lock (SharedLock)
        {
            _addressDataAccess.Add(address);
            _addresses.Add(address);
        }
    }

    public void Update(Address address)
    {
        lock (SharedLock)
        {
            _addressDataAccess.Update(address);

            var item = GetById(address.Id) ?? throw new InvalidOperationException($"Address with id: {address.Id} does not exist.");

            OverwriteItemInPlace(item, address);
        }
    }

    public void Delete(Address address)
    {
        Delete(address.Id);
    }

    public void Delete(int id)
    {
        lock (SharedLock)
        {
            _addressDataAccess.Delete(id);

            var item = GetById(id) ?? throw new InvalidOperationException($"Address with id: {id} does not exist.");

            _addresses.Remove(item);
        }
    }

}