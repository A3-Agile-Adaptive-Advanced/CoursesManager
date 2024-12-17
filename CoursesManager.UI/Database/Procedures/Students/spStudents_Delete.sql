DELIMITER $$
CREATE DEFINER=`courses_manager`@`%` PROCEDURE `spStudents_Delete`(IN p_id INT)
BEGIN
    DELETE FROM students WHERE id = p_id;
END$$
DELIMITER ;