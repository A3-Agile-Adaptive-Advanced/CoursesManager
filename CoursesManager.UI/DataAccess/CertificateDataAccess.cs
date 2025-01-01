using CoursesManager.MVVM.Exceptions;
using CoursesManager.UI.Database;
using CoursesManager.UI.Models;
using MySql.Data.MySqlClient;

namespace CoursesManager.UI.DataAccess
{
    public class CertificateDataAccess : BaseDataAccess<Certificate>
    {

        public void SaveCertificate(Certificate certificate)
        {
            try
            {
                string procedureName = StoredProcedures.AddCertificate;

                var rows = ExecuteProcedure(procedureName, [
                    new MySqlParameter("@p_pdf_html", certificate.PdfString),
                    new MySqlParameter("@p_student_code", certificate.StudentCode),
                    new MySqlParameter("@p_course_code", certificate.CourseCode),
                    new MySqlParameter("@p_created_at", DateTime.Now)]);
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
