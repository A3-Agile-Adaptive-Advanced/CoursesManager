using CoursesManager.UI.Models;
using MySql.Data.MySqlClient;
using System.Reflection;
using CoursesManager.UI.Database;
using System.Linq;

namespace CoursesManager.UI.DataAccess;

public class RegistrationDataAccess : BaseDataAccess<Registration>
{
    public List<Registration> GetAllRegistrationsByCourse(int courseId)
    {
        try
        {
            return ExecuteProcedure(StoredProcedures.RegistrationsGetByCourseId, new MySqlParameter("@p_courseId", courseId)).Select(row => new Registration
            {
                Id = Convert.ToInt32(row["id"]),
                CourseId = Convert.ToInt32(row["course_id"]),
                StudentId = Convert.ToInt32(row["student_id"]),
                RegistrationDate = Convert.ToDateTime(row["registration_date"]),
                PaymentStatus = Convert.ToBoolean(row["payment_status"]),
                IsAchieved = Convert.ToBoolean(row["is_achieved"]),
                IsActive = Convert.ToBoolean(row["is_active"])
            }).ToList();
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException(ex.Message, ex);
        }
    }

    public List<Registration> GetAll()
    {
        return ExecuteProcedure(StoredProcedures.GetAllRegistrations).Select(row => new Registration
        {
            Id = Convert.ToInt32(row["id"]),
            CourseId = Convert.ToInt32(row["course_id"]),
            StudentId = Convert.ToInt32(row["student_id"]),
            RegistrationDate = Convert.ToDateTime(row["registration_date"]),
            PaymentStatus = Convert.ToBoolean(row["payment_status"]),
            IsActive = Convert.ToBoolean(row["is_active"]),
            IsAchieved = Convert.ToBoolean(row["is_achieved"]),
            Course = new Course
            {
                Id = Convert.ToInt32(row["course_id"]),
                Name = row["course_name"].ToString(),
            }
        }).ToList();
    }

    public void Delete(int id)
    {
        try
        {
            ExecuteNonProcedure(
                    StoredProcedures.DeleteRegistrations,
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
                StoredProcedures.AddRegistrations,
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
                StoredProcedures.EditRegistrations,
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

            return result.Select(row => new Registration
            {
                Id = Convert.ToInt32(row["id"]),
                CourseId = Convert.ToInt32(row["course_id"]),
                StudentId = Convert.ToInt32(row["student_id"]),
                RegistrationDate = Convert.ToDateTime(row["registration_date"]),
                PaymentStatus = Convert.ToBoolean(row["payment_status"]),
                IsAchieved = Convert.ToBoolean(row["is_achieved"]),
                IsActive = Convert.ToBoolean(row["is_active"])
            }).ToList();
        }
        catch (MySqlException ex)
        {
            throw new InvalidOperationException(ex.Message, ex);
        }
    }
}