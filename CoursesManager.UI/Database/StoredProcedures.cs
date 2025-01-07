namespace CoursesManager.UI.Database;

public static class StoredProcedures
{
    #region Courses

    public const string DeleteCourse = "spCourses_DeleteById";
    public const string AddCourse = "spCourse_Add";
    public const string GetAllCourses = "spCourses_GetAll";
    public const string UpdateCourse = "spCourse_Edit";
    public const string GetCourseById = "spCourses_GetById";

    #endregion Courses

    #region Locations

    public const string GetAllLocationsWithAddresses = "spLocationsWithAddresses_GetAll";
    public const string AddLocation = "spLocations_Insert";
    public const string DeleteLocation = "spLocations_DeleteById";
    public const string UpdateLocation = "spLocations_Update";
    public const string GetLocationWithAddressesById = "spLocationsWithAddresses_GetById";

    #endregion Locations

    #region Registrations

    public const string GetRegistrationsByCourseId = "spRegistrations_GetByCourseId";
    public const string GetRegistrationsByStudentId = "spRegistrations_GetByStudentId";
    public const string GetRegistrationById = "spRegistrations_GetById";
    public const string EditRegistration = "spRegistrations_Edit";
    public const string AddRegistration = "spRegistrations_Add";
    public const string GetAllRegistrations = "spRegistrations_GetAll";
    public const string DeleteRegistration = "spRegistrations_Delete";

    #endregion Registrations

    #region Addresses

    public const string AddAddress = "spAddresses_Add";
    public const string GetAddressById = "spAddresses_GetById";
    public const string GetAllAddresses = "spAddresses_GetAll";
    public const string UpdateAddress = "spAddresses_Update";
    public const string DeleteAddress = "spAddresses_Delete";

    #endregion Addresses

    #region Students

    public const string AddStudent = "spStudents_Add";
    public const string UpdateStudent = "spStudents_Edit";
    public const string DeleteStudent = "spStudents_Delete";
    public const string GetStudentById = "spStudents_GetById";
    public const string GetAllStudents = "spStudents_GetAll";
    public const string GetNotDeletedStudents = "spStudents_GetNotDeleted";
    public const string GetDeletedStudents = "spStudents_GetDeleted";

    #endregion Students

    #region Templates

    public const string GetAllTemplates = "spTemplates_GetAll";
    public const string GetTemplateByName = "spTemplates_GetByName";
    public const string UpdateTemplate = "spTemplates_Update";

    #endregion Templates

    #region Certificates

    public const string AddCertificate = "spCertificates_Add";
    public const string GetAllCertificates = "spCertificates_GetAll";
    public const string GetCertificateById = "spCertificates_GetById";
    public static string CheckIfCertificateExists = "spCertificates_GetByStudentAndCourseCode";

    #endregion Certificates

}