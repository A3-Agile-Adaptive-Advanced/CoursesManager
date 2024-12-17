CREATE DEFINER=`courses_manager`@`%` PROCEDURE `spTemplates_GetByName`(
    IN p_name TEXT
)
BEGIN
    SELECT * FROM templates WHERE name = p_name;
END