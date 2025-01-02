using CoursesManager.UI.DataAccess;
using CoursesManager.UI.Models;
using System.Collections.ObjectModel;
using CoursesManager.UI.Repositories.Base;

namespace CoursesManager.UI.Repositories.TemplateRepository
{
    public class TemplateRepository : BaseRepository<Template>, ITemplateRepository
    {
        private readonly TemplateDataAccess _templateDataAccess;

        private readonly ObservableCollection<Template> _templates;

        private const string Cachekey = "templatesCache";

        private static readonly object SharedLock = new();

        public TemplateRepository(TemplateDataAccess templateDataAccess)
        {
            _templateDataAccess = templateDataAccess;

            try
            {
                _templates = GlobalCache.Instance.Get(Cachekey) as ObservableCollection<Template> ?? SetupCache(Cachekey);
            }
            catch
            {
                _templates = SetupCache(Cachekey);
            }
        }

        public Template? GetTemplateByName(string name)
        {
            lock (SharedLock)
            {
                var item = _templates.FirstOrDefault(t => t.Name == name);

                if (item is null)
                {
                    item = _templateDataAccess.GetByName(name);

                    if (item is not null) _templates.Add(item);
                }

                return item;
            }
        }

        public void Update(Template template)
        {
            lock (SharedLock)
            {
                _templateDataAccess.UpdateTemplate(template);

                var item = GetTemplateByName(template.Name) ?? throw new InvalidOperationException($"Template with name: {template.Name} does not exist.");

                OverwriteItemInPlace(item, template);
            }
        }
    }
}
