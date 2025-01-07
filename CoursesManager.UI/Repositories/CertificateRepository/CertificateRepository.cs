using CoursesManager.UI.DataAccess;
using CoursesManager.UI.Models;
using System.Collections.ObjectModel;
using CoursesManager.UI.Repositories.Base;

namespace CoursesManager.UI.Repositories.CertificateRepository
{
    public class CertificateRepository : BaseRepository<Certificate>, ICertificateRepository
    {
        private readonly CertificateDataAccess _certificateDataAccess;

        private readonly ObservableCollection<Certificate> _certificates;

        private const string Cachekey = "certificatesCache";

        private static readonly object SharedLock = new();

        public CertificateRepository(CertificateDataAccess certificateDataAccess)
        {
            _certificateDataAccess = certificateDataAccess;

            try
            {
                _certificates = GlobalCache.Instance.Get(Cachekey) as ObservableCollection<Certificate> ??
                                SetupCache(Cachekey);
            }
            catch
            {
                _certificates = SetupCache(Cachekey);
            }
        }

        public ObservableCollection<Certificate> GetAll()
        {
            lock (SharedLock)
            {
                if (_certificates.Count == 0)
                {
                    _certificateDataAccess.GetAll().ForEach(_certificates.Add);
                }

                return _certificates;
            }
        }

        public Certificate? GetById(int id)
        {
            lock (SharedLock)
            {
                var item = _certificates.FirstOrDefault(c => c.Id == id);

                if (item is null)
                {
                    item = _certificateDataAccess.GetById(id);

                    if (item is not null) _certificates.Add(item);
                }

                return item;
            }
        }

        public void Add(Certificate certificate)
        {
            lock (SharedLock)
            {
                _certificateDataAccess.SaveCertificate(certificate);
                _certificates.Add(certificate);
            }
        }
    }
}
