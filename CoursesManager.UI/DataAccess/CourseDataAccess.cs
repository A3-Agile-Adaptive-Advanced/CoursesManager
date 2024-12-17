using CoursesManager.UI.Models;
using MySql.Data.MySqlClient;
using CoursesManager.UI.Database;
using System.IO;
using System.Windows.Media.Imaging;

namespace CoursesManager.UI.DataAccess;

public class CourseDataAccess : BaseDataAccess<Course>
{
    private readonly StudentDataAccess _studentDataAccess = new();
    private readonly Registration _registrationDataAccess = new();

    public List<Course> GetAll()
    {
        try
        {
            // Voer de stored procedure uit
            var results = ExecuteProcedure("spCourses_GetAll");

            var students = _studentDataAccess.GetAll();

            // Converteer de resultaten naar een lijst van Course-objecten
            var models = results.Select(row => new Course
            {
                Id = Convert.ToInt32(row["course_id"]),
                Name = row["course_name"]?.ToString() ?? string.Empty,
                Code = row["course_code"]?.ToString() ?? string.Empty,
                Description = row["course_description"]?.ToString() ?? string.Empty,
                LocationId = Convert.ToInt32(row["course_location_id"]),
                Location = new Location
                {
                    Id = Convert.ToInt32(row["course_location_id"]),
                    Name = row["location_name"]?.ToString() ?? string.Empty
                },
                IsActive = Convert.ToBoolean(row["is_active"]),
                StartDate = Convert.ToDateTime(row["start_date"]),
                EndDate = Convert.ToDateTime(row["end_date"]),
                DateCreated = Convert.ToDateTime(row["created_at"]),
                Image = row.ContainsKey("tile_image") && row["tile_image"] != DBNull.Value
                    ? (byte[])row["tile_image"]
                    : null
            }).ToList();


            models.ForEach(m =>
            {
                m.Students = new(students.Where(s => s.Registrations.Any(r => r.CourseId == m.Id)));
                m.Participants = m.Students.Count;
                m.IsPayed = true;
                m.Registrations = new();

                foreach (var student in m.Students)
                {
                    var registration = student.Registrations?.FirstOrDefault(r => r.CourseId == m.Id);
                    m.Registrations.Add(registration);

                    if (registration is not null)
                    {
                        if (m.IsPayed)
                        {
                            m.IsPayed = registration.PaymentStatus;
                        }
                    }
                }
            });

            return models;
        }
        catch (Exception ex)
        {
            LogUtil.Error($"Error in GetAll: {ex.Message}");
            throw;
        }
    }

    public void Add(Course course)
    {
        try
        {
            ExecuteNonProcedure(
                StoredProcedures.CourseAdd,
                new MySqlParameter("@p_coursename", course.Name),
                new MySqlParameter("@p_coursecode", course.Code),
                new MySqlParameter("@p_location_id", course.Location.Id),
                new MySqlParameter("@p_isactive", course.IsActive),
                new MySqlParameter("@p_begindate", course.StartDate),
                new MySqlParameter("@p_enddate", course.EndDate),
                new MySqlParameter("@p_description", course.Description),
                new MySqlParameter("@p_tile_image", course.Image != null ? course.Image : DBNull.Value)
            );

            LogUtil.Log("Stored procedure executed successfully.");
        }
        catch (MySqlException ex)
        {
            LogUtil.Error($"MySQL error in CourseDataAccess.Add: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            LogUtil.Error($"General error in CourseDataAccess.Add: {ex.Message}");
            throw;
        }
    }



    public void Update(Course course)
    {
        ArgumentNullException.ThrowIfNull(course);

        try
        {
            ExecuteNonProcedure(
                StoredProcedures.CourseEdit,
                new MySqlParameter("@p_id", course.Id),
                new MySqlParameter("@p_coursename", course.Name),
                new MySqlParameter("@p_coursecode", course.Code),
                new MySqlParameter("@p_location_id", course.LocationId),
                new MySqlParameter("@p_isactive", course.IsActive),
                new MySqlParameter("@p_begindate", course.StartDate),
                new MySqlParameter("@p_enddate", course.EndDate),
                new MySqlParameter("@p_description", course.Description),
                new MySqlParameter("@p_tile_image", course.Image != null ? course.Image : DBNull.Value)
            );

            LogUtil.Log("Course updated successfully.");
        }
        catch (MySqlException ex)
        {
            LogUtil.Error($"MySQL error in Update: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            LogUtil.Error($"General error in Update: {ex.Message}");
            throw;
        }
    }


    public void Delete(int id)
    {
        try
        {
            ExecuteNonProcedure(StoredProcedures.CoursesDeleteById, new MySqlParameter("@p_id", id));
            LogUtil.Log("Course deleted successfully.");
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException(ex.Message, ex);
        }
    }
}