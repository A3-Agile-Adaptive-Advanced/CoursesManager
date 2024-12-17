CREATE DEFINER=`courses_manager`@`%` PROCEDURE `spTemplates_Update`(
    IN p_id INT,
    IN p_html TEXT(9999),
    IN p_name VARCHAR(255),
    IN p_created_at DATETIME,
    IN p_updated_at DATETIME
)
BEGIN
    UPDATE templates
    SET html = p_html, name = p_name, city = p_city, created_at = p_created_at,
        updated_at = p_updated_at
    WHERE id = p_id;
END