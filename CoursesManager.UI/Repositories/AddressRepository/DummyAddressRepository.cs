using CoursesManager.UI.Models;

namespace CoursesManager.UI.Repositories.AddressRepository;

public class DummyAddressRepository : IAddressRepository
{
    public List<Address> GetAll()
    {
        throw new NotImplementedException();
    }

    public Address GetById(int id)
    {
        throw new NotImplementedException();
    }

    public void Add(Address course)
    {
        throw new NotImplementedException();
    }

    public void Update(Address course)
    {
        throw new NotImplementedException();
    }

    public void Delete(Address data)
    {
        throw new NotImplementedException();
    }

    public void Delete(int id)
    {
        throw new NotImplementedException();
    }
}