CREATE PROCEDURE `spCourse_GetById`(IN p_id INT)
BEGIN
    SELECT * FROM courses WHERE id = p_id;
END