namespace CoursesManager.UI.Database;

public static class StoredProcedures
{
    #region Courses

    public const string CoursesDeleteById = "spCourses_DeleteById";

    public const string CourseAdd = "spCourse_Add";

    public const string CourseGetAll = "spCourse_GetAll";

    public const string CourseEdit = "spCourse_Edit";

    #endregion

    #region Locations

    public const string LocationsWithAddressesGetAll = "spLocationsWithAddresses_GetAll";
    public const string LocationsInsert = "spLocations_Insert";
    public const string LocationsDeleteById = "spLocations_DeleteById";
    public const string LocationsUpdate = "spLocations_Update";
    public const string LocationUpdate = "spLocations_Update";

    #endregion

    #region Registrations

    public const string RegistrationsGetByCourseId = "spRegistrations_GetByCourseId";
    public const string RegistrationsGetByStudentId = "spRegistrations_GetByStudentId";
    public const string EditRegistrations = "spRegistrations_Edit";
    public const string AddRegistrations = "spRegistrations_Add";
    public const string GetAllRegistrations = "spRegistrations_GetAll";
    public const string DeleteRegistrations = "spRegistrations_Delete";

    #endregion

    #region Addresses

    public const string AddAddress = "spAddresses_Add";
    public const string GetAddressById = "spAddresses_GetById";
    public const string GetAllAddresses = "spAddresses_GetAll";
    public const string UpdateAddress = "spAddresses_Update";
    public const string DeleteAddress = "spAddresses_Delete";

    #endregion

    #region Students

    public const string AddStudent = "spStudents_Add";
    public const string EditStudent = "spStudents_Edit";
    public const string DeleteStudent = "spStudents_Delete";
    public const string GetStudentById = "spStudents_GetById";
    public const string GetAllStudents = "spStudents_GetAll";
    public const string GetNotDeletedStudents = "spStudents_GetNotDeleted";
    public const string GetDeletedStudents = "spStudents_GetDeleted";

    #endregion

    #region Templates
    public const string GetAllTemplates = "spTemplates_GetAll";
    public const string GetTemplateByName = "spTemplates_GetByName";
    public const string UpdateTemplate = "spTemplates_Update";


    #endregion

    #region Certificates
    public const string AddCertificate = "spCertificates_Add";

    #endregion
}