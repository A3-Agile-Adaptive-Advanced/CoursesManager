using CoursesManager.UI.Database;
using CoursesManager.UI.Models;
using MySql.Data.MySqlClient;
using System.Data;

namespace CoursesManager.UI.DataAccess;

public class LocationDataAccess : BaseDataAccess<Location>
{
    public void Add(Location location)
    {
        try
        {
            var outputParameter = new MySqlParameter("@p_id", MySqlDbType.Int32)
            {
                Direction = ParameterDirection.Output
            };

            ExecuteNonProcedure(StoredProcedures.AddLocation, [
                new MySqlParameter("@p_name", location.Name),
                new MySqlParameter("@p_address_id", location.Address.Id),
                outputParameter
            ]);

            location.Id = Convert.ToInt32(outputParameter.Value);

            LogUtil.Log("Location added successfully.");
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException(ex.Message, ex);
        }
    }

    public List<Location> GetAll()
    {
        try
        {
            return ExecuteProcedure(StoredProcedures.GetAllLocationsWithAddresses).Select(MapToLocation).ToList();

        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException(ex.Message, ex);
        }
    }

    private static Location MapToLocation(Dictionary<string, object?> row)
    {
        return new Location
        {
            Name = row["name"]?.ToString() ?? string.Empty,
            Id = Convert.ToInt32(row["location_id"])
        };
    }

    public void Update(Location data)
    {
        try
        {
            ExecuteNonProcedure(StoredProcedures.UpdateLocation, [
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
            ExecuteNonProcedure(StoredProcedures.DeleteLocation, [
                new MySqlParameter("@p_id", id)
            ]);
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException(ex.Message, ex);
        }
    }

    public Location? GetById(int id)
    {
        try
        {
            var res = ExecuteProcedure(StoredProcedures.GetLocationWithAddressesById, [
                new MySqlParameter("@p_id", id)
            ]);

            return !res.Any() ? null : MapToLocation(res.First());
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException(ex.Message, ex);
        }
    }
}