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
        private readonly TemplateDataAccess _TemplatedataAccess;
        public TemplateRepository() 
        {
            TemplateDataAccess templateDataAccess = new TemplateDataAccess();
            _TemplatedataAccess = templateDataAccess;
        }
        public Template? GetTemplateByName(string name)
        {
            return _TemplatedataAccess.GetByName(name);
        }

    }
}
