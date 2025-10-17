-- Ensure basic data exists for honey creation
-- This script ensures all required data exists in the database

-- 1. Ensure categories exist with Bulgarian names
INSERT INTO "Categories" ("Id", "Name") VALUES 
(1, 'Липов'),
(2, 'Слънчогледов'),
(3, 'Билков'),
(4, 'Акациев'),
(5, 'Горски')
ON CONFLICT ("Id") DO UPDATE SET "Name" = EXCLUDED."Name";

-- 2. Check if we have any users
SELECT 'Users count:' as Info, COUNT(*) as Count FROM "AspNetUsers";

-- 3. Check if we have any beekeepers
SELECT 'Beekeepers count:' as Info, COUNT(*) as Count FROM "Beekeepers";

-- 4. If no beekeepers exist, create a sample beekeeper
DO $$
DECLARE
    user_id UUID;
    beekeeper_id UUID;
BEGIN
    -- Check if we have any users
    SELECT "Id" INTO user_id FROM "AspNetUsers" LIMIT 1;
    
    IF user_id IS NULL THEN
        -- Create a sample user
        INSERT INTO "AspNetUsers" (
            "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail",
            "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
            "FirstName", "LastName", "PhoneNumber", "PhoneNumberConfirmed",
            "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount"
        ) VALUES (
            gen_random_uuid(),
            'sample_beekeeper',
            'SAMPLE_BEEKEEPER', 
            'sample@example.com',
            'SAMPLE@EXAMPLE.COM',
            true,
            'AQAAAAEAACcQAAAAEExampleHash123456789',
            'SecurityStamp123456789',
            'ConcurrencyStamp123456789',
            'Sample',
            'Beekeeper',
            '+359888123456',
            true,
            false,
            true,
            0
        ) RETURNING "Id" INTO user_id;
    END IF;
    
    -- Check if we have any beekeepers
    SELECT "Id" INTO beekeeper_id FROM "Beekeepers" WHERE "UserId" = user_id;
    
    IF beekeeper_id IS NULL THEN
        -- Create a beekeeper for the user
        INSERT INTO "Beekeepers" (
            "Id", "PhoneNumber", "UserId", "HiveFarmPicturePaths"
        ) VALUES (
            gen_random_uuid(),
            '+359888123456',
            user_id,
            '/uploads/beekeeper-farm.jpg'
        ) RETURNING "Id" INTO beekeeper_id;
    END IF;
END $$;

-- 5. Verify data
SELECT 'Categories:' as Info;
SELECT "Id", "Name" FROM "Categories" ORDER BY "Id";

SELECT 'Users:' as Info;
SELECT "Id", "UserName", "Email" FROM "AspNetUsers" LIMIT 3;

SELECT 'Beekeepers:' as Info;
SELECT "Id", "UserId", "PhoneNumber" FROM "Beekeepers" LIMIT 3;

-- 6. Check if we have any active honeys
SELECT 'Active Honeys:' as Info, COUNT(*) as Count FROM "Honeys" WHERE "IsActive" = true;
