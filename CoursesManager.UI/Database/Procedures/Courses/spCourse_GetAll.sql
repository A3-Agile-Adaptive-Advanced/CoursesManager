CREATE PROCEDURE spCourses_GetAll()
BEGIN
    SELECT
        courses.id,
        courses.name,
        courses.code,
        courses.description,
        courses.location_id,
        courses.is_active,
        courses.start_date,
        courses.end_date,
        courses.created_at,
        courses.tile_image,
        locations.name
    FROM courses
    LEFT JOIN locations ON courses.location_id = locations.id;
END;
