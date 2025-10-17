-- Fix homepage honey images by ensuring there are promoted honeys with valid images
-- This script will create sample promoted honeys if none exist

-- First, check current state
SELECT 'Current State:' as Info;
SELECT 
    COUNT(*) as TotalHoneys,
    COUNT(CASE WHEN "IsActive" = true THEN 1 END) as ActiveHoneys,
    COUNT(CASE WHEN "IsActive" = true AND "IsPromoted" = true THEN 1 END) as PromotedHoneys
FROM "Honeys";

-- Ensure we have categories
INSERT INTO "Categories" ("Id", "Name") VALUES 
(1, 'Липов'),
(2, 'Слънчогледов'),
(3, 'Билков'),
(4, 'Акациев'),
(5, 'Горски')
ON CONFLICT ("Id") DO UPDATE SET "Name" = EXCLUDED."Name";

-- Get or create a beekeeper for the sample honeys
DO $$
DECLARE
    beekeeper_id UUID;
    user_id UUID;
BEGIN
    -- Try to get existing beekeeper
    SELECT b."Id" INTO beekeeper_id 
    FROM "Beekeepers" b
    JOIN "AspNetUsers" u ON b."UserId" = u."Id"
    WHERE u."FirstName" = 'Ивайло' AND u."LastName" = 'Борисов'
    LIMIT 1;
    
    -- If no beekeeper found, create one
    IF beekeeper_id IS NULL THEN
        -- Create user first
        INSERT INTO "AspNetUsers" (
            "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail",
            "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
            "FirstName", "LastName", "PhoneNumber", "PhoneNumberConfirmed",
            "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount"
        ) VALUES (
            gen_random_uuid(),
            'sample.beekeeper',
            'SAMPLE.BEEKEEPER', 
            'sample@savethebeebulgaria.com',
            'SAMPLE@SAVETHEBEEBULGARIA.COM',
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
        
        -- Create beekeeper
        INSERT INTO "Beekeepers" (
            "Id", "PhoneNumber", "UserId", "HiveFarmPicturePaths"
        ) VALUES (
            gen_random_uuid(),
            '+359888123456',
            user_id,
            '/uploads/beekeeper-farm.jpg'
        ) RETURNING "Id" INTO beekeeper_id;
    END IF;
    
    RAISE NOTICE 'Using beekeeper ID: %', beekeeper_id;
END $$;

-- Get the beekeeper ID for sample honeys
SELECT b."Id" as BeekeeperId INTO TEMP TABLE temp_beekeeper FROM "Beekeepers" b LIMIT 1;

-- Create sample promoted honeys if none exist
INSERT INTO "Honeys" (
    "Id", "Title", "Origin", "Description", "ImageUrl", "Price", 
    "NetWeight", "CreatedOn", "YearMade", "IsActive", "CategoryId", 
    "BeekeeperId", "IsPromoted"
)
SELECT 
    gen_random_uuid(),
    'Липов мед от Северозападна България',
    'Враца, България',
    'Чист липов мед, събран от вековни липи в Северозападна България. Има нежен аромат и леко жълтеникав цвят.',
    '/uploads/HoneyPictures/lipov-med-sample.jpg',
    25.90,
    500,
    NOW(),
    2024,
    true,
    1, -- Липов category
    (SELECT "Id" FROM temp_beekeeper LIMIT 1),
    true
WHERE NOT EXISTS (
    SELECT 1 FROM "Honeys" 
    WHERE "IsActive" = true AND "IsPromoted" = true
);

INSERT INTO "Honeys" (
    "Id", "Title", "Origin", "Description", "ImageUrl", "Price", 
    "NetWeight", "CreatedOn", "YearMade", "IsActive", "CategoryId", 
    "BeekeeperId", "IsPromoted"
)
SELECT 
    gen_random_uuid(),
    'Слънчогледов мед',
    'Добруджа, България',
    'Златист слънчогледов мед с богат вкус и аромат. Събран от слънчогледови полета в Добруджа.',
    '/uploads/HoneyPictures/slunchogledov-med-sample.jpg',
    22.50,
    500,
    NOW(),
    2024,
    true,
    2, -- Слънчогледов category
    (SELECT "Id" FROM temp_beekeeper LIMIT 1),
    true
WHERE NOT EXISTS (
    SELECT 1 FROM "Honeys" 
    WHERE "IsActive" = true AND "IsPromoted" = true
);

INSERT INTO "Honeys" (
    "Id", "Title", "Origin", "Description", "ImageUrl", "Price", 
    "NetWeight", "CreatedOn", "YearMade", "IsActive", "CategoryId", 
    "BeekeeperId", "IsPromoted"
)
SELECT 
    gen_random_uuid(),
    'Билков мед',
    'Родопи, България',
    'Многобилков мед от разнообразни планински билки в Родопите. Има сложен аромат и богат вкус.',
    '/uploads/HoneyPictures/bilkov-med-sample.jpg',
    28.90,
    500,
    NOW(),
    2024,
    true,
    3, -- Билков category
    (SELECT "Id" FROM temp_beekeeper LIMIT 1),
    true
WHERE NOT EXISTS (
    SELECT 1 FROM "Honeys" 
    WHERE "IsActive" = true AND "IsPromoted" = true
);

-- Clean up temp table
DROP TABLE temp_beekeeper;

-- Verify the results
SELECT 'Final State:' as Info;
SELECT 
    COUNT(*) as TotalHoneys,
    COUNT(CASE WHEN "IsActive" = true THEN 1 END) as ActiveHoneys,
    COUNT(CASE WHEN "IsActive" = true AND "IsPromoted" = true THEN 1 END) as PromotedHoneys
FROM "Honeys";

-- Show the promoted honeys that will appear on homepage
SELECT 'Promoted Honeys for Homepage:' as Info;
SELECT 
    "Title",
    "Origin",
    "ImageUrl",
    "Price",
    "IsActive",
    "IsPromoted"
FROM "Honeys" 
WHERE "IsActive" = true AND "IsPromoted" = true
ORDER BY "CreatedOn" DESC;
