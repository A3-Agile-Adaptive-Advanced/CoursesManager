create
    definer = courses_manager@`%` procedure spAddresses_GetById(IN p_id int)
BEGIN
    SELECT * FROM addresses WHERE id = p_id;
END;

