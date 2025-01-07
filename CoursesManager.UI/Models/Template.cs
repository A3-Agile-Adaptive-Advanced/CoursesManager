using CoursesManager.MVVM.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoursesManager.UI.Models
{
    public class Template : ICopyable<Template>
    {
        public int Id { get; set; }
        public string HtmlString { get; set; }
        public string SubjectString { get; set; }
        public string Name { get; set; }

        public Template Copy()
        {
            return new Template { Id = Id, HtmlString = HtmlString, SubjectString = SubjectString, Name = Name };
        }
    }
}
