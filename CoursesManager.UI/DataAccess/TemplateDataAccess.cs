using CoursesManager.UI.Database;
using CoursesManager.UI.Models;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using CoursesManager.MVVM.Exceptions;

namespace CoursesManager.UI.DataAccess
{
    public class TemplateDataAccess : BaseDataAccess<Template>
    {
        public Template? GetByName(string name)
        {
            string procedureName = StoredProcedures.GetTemplateByName;
            try
            {
                Template template = new();
                template = ExecuteProcedure(procedureName, new MySqlParameter("@p_name", name))
                    .Select(row => ToTemplate(row)).FirstOrDefault() ?? template;
                if (template == null) throw new DataAccessException("Template not found");
                return template;
            }
            catch (MySqlException ex)
            {
                LogUtil.Error($"Error executing procedure '{procedureName}': {ex.Message}");
                throw new DataAccessException("Something went wrong while accessing the database", ex);
            }


        }

        public void UpdateTemplate(Template template)
        {
            try
            {
                ExecuteNonProcedure(StoredProcedures.UpdateTemplate, [
                    new MySqlParameter("@p_id", template.Id),
                    new MySqlParameter("@p_html", template.HtmlString),
                    new MySqlParameter("@p_subject", template.SubjectString),
                    new MySqlParameter("@p_name", template.Name),
                    new MySqlParameter("@p_updated_at", DateTime.Now)
                ]);
            }
            catch (MySqlException ex)
            {
                LogUtil.Error(ex.Message);
                throw new DataAccessException("Something went wrong while accessing the database", ex);
            }
        }

        private Template ToTemplate(Dictionary<string, object>? row)
        {
            if (row == null)
            {
                throw new DataAccessException("Row data is null.");
            }
            return new Template
            {
                Id = Convert.ToInt32(row["id"]),
                HtmlString = row["html"]?.ToString() ?? string.Empty,
                SubjectString = row["subject"]?.ToString() ?? string.Empty,
                Name = row["name"]?.ToString() ?? string.Empty,

            };
        }
    }

}
