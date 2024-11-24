using CoursesManager.UI.Models;
using MySql.Data.MySqlClient;

namespace CoursesManager.UI.DataAccess;

public class AddressDataAccess : BaseDataAccess<Address>
{
    public void Add(Address address)
    {
        ArgumentNullException.ThrowIfNull(address);

        InsertRow(
            new Dictionary<string, object>
            {
                {"country", address.Country},
                {"zipcode", address.Zipcode},
                {"city", address.City},
                {"Street", address.Street},
                {"house_number", address.HouseNumber}
            }
        );
    }

    public void Update(Address address)
    {
        ArgumentNullException.ThrowIfNull(address);

        UpdateRow(
            new Dictionary<string, object>
            {
                {"country", address.Country},
                {"zipcode", address.Zipcode},
                {"city", address.City},
                {"Street", address.Street},
                {"house_number", address.HouseNumber}
            },
            "ID = @ID",
            [new MySqlParameter("@ID", address.Id)]
        );
    }

    public void Delete(int id)
    {
        DeleteRow("ID = @ID", [new MySqlParameter("@ID", id)]);
    }
}