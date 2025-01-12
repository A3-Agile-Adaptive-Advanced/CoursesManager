create
    definer = courses_manager@`%` procedure spAddresses_Add(IN p_country varchar(255), IN p_zipcode varchar(10),
                                                            IN p_city varchar(255), IN p_street varchar(255),
                                                            IN p_house_number varchar(10),
                                                            IN p_house_number_extension varchar(10),
                                                            IN p_created_at datetime, IN p_updated_at datetime,
                                                            OUT p_id int)
BEGIN
    INSERT INTO addresses (country, zipcode, city, street, house_number, house_number_extension, created_at, updated_at)
    VALUES (p_country, p_zipcode, p_city, p_street, p_house_number, p_house_number_extension, p_created_at, p_updated_at);

    SET p_id = LAST_INSERT_ID(); -- Return the generated ID
END;

