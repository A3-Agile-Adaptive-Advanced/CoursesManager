CREATE PROCEDURE spCourse_AddById
(
    IN p_coursename NVARCHAR(255),
    IN p_coursecode NVARCHAR(255),
    IN p_location_id INT,
    IN p_isactive BOOLEAN,
    IN p_begindate DATE,
    IN p_enddate DATE,
    IN p_description TEXT
)
BEGIN
    INSERT INTO courses (name, code, description, location_id, is_active, start_date, end_date, created_at, updated_at)
    VALUES (p_coursename, p_coursecode, p_description, p_location_id, p_isactive, p_begindate, p_enddate, NOW(), NOW());
END;
