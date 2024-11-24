using CoursesManager.MVVM.Env;
using CoursesManager.UI.DataAccess;
using CoursesManager.UI.Models;

namespace CoursesManager.UI.Repositories.AddressRepository;

public class AddressRepository : IAddressRepository
{
    private readonly AddressDataAccess _addressDataAccess = new();

    public List<Address> GetAll()
    {
        return _addressDataAccess.FetchAll();
    }

    public Address? GetById(int id)
    {
        return _addressDataAccess.FetchOneById(id);
    }

    public void Add(Address address)
    {
        _addressDataAccess.Add(address);
    }

    public void Update(Address address)
    {
        _addressDataAccess.Update(address);
    }

    public void Delete(Address address)
    {
        _addressDataAccess.Delete(address.Id);
    }

    public void Delete(int id)
    {
        _addressDataAccess.Delete(id);
    }
}