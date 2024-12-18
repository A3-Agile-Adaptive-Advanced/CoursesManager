using CoursesManager.UI.Models;

namespace CoursesManager.UI.Repositories.TemplateRepository
{
    public interface ITemplateRepository : IRepository<Template>
    {
        Template GetTemplateByName(string templateName);
        void UpdateTemplate(Template template);
    }
}
