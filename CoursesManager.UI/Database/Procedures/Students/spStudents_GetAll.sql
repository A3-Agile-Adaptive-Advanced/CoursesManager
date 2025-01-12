create
    definer = courses_manager@`%` procedure spStudents_GetAll()
BEGIN
    SELECT * FROM students;
END;

