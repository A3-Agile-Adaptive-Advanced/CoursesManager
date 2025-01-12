CREATE PROCEDURE spLocations_Insert(
    IN p_name varchar(255),
    IN p_address_id INT,
    OUT p_id int
)
BEGIN
    DECLARE txn_started_by_me BOOLEAN DEFAULT FALSE;

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        IF txn_started_by_me THEN
            ROLLBACK;
        END IF;

        RESIGNAL;
    END;

    -- Check if a transaction is already active
    IF @@autocommit = 1 THEN
        SET txn_started_by_me = TRUE;
        START TRANSACTION;
    END IF;

    INSERT INTO locations (name, address_id, created_at, updated_at)
    VALUES (p_name, p_address_id, NOW(), NOW());

    IF txn_started_by_me THEN
        COMMIT;
    END IF;

    SET p_id = LAST_INSERT_ID();
END;