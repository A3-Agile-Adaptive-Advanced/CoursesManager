using CoursesManager.UI.Database;
using CoursesManager.UI.Models;
using MySql.Data.MySqlClient;
using System;
using System.Data;

namespace CoursesManager.UI.DataAccess
{
    internal class TemplateDataAccess : BaseDataAccess<Template>
    {

        public List<Template> GetAll()
        {
            string procedureName = StoredProcedures.GetAllTemplates;
            try
            {
                return ExecuteProcedure(procedureName).Select(row => ToTemplate(row)).ToList();

            }
            catch (MySqlException ex)
            {
                LogUtil.Error($"Error executing procedure '{procedureName}': {ex.Message}");
                throw;
            }
        }

        public Template? GetByName(string name)
        {
            string procedureName = StoredProcedures.GetTemplateByName;
            try
            {
                return ExecuteProcedure(procedureName, new MySqlParameter("@p_name", name)).Select(row => ToTemplate(row)).FirstOrDefault();

            }
            catch (MySqlException ex)
            {
                LogUtil.Error($"Error executing procedure '{procedureName}': {ex.Message}");
                throw;
            }

        }

        public void UpdateTemplate(Template template)
        {

            try
            {
                ExecuteNonProcedure(StoredProcedures.UpdateTemplate, [
                    new MySqlParameter("@p_id", template.Id),
                    new MySqlParameter("@p_html", template.HtmlString),
                    new MySqlParameter("@subject", template.SubjectString),
                    new MySqlParameter("@p_name", template.Name),
                    new MySqlParameter("@p_updated_at", DateTime.Now)
                ]);
            }
            catch (MySqlException ex)
            {
                LogUtil.Error(ex.Message);
                throw;
            }
        }

        private Template ToTemplate(Dictionary<string, object> row)
        {
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
