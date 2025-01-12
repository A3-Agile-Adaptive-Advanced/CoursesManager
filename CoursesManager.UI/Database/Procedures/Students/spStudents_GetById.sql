create
    definer = courses_manager@`%` procedure spStudents_GetById(IN p_id int)
BEGIN
    SELECT * FROM students WHERE id = p_id;
END;