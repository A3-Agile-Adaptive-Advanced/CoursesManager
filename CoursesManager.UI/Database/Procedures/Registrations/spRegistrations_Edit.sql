DELIMITER $$
CREATE DEFINER=`courses_manager`@`%` PROCEDURE `spRegistrations_Edit`(
	IN p_id INT,
    IN p_course_id INT,
    IN p_student_id INT,
    IN p_registration_date DATE,
    IN p_payment_status TINYINT,
    IN p_is_active TINYINT,
    IN p_is_achieved TINYINT
)
BEGIN
    UPDATE registrations
    SET
        course_id = p_course_id,
        student_id = p_student_id,
        registration_date = p_registration_date,
        payment_status = p_payment_status,
        is_active = p_is_active,
        is_achieved = p_is_achieved,
        updated_at = NOW()
    WHERE id = p_id;
END$$
DELIMITER ;
