create
    definer = courses_manager@`%` procedure spCourse_GetBetweenDates(IN p_startDate datetime, IN p_endDate datetime)
BEGIN
    SELECT
        courses.id AS course_id,
        courses.name AS course_name,
        courses.code AS course_code,
        courses.description AS course_description,
        courses.location_id AS course_location_id,
        courses.is_active,
        courses.start_date,
        courses.end_date,
        courses.created_at,
        courses.tile_image,
        locations.name AS location_name
    FROM courses
    LEFT JOIN locations ON courses.location_id = locations.id
    WHERE (courses.start_date >= p_startDate AND courses.start_date <= p_endDate)
      OR (courses.end_date >= p_startDate AND courses.end_date <= p_endDate);
END;

