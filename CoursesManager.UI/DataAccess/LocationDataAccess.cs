using CoursesManager.UI.Database;
using CoursesManager.UI.Models;
using MySql.Data.MySqlClient;

namespace CoursesManager.UI.DataAccess;

public class LocationDataAccess : BaseDataAccess<Location>
{
    public void Add(Location location)
    {
        try
        {
            ExecuteNonProcedure(StoredProcedures.LocationsInsert, [
                new MySqlParameter("@p_name", location.Name),
                new MySqlParameter("@p_address_id", location.Address.Id)
            ]);
            LogUtil.Log("Location added successfully.");
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException(ex.Message, ex);
        }
    }

    public List<Location> GetAllWithAddresses()
    {
        try
        {
            return ExecuteProcedure(StoredProcedures.LocationsWithAddressesGetAll).Select(row => new Location
            {
                Address = new Address
                {
                    Id = Convert.ToInt32(row["adress_id"]),
                    City = row["city"]?.ToString() ?? string.Empty,
                    Country = row["country"]?.ToString() ?? string.Empty,
                    HouseNumber = row["house_number"]?.ToString() ?? string.Empty,
                    Street = row["street"]?.ToString() ?? string.Empty,
                    Zipcode = row["zipcode"]?.ToString() ?? string.Empty
                },
                Name = row["name"]?.ToString() ?? string.Empty,
                Id = Convert.ToInt32(row["location_id"])
            }).ToList();

        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException(ex.Message, ex);
        }
    }

    public void Update(Location data)
    {
        try
        {
            ExecuteNonProcedure(StoredProcedures.LocationsUpdate, [
                new MySqlParameter("@p_location_id", data.Id),
                new MySqlParameter("@p_new_name", data.Name),
                new MySqlParameter("@p_new_address_id", data.Address.Id)
            ]);
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException(ex.Message, ex);
        }
    }

    public void Delete(int id)
    {
        try
        {
            ExecuteNonProcedure(StoredProcedures.LocationsDeleteById, [
                new MySqlParameter("@p_id", id)
            ]);
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException(ex.Message, ex);
        }
    }
}