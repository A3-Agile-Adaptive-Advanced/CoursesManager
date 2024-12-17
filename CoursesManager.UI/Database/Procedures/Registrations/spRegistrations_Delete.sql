DELIMITER $$
CREATE DEFINER=`courses_manager`@`%` PROCEDURE `spRegistrations_Delete`(IN p_id INT)
BEGIN
    DELETE FROM registrations WHERE id = p_id;
END$$
DELIMITER ;