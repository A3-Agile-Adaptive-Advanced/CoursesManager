using CoursesManager.UI.DataAccess;
using CoursesManager.UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public Template GetTemplateByName(string name)
        {
            return _templateDataAccess.GetByName(name);
        }

        public List<Template> RefreshAll()
        {
            throw new NotImplementedException();
        }

        public void Update(Template data)
        {
            throw new NotImplementedException();
        }

        public void UpdateTemplate(Template template)
        {
            _templateDataAccess.UpdateTemplate(template);
        }
    }
}
