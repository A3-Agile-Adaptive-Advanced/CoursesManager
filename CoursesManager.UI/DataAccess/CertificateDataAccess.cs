using CoursesManager.UI.Database;
using CoursesManager.UI.Models;
using MySql.Data.MySqlClient;
using System.Data;

namespace CoursesManager.UI.DataAccess
{
    public class CertificateDataAccess : BaseDataAccess<Certificate>
    {

        public bool SaveCertificate(Template template, Course course, Student student)
        {
            try
            {
                string procedureName = StoredProcedures.AddCertificate;

                var rows = ExecuteProcedure(procedureName, [
                    new MySqlParameter("@p_pdf_html", template.HtmlString),
                    new MySqlParameter("@p_student_code", student.Id),
                    new MySqlParameter("@p_course_code", course.Code),
                    new MySqlParameter("@p_created_at", DateTime.Now)]);

                if (rows.Any())
                {
                    return true;
                }
                return false;
            }
            catch (MySqlException ex)
            {
                LogUtil.Error(ex.Message);
                throw;
            }
        }

    }
}
