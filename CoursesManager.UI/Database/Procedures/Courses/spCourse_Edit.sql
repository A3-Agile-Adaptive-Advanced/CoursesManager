CREATE PROCEDURE spCourse_Edit(
    IN p_id INT,
    IN p_coursename NVARCHAR(255),
    IN p_coursecode NVARCHAR(255),
    IN p_location_id INT,
    IN p_isactive BOOLEAN,
    IN p_begindate DATE,
    IN p_enddate DATE,
    IN p_description TEXT,
    IN p_tile_image LONGBLOB
)
BEGIN
    UPDATE courses
    SET 
        name = p_coursename,
        code = p_coursecode,
        description = p_description,
        location_id = p_location_id,
        is_active = p_isactive,
        tile_image = p_tile_image,
        start_date = p_begindate,
        end_date = p_enddate,
        updated_at = NOW()
    WHERE id = p_id;
END;
