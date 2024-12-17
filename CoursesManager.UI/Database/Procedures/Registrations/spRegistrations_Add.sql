DELIMITER $$
CREATE DEFINER=`courses_manager`@`%` PROCEDURE `spRegistrations_Add`(
    IN p_course_id INT,
    IN p_student_id INT,
    IN p_registration_date DATE,
    IN p_payment_status TINYINT,
    IN p_is_active TINYINT,
    IN p_is_achieved TINYINT
)
BEGIN
    INSERT INTO registrations (course_id, student_id, registration_date, payment_status, is_active, is_achieved, created_at, updated_at)
    VALUES (p_course_id, p_student_id, p_registration_date, p_payment_status, p_is_active, p_is_achieved, NOW(), NOW());
END$$
DELIMITER ;
