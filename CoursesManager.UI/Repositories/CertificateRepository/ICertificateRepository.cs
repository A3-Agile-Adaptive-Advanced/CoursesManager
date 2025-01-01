using CoursesManager.UI.Models;
using CoursesManager.UI.Repositories.Base;

namespace CoursesManager.UI.Repositories.CertificateRepository
{
    public interface ICertificateRepository : IAddRepository<Certificate>, IReadOnlyRepository<Certificate>
    {
    }
}
