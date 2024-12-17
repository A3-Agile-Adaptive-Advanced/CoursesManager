using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoursesManager.UI.Models
{
    public class Certificate
    {
        public int Id { get; set; }
        public string PdfString { get; set; }
        public int StudentCode { get; set; }
        public string CourseCode { get; set; }
    }
}
