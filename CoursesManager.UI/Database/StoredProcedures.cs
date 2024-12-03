namespace CoursesManager.UI.Database;

public static class StoredProcedures
{
    #region Courses

    public const string CoursesDeleteById = "spCourses_DeleteById";

    public const string CourseAddById = "spCourse_AddById";

    public const string CourseGetAll = "spCourse_GetAll";

    public const string CourseEditById = "spCourse_EditById";

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