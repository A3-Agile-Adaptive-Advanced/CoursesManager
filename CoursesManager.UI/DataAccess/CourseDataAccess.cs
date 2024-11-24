using CoursesManager.UI.Models;
using MySql.Data.MySqlClient;

namespace CoursesManager.UI.DataAccess;

public class CourseDataAccess : BaseDataAccess<Course>
{
    public List<Course> GetAll()
    {
        return FetchAll(
            "SELECT c.*, COUNT(r.id) AS participants, " +
            "CASE WHEN COUNT(r.id) = COUNT(CASE WHEN r.payment_status = 1 THEN 1 END) " +
            "THEN 'TRUE' ELSE 'FALSE' END AS isPayed " +
            "FROM courses c " +
            "LEFT JOIN registrations r ON r.course_id = c.id;"
        );
    }

    public void Add(Course course)
    {
        ArgumentNullException.ThrowIfNull(course);

        InsertRow(
            new Dictionary<string, object> {
                { "Name", course.Name },
                { "Description", course.Description },
                { "IsActive", course.IsActive },
                { "Category", course.Category },
                { "StartDate", course.StartDate },
                { "EndDate", course.EndDate },
                { "LocationId", course.LocationId },
                { "DateCreated", DateTime.Now },
            }
        );
    }

    public void Update(Course course)
    {
        UpdateRow(
            new Dictionary<string, object> {
                { "Name", course.Name },
                { "Description", course.Description },
                { "IsActive", course.IsActive },
                { "Category", course.Category },
                { "StartDate", course.StartDate },
                { "EndDate", course.EndDate },
                { "LocationId", course.LocationId },
                { "DateUpdated", DateTime.Now },
            },
            "ID = @ID",
            [new MySqlParameter("@ID", course.ID)]
        );
    }

    public void Delete(int id)
    {
        DeleteRow("ID = @ID", [new MySqlParameter("@ID", id)]);
    }
}