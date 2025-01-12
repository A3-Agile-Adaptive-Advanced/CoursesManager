create
    definer = courses_manager@`%` procedure spStudents_Add(IN p_firstname varchar(255), IN p_lastname varchar(255),
                                                           IN p_email varchar(255), IN p_phone varchar(254),
                                                           IN p_address_id int, IN p_is_deleted tinyint(1),
                                                           IN p_deleted_at datetime, IN p_created_at datetime,
                                                           IN p_updated_at datetime, IN p_insertion varchar(255),
                                                           IN p_date_of_birth date, OUT p_id int)
BEGIN
    -- Check for duplicate email
    IF EXISTS (SELECT 1 FROM students WHERE email = p_email) THEN
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'Duplicate email';
    END IF;

    -- Insert the student
    INSERT INTO students (firstname, lastname, email, phone, address_id, is_deleted, deleted_at, created_at, updated_at, insertion, date_of_birth)
    VALUES (p_firstname, p_lastname, p_email, p_phone, p_address_id, p_is_deleted, p_deleted_at, p_created_at, p_updated_at, p_insertion, p_date_of_birth);

    SET p_id = LAST_INSERT_ID();
END;

