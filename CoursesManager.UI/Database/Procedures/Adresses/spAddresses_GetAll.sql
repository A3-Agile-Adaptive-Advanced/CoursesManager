create
    definer = courses_manager@`%` procedure spAddresses_GetAll()
BEGIN
    SELECT * FROM addresses;
END;

