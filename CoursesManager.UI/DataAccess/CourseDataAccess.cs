using CoursesManager.UI.Models;
using MySql.Data.MySqlClient;
using CoursesManager.UI.Database;
using System.Data;

namespace CoursesManager.UI.DataAccess;

public class CourseDataAccess : BaseDataAccess<Course>
{
    public List<Course> GetAll()
    {
        try
        {
            // Voer de stored procedure uit
            var results = ExecuteProcedure(StoredProcedures.GetAllCourses);

            // Converteer de resultaten naar een lijst van Course-objecten
            var models = results.Select(MapToCourse).ToList();

            return models;
        }
        catch (Exception ex)
        {
            LogUtil.Error($"Error in GetAll: {ex.Message}");
            throw;
        }
    }

    public List<Course> GetAllBetweenDates(DateTime start, DateTime end)
    {
        try
        {
            List<Dictionary<string, object?>> results = ExecuteProcedure(
                StoredProcedures.GetCoursesBetweenDates, 
                new MySqlParameter("@p_startDate", start),
                new MySqlParameter("@p_endDate", end)
            );

            List<Course> models = results.Select(MapToCourse).ToList();

            return models;
        }
        catch (Exception ex)
        {
            LogUtil.Error($"Error in GetAllBetweenDates: {ex.Message}");
            throw;
        }
    }

    private static Course MapToCourse(Dictionary<string, object?> row)
    {
        return new Course
        {
            Id = Convert.ToInt32(row["course_id"]),
            Name = row["course_name"]?.ToString() ?? string.Empty,
            Code = row["course_code"]?.ToString() ?? string.Empty,
            Description = row["course_description"]?.ToString() ?? string.Empty,
            LocationId = Convert.ToInt32(row["course_location_id"]),
            IsActive = Convert.ToBoolean(row["is_active"]),
            StartDate = Convert.ToDateTime(row["start_date"]),
            EndDate = Convert.ToDateTime(row["end_date"]),
            DateCreated = Convert.ToDateTime(row["created_at"]),
            Image = row.ContainsKey("tile_image") && row["tile_image"] != DBNull.Value
                ? (byte[])row["tile_image"]
                : null
        };
    }


    public void Add(Course course)
    {
        try
        {
            var outputParameter = new MySqlParameter("@p_id", MySqlDbType.Int32)
            {
                Direction = ParameterDirection.Output
            };

            ExecuteNonProcedure(
                StoredProcedures.AddCourse,
                new MySqlParameter("@p_coursename", course.Name),
                new MySqlParameter("@p_coursecode", course.Code),
                new MySqlParameter("@p_location_id", course.Location.Id),
                new MySqlParameter("@p_isactive", course.IsActive),
                new MySqlParameter("@p_begindate", course.StartDate),
                new MySqlParameter("@p_enddate", course.EndDate),
                new MySqlParameter("@p_description", course.Description),
                new MySqlParameter("@p_tile_image", course.Image != null ? course.Image : DBNull.Value),
                outputParameter
            );

            course.Id = Convert.ToInt32(outputParameter.Value);

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
                StoredProcedures.UpdateCourse,
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
            ExecuteNonProcedure(StoredProcedures.DeleteCourse, new MySqlParameter("@p_id", id));
            LogUtil.Log("Course deleted successfully.");
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException(ex.Message, ex);
        }
    }

    public Course? GetById(int id)
    {
        try
        {
            var res = ExecuteProcedure(StoredProcedures.GetCourseById, [
                new MySqlParameter("@p_id", id)
            ]);

            return !res.Any() ? null : MapToCourse(res.First());
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException(ex.Message, ex);
        }
    }
}