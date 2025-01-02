using CoursesManager.UI.DataAccess;
using CoursesManager.UI.Repositories.AddressRepository;
using CoursesManager.UI.Repositories.CertificateRepository;
using CoursesManager.UI.Repositories.CourseRepository;
using CoursesManager.UI.Repositories.LocationRepository;
using CoursesManager.UI.Repositories.RegistrationRepository;
using CoursesManager.UI.Repositories.StudentRepository;
using CoursesManager.UI.Repositories.TemplateRepository;
using CoursesManager.UI.Service;

namespace CoursesManager.UI.Factory;

public class RepositoryFactory
{
    public IAddressRepository AddressRepository => new AddressRepository(new AddressDataAccess());

    public ILocationRepository LocationRepository => new LocationRepository(new LocationDataAccess(), AddressRepository);

    public ICourseRepository CourseRepository => new CourseRepository(new CourseDataAccess(), LocationRepository, StudentRegistrationCourseAggregator);

    public IStudentRepository StudentRepository => new StudentRepository(new StudentDataAccess(), AddressRepository, StudentRegistrationCourseAggregator);

    public IRegistrationRepository RegistrationRepository => new RegistrationRepository(new RegistrationDataAccess(), StudentRegistrationCourseAggregator);

    public ITemplateRepository TemplateRepository => new TemplateRepository(new TemplateDataAccess());

    public ICertificateRepository CertificateRepository = new CertificateRepository(new CertificateDataAccess());

    public IStudentRegistrationCourseAggregator StudentRegistrationCourseAggregator;

    public RepositoryFactory()
    {
        StudentRegistrationCourseAggregator = new StudentRegistrationCourseAggregator(this);
    }
}

