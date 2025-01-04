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
                throw new DataAccessException("Something went wrong while accessing the database");
            }
        }

    }
}
