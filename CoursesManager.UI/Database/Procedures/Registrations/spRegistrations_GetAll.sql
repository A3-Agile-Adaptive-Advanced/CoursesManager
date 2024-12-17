DELIMITER $$
CREATE DEFINER=`courses_manager`@`%` PROCEDURE `spRegistrations_GetAll`()
BEGIN
    SELECT * FROM registrations;
END$$
DELIMITER ;