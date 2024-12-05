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

    #endregion

    #region Registrations

    public const string RegistrationsGetByCourseId = "spRegistrations_GetByCourseId";

    #endregion
}