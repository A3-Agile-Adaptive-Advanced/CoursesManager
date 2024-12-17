DELIMITER $$
CREATE DEFINER=`courses_manager`@`%` PROCEDURE `spStudents_Edit`(
    IN p_id INT,
    IN p_first_name VARCHAR(255),
    IN p_last_name VARCHAR(255),
    IN p_email VARCHAR(255),
    IN p_phone VARCHAR(20),
    IN p_address_id INT,
    IN p_is_deleted BOOLEAN,
    IN p_insertion VARCHAR(255),
    IN p_date_of_birth DATE
)
BEGIN
    UPDATE students
    SET
        firstname = p_first_name,
        lastname = p_last_name,
        email = p_email,
        phone = p_phone,
        address_id = p_address_id,
        is_deleted = p_is_deleted,
        updated_at = NOW(),
        insertion = p_insertion,
        date_of_birth = p_date_of_birth
    WHERE id = p_id;

    if p_is_deleted = 1 THEN
        UPDATE students
        SET deleted_at = NOW()
        WHERE id = p_id;
    ELSEIF p_is_deleted = 0 THEN
        UPDATE students
        SET deleted_at = NULL
        WHERE id = p_id;
    END IF;
END$$
DELIMITER ;