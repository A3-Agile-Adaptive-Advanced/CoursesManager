using CoursesManager.UI.Models;
using MySql.Data.MySqlClient;
using CoursesManager.UI.Database;

namespace CoursesManager.UI.DataAccess;

public class RegistrationDataAccess : BaseDataAccess<Registration>
{
    public List<Registration> GetAllRegistrationsByCourse(int courseId)
    {
        try
        {
            return ExecuteProcedure(StoredProcedures.GetRegistrationsByCourseId, new MySqlParameter("@p_courseId", courseId)).Select(MapToRegistration).ToList();
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException(ex.Message, ex);
        }
    }

    private static Registration MapToRegistration(Dictionary<string, object?> row)
    {
        if (row.TryGetValue("id", out var id))
        {
        }

        return new Registration
        {
            Id = Convert.ToInt32(id ?? row["registration_id"]),
            CourseId = Convert.ToInt32(row["course_id"]),
            StudentId = Convert.ToInt32(row["student_id"]),
            RegistrationDate = Convert.ToDateTime(row["registration_date"]),
            PaymentStatus = Convert.ToBoolean(row["payment_status"]),
            IsAchieved = Convert.ToBoolean(row["is_achieved"]),
            IsActive = Convert.ToBoolean(row["is_active"])
        };
    }

    public List<Registration> GetAll()
    {
        return ExecuteProcedure(StoredProcedures.GetAllRegistrations).Select(MapToRegistration).ToList();
    }

    public void Delete(int id)
    {
        try
        {
            ExecuteNonProcedure(StoredProcedures.DeleteRegistration,
                new MySqlParameter("@p_id", id)
            );
            LogUtil.Log($"Registration deleted successfully for Registration ID: {id}");
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

    public void Add(Registration registration)
    {
        try
        {
            ExecuteNonProcedure(
                StoredProcedures.AddRegistration,
                new MySqlParameter("@p_is_achieved", registration.IsAchieved),
                new MySqlParameter("@p_is_active", registration.IsActive),
                new MySqlParameter("@p_payment_status", registration.PaymentStatus),
                new MySqlParameter("@p_registration_date", registration.RegistrationDate),
                new MySqlParameter("@p_student_id", registration.StudentId),
                new MySqlParameter("@p_course_id", registration.CourseId)
            );

            LogUtil.Log($"Registration added successfully for Student ID: {registration.StudentId}, Course ID: {registration.CourseId}");
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

    public void Update(Registration registration)
    {
        try
        {
            ExecuteNonProcedure(
                StoredProcedures.EditRegistration,
                new MySqlParameter("@p_id", registration.Id),
                new MySqlParameter("@p_is_achieved", registration.IsAchieved),
                new MySqlParameter("@p_is_active", registration.IsActive),
                new MySqlParameter("@p_payment_status", registration.PaymentStatus),
                new MySqlParameter("@p_registration_date", registration.RegistrationDate),
                new MySqlParameter("@p_student_id", registration.StudentId),
                new MySqlParameter("@p_course_id", registration.CourseId)
            );

            LogUtil.Log($"Registration updated successfully for Student ID: {registration.StudentId}, Course ID: {registration.CourseId}");
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

    public List<Registration> GetByStudentId(int studentId)
    {
        try
        {
            var result = ExecuteProcedure("GetRegistrationsByStudentId", new MySqlParameter[]
            {
                new MySqlParameter("@p_student_id", studentId)
            });

            return result.Select(MapToRegistration).ToList();
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException(ex.Message, ex);
        }
    }

    public Registration? GetById(int id)
    {
        try
        {
            var res = ExecuteProcedure(StoredProcedures.GetRegistrationById, new MySqlParameter("@p_id", id));

            return !res.Any() ? null : MapToRegistration(res.First());
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException(ex.Message, ex);
        }
    }

    public List<Registration> GetAllRegistrationsByStudent(int studentId)
    {
        try
        {
            return ExecuteProcedure(StoredProcedures.GetRegistrationsByStudentId, new MySqlParameter("@p_studentId", studentId)).Select(MapToRegistration).ToList();
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException(ex.Message, ex);
        }
    }
}