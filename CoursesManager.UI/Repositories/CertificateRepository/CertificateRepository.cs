using CoursesManager.UI.DataAccess;
using CoursesManager.UI.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoursesManager.UI.Repositories.CertificateRepository
{
    public class CertificateRepository : ICertificateRepository
    {
        private readonly CertificateDataAccess _certificateDataAccess;

        public CertificateRepository()
        {
            CertificateDataAccess certificateDataAccess = new CertificateDataAccess();
            _certificateDataAccess = certificateDataAccess;
        }


        public bool SaveCertificate(Template template, Course course, Student student)
        {
            return _certificateDataAccess.SaveCertificate(template, course, student);
        }
    }
}
