CREATE DEFINER=`courses_manager`@`%` PROCEDURE `spCertificates_Add`(
    IN p_pdf_html TEXT,
    IN student_code INT,
    IN p_course_code VARCHAR(99),
    IN p_created_at DATETIME
)
BEGIN
    INSERT INTO certificates (pdf_html, student_code, course_code, created_at)
    VALUES (p_pdf_html, student_code, p_course_code, p_created_at);
END