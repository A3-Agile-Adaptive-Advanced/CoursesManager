using CoursesManager.UI.DataAccess;
using CoursesManager.UI.Models;

namespace CoursesManager.UI.Repositories.TemplateRepository
{
    public class TemplateRepository : ITemplateRepository
    {
        private readonly TemplateDataAccess _templateDataAccess;
        public TemplateRepository() 
        {
            TemplateDataAccess templateDataAccess = new TemplateDataAccess();
            _templateDataAccess = templateDataAccess;
        }

        public Template GetTemplateByName(string name)
        {
            try
            {
                return _templateDataAccess.GetByName(name);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public void UpdateTemplate(Template template)
        {
            try
            {
                _templateDataAccess.UpdateTemplate(template);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public void Add(Template data)
        {
            throw new NotImplementedException();
        }

        public void Delete(Template data)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public List<Template> GetAll()
        {
            throw new NotImplementedException();
        }

        public Template? GetById(int id)
        {
            throw new NotImplementedException();
        }

        public List<Template> RefreshAll()
        {
            throw new NotImplementedException();
        }

        public void Update(Template data)
        {
            throw new NotImplementedException();
        }

    }
}
