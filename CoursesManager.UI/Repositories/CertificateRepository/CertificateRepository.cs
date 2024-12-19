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

        public void Add(Certificate certificate)
        {
            _certificateDataAccess.SaveCertificate(certificate);
        }

        public void Delete(Certificate data)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public List<Certificate> GetAll()
        {
            throw new NotImplementedException();
        }

        public Certificate? GetById(int id)
        {
            throw new NotImplementedException();
        }

        public List<Certificate> RefreshAll()
        {
            throw new NotImplementedException();
        }

        public void Update(Certificate data)
        {
            throw new NotImplementedException();
        }
    }
}
