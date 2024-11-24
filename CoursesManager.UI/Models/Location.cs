using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoursesManager.UI.Models
{
    public class Location
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Address Address { get; set; }
        public string Capacity { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
