create
    definer = courses_manager@`%` procedure spAddresses_Update(IN p_id int, IN p_country varchar(255),
                                                               IN p_zipcode varchar(10), IN p_city varchar(255),
                                                               IN p_street varchar(255), IN p_house_number varchar(10),
                                                               IN p_house_number_extension varchar(10),
                                                               IN p_updated_at datetime)
BEGIN
    UPDATE addresses
    SET country = p_country, zipcode = p_zipcode, city = p_city, street = p_street,
        house_number = p_house_number, house_number_extension = p_house_number_extension, updated_at = p_updated_at
    WHERE id = p_id;
END;

