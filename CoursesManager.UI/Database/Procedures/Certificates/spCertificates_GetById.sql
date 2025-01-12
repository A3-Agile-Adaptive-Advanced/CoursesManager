create
    definer = courses_manager@`%` procedure spCertificates_GetById(IN p_id int)
BEGIN
    SELECT * FROM certificates WHERE id = p_id;
END;

