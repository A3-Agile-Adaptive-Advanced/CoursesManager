using CoursesManager.MVVM.Exceptions;
using CoursesManager.UI.Database;
using CoursesManager.UI.Models;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Common;
using System.Data;
using System.Diagnostics;

namespace CoursesManager.UI.DataAccess
{
    public class CertificateDataAccess : BaseDataAccess<Certificate>
    {
        // First we have to check if a certificate exists in the db before we enter a new one.
        // This ensures unique certificates for each student and makes the retrieval process logical.
        public void SaveCertificate(Certificate certificate)
        {
            try
            {
                string checkProcedureName = StoredProcedures.CheckIfCertificateExists;
                var exists = ExecuteProcedure(checkProcedureName, new[]
                {
                    new MySqlParameter("@p_student_code", certificate.StudentCode),
                    new MySqlParameter("@p_course_code", certificate.CourseCode)
                });
                if (exists != null && exists.Any())
                {
                    throw new InvalidOperationException(
                        "A certificate already exists for this student and course combination.");
                }

                var outputParameter = new MySqlParameter("@p_id", MySqlDbType.Int32)
                {
                    Direction = ParameterDirection.Output
                };

                string procedureName = StoredProcedures.AddCertificate;

                var rows = ExecuteProcedure(procedureName, [
                    new MySqlParameter("@p_pdf_html", certificate.PdfString),
                    new MySqlParameter("@p_student_code", certificate.StudentCode),
                    new MySqlParameter("@p_course_code", certificate.CourseCode),
                    new MySqlParameter("@p_created_at", DateTime.Now),
                    outputParameter]);

                certificate.Id = Convert.ToInt32(outputParameter.Value);
            }
            catch (MySqlException ex)
            {
                LogUtil.Error(ex.Message);
                throw new DataAccessException("Something went wrong while accessing the database", ex);
            }
        }

        public List<Certificate> GetAll()
        {
            try
            {
                return ExecuteProcedure(StoredProcedures.GetAllCertificates).Select(MapToCertificate).ToList();
            }
            catch (MySqlException ex)
            {
                LogUtil.Error(ex.Message);
                throw new DataAccessException("Something went wrong while accessing the database", ex);
            }
        }

        private static Certificate MapToCertificate(Dictionary<string, object?> row)
        {
            return new Certificate
            {
                CourseCode = row["course_code"]?.ToString() ?? string.Empty,
                Id = Convert.ToInt32(row["id"]),
                PdfString = row["pdf_html"]?.ToString() ?? string.Empty,
                StudentCode = Convert.ToInt32(row["student_code"])
            };
        }

        public Certificate? GetById(int id)
        {
            try
            {
                var res = ExecuteProcedure(StoredProcedures.GetCertificateById, [
                    new MySqlParameter("@p_id", id)
                ]);

                return !res.Any() ? null : MapToCertificate(res.First());
            }
            catch (MySqlException ex)
            {
                LogUtil.Error(ex.Message);
                throw new DataAccessException("Something went wrong while accessing the database", ex);
            }
        }
    }
}
