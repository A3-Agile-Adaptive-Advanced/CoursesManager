using CoursesManager.UI.Models;
using CoursesManager.UI.Repositories.Base;

namespace CoursesManager.UI.Repositories.TemplateRepository
{
    public interface ITemplateRepository : IUpdateRepository<Template>
    {
        Template? GetTemplateByName(string templateName);
    }
}
