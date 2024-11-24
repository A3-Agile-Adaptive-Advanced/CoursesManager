using CoursesManager.UI.Models;

namespace CoursesManager.UI.Repositories.LocationRepository;

public interface ILocationRepository : IRepository<Location>
{
    void AddIfNew(Location courseLocation);
}