-- Create or update Ивайло Борисов as a real beekeeper in the database
-- This script will create a user and beekeeper record for Ивайло Борисов

-- First, check if Ивайло Борисов already exists
SELECT 'Checking for existing Ивайло Борисов...' as Info;

SELECT 
    u."Id" as UserId,
    u."FirstName",
    u."LastName", 
    u."Email",
    b."Id" as BeekeeperId,
    b."PhoneNumber"
FROM "AspNetUsers" u
LEFT JOIN "Beekeepers" b ON u."Id" = b."UserId"
WHERE u."FirstName" = 'Ивайло' AND u."LastName" = 'Борисов';

-- Create Ивайло Борисов if he doesn't exist
DO $$
DECLARE
    user_id UUID;
    beekeeper_id UUID;
BEGIN
    -- Check if user already exists
    SELECT "Id" INTO user_id 
    FROM "AspNetUsers" 
    WHERE "FirstName" = 'Ивайло' AND "LastName" = 'Борисов';
    
    IF user_id IS NULL THEN
        -- Create the user
        INSERT INTO "AspNetUsers" (
            "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail",
            "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
            "FirstName", "LastName", "PhoneNumber", "PhoneNumberConfirmed",
            "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount"
        ) VALUES (
            gen_random_uuid(),
            'ivaylo.borisov',
            'IVAYLO.BORISOV', 
            'ivaylo.borisov@savethebeebulgaria.com',
            'IVAYLO.BORISOV@SAVETHEBEEBULGARIA.COM',
            true,
            'AQAAAAEAACcQAAAAEExampleHash123456789', -- This is a placeholder hash
            'SecurityStamp123456789',
            'ConcurrencyStamp123456789',
            'Ивайло',
            'Борисов',
            '+359886612263',
            true,
            false,
            true,
            0
        ) RETURNING "Id" INTO user_id;
        
        RAISE NOTICE 'Created user Ивайло Борисов with ID: %', user_id;
    ELSE
        RAISE NOTICE 'User Ивайло Борисов already exists with ID: %', user_id;
    END IF;
    
    -- Check if beekeeper record exists
    SELECT "Id" INTO beekeeper_id 
    FROM "Beekeepers" 
    WHERE "UserId" = user_id;
    
    IF beekeeper_id IS NULL THEN
        -- Create the beekeeper record
        INSERT INTO "Beekeepers" (
            "Id", "PhoneNumber", "UserId", "HiveFarmPicturePaths"
        ) VALUES (
            gen_random_uuid(),
            '+359886612263',
            user_id,
            '/uploads/beekeeper-farm-ivaylo.jpg'
        ) RETURNING "Id" INTO beekeeper_id;
        
        RAISE NOTICE 'Created beekeeper record for Ивайло Борисов with ID: %', beekeeper_id;
    ELSE
        RAISE NOTICE 'Beekeeper record for Ивайло Борисов already exists with ID: %', beekeeper_id;
    END IF;
END $$;

-- Verify the creation
SELECT 'Final verification:' as Info;

SELECT 
    u."Id" as UserId,
    u."FirstName",
    u."LastName", 
    u."Email",
    b."Id" as BeekeeperId,
    b."PhoneNumber"
FROM "AspNetUsers" u
LEFT JOIN "Beekeepers" b ON u."Id" = b."UserId"
WHERE u."FirstName" = 'Ивайло' AND u."LastName" = 'Борисов';

-- Show the beekeeper ID that should be used in the code
SELECT 
    'Use this BeekeeperId in your code:' as Info,
    b."Id"::text as BeekeeperId
FROM "AspNetUsers" u
JOIN "Beekeepers" b ON u."Id" = b."UserId"
WHERE u."FirstName" = 'Ивайло' AND u."LastName" = 'Борисов';
