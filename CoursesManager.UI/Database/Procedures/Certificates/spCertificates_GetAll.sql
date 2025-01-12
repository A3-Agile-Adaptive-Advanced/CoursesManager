create
    definer = courses_manager@`%` procedure spCertificates_GetAll()
BEGIN
    SELECT * FROM certificates;
END;

