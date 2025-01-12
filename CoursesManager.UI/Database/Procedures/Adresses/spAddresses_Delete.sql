create
    definer = courses_manager@`%` procedure spAddresses_Delete(IN p_id int)
BEGIN
    DELETE FROM addresses WHERE id = p_id;
END;

