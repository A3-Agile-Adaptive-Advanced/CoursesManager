using CoursesManager.UI.Database;
using CoursesManager.UI.Models;
using MySql.Data.MySqlClient;
using System.Data;

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
                throw;
            }
        }

    }
}
