-- Quick Setup for Email Functionality
-- This script ensures you have the minimum required data for orders and emails

-- 1. Ensure categories exist
INSERT INTO "Categories" ("Id", "Name") VALUES 
(1, 'Липов'),
(2, 'Слънчогледов'),
(3, 'Билков')
ON CONFLICT ("Id") DO UPDATE SET "Name" = EXCLUDED."Name";

-- 2. Create a test user and beekeeper if they don't exist
DO $$
DECLARE
    user_id UUID;
    beekeeper_id UUID;
    user_exists BOOLEAN;
BEGIN
    -- Check if test user exists
    SELECT EXISTS(SELECT 1 FROM "AspNetUsers" WHERE "Email" = 'test@example.com') INTO user_exists;
    
    IF NOT user_exists THEN
        -- Create test user
        INSERT INTO "AspNetUsers" (
            "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail",
            "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
            "FirstName", "LastName", "PhoneNumber", "PhoneNumberConfirmed",
            "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount"
        ) VALUES (
            gen_random_uuid(),
            'test.user',
            'TEST.USER', 
            'test@example.com',
            'TEST@EXAMPLE.COM',
            true,
            'AQAAAAEAACcQAAAAEExampleHash123456789',
            'SecurityStamp123456789',
            'ConcurrencyStamp123456789',
            'Test',
            'User',
            '+359888123456',
            true,
            false,
            true,
            0
        ) RETURNING "Id" INTO user_id;
        
        -- Create beekeeper
        INSERT INTO "Beekeepers" (
            "Id", "PhoneNumber", "UserId", "HiveFarmPicturePaths"
        ) VALUES (
            gen_random_uuid(),
            '+359888123456',
            user_id,
            '/uploads/beekeeper-farm.jpg'
        ) RETURNING "Id" INTO beekeeper_id;
        
        RAISE NOTICE 'Created test user: % and beekeeper: %', user_id, beekeeper_id;
    ELSE
        RAISE NOTICE 'Test user already exists';
    END IF;
END $$;

-- 3. Create test honey if none exist
DO $$
DECLARE
    honey_count INTEGER;
    honey_id UUID;
    beekeeper_id UUID;
BEGIN
    -- Check if we have any active honeys
    SELECT COUNT(*) INTO honey_count FROM "Honeys" WHERE "IsActive" = true;
    
    IF honey_count = 0 THEN
        -- Get a beekeeper ID
        SELECT "Id" INTO beekeeper_id 
        FROM "Beekeepers" 
        LIMIT 1;
        
        IF beekeeper_id IS NOT NULL THEN
            -- Create test honey
            INSERT INTO "Honeys" (
                "Id", "Title", "Origin", "Description", "ImageUrl", "Price", 
                "NetWeight", "CreatedOn", "YearMade", "IsActive", "CategoryId", 
                "BeekeeperId", "IsPromoted"
            ) VALUES (
                gen_random_uuid(),
                'Тестов липов мед',
                'София, България',
                'Тестов мед за проверка на функционалността',
                '/uploads/test-honey.jpg',
                25.90,
                500,
                NOW(),
                2024,
                true,
                1, -- Липов category
                beekeeper_id,
                true
            ) RETURNING "Id" INTO honey_id;
            
            RAISE NOTICE 'Created test honey: %', honey_id;
        END IF;
    ELSE
        RAISE NOTICE 'Active honeys already exist: %', honey_count;
    END IF;
END $$;

-- 4. Verify setup
SELECT 'Setup Verification:' as Info;
SELECT 
    'Categories: ' || COUNT(*) as CategoriesCount
FROM "Categories"
WHERE "Name" IN ('Липов', 'Слънчогледов', 'Билков')
UNION ALL
SELECT 
    'Active Honeys: ' || COUNT(*) as HoneysCount
FROM "Honeys" 
WHERE "IsActive" = true
UNION ALL
SELECT 
    'Beekeepers: ' || COUNT(*) as BeekeepersCount
FROM "Beekeepers"
UNION ALL
SELECT 
    'Users: ' || COUNT(*) as UsersCount
FROM "AspNetUsers";

-- 5. Show sample data
SELECT 'Sample Data:' as Info;
SELECT 
    'Categories:' as Type,
    "Name" as Value
FROM "Categories"
WHERE "Name" IN ('Липов', 'Слънчогледов', 'Билков')
UNION ALL
SELECT 
    'Honeys:' as Type,
    "Title" as Value
FROM "Honeys" 
WHERE "IsActive" = true
LIMIT 3
UNION ALL
SELECT 
    'Beekeepers:' as Type,
    u."FirstName" || ' ' || u."LastName" as Value
FROM "Beekeepers" b
JOIN "AspNetUsers" u ON b."UserId" = u."Id"
LIMIT 3;
