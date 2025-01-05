CREATE PROCEDURE `spCertificates_GetByStudentAndCourseCode`(IN p_student_code INT, p_course_code INT)
BEGIN
SELECT * FROM certificates WHERE student_code = p_student_code AND course_code = p_course_code;
END