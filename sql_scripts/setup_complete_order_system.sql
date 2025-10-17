-- Complete Order System Setup Script
-- This script ensures all required data exists for the order form to work properly

-- 1. Create Categories (Honey Types)
INSERT INTO "Categories" ("Id", "Name") VALUES 
(1, 'Липов'),
(2, 'Слънчогледов'),
(3, 'Билков'),
(4, 'Акациев'),
(5, 'Горски')
ON CONFLICT ("Id") DO UPDATE SET "Name" = EXCLUDED."Name";

-- 2. Create a sample user for beekeepers
DO $$
DECLARE
    user_id UUID;
    beekeeper_id UUID;
BEGIN
    -- Check if sample user exists
    SELECT "Id" INTO user_id 
    FROM "AspNetUsers" 
    WHERE "Email" = 'sample.beekeeper@savethebeebulgaria.com';
    
    IF user_id IS NULL THEN
        -- Create sample user
        INSERT INTO "AspNetUsers" (
            "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail",
            "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
            "FirstName", "LastName", "PhoneNumber", "PhoneNumberConfirmed",
            "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount"
        ) VALUES (
            gen_random_uuid(),
            'sample.beekeeper',
            'SAMPLE.BEEKEEPER', 
            'sample.beekeeper@savethebeebulgaria.com',
            'SAMPLE.BEEKEEPER@SAVETHEBEEBULGARIA.COM',
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
        
        RAISE NOTICE 'Created sample user with ID: %', user_id;
    ELSE
        RAISE NOTICE 'Sample user already exists with ID: %', user_id;
    END IF;
    
    -- Check if beekeeper record exists
    SELECT "Id" INTO beekeeper_id 
    FROM "Beekeepers" 
    WHERE "UserId" = user_id;
    
    IF beekeeper_id IS NULL THEN
        -- Create beekeeper record
        INSERT INTO "Beekeepers" (
            "Id", "PhoneNumber", "UserId", "HiveFarmPicturePaths"
        ) VALUES (
            gen_random_uuid(),
            '+359888123456',
            user_id,
            '/uploads/beekeeper-farm.jpg'
        ) RETURNING "Id" INTO beekeeper_id;
        
        RAISE NOTICE 'Created beekeeper record with ID: %', beekeeper_id;
    ELSE
        RAISE NOTICE 'Beekeeper record already exists with ID: %', beekeeper_id;
    END IF;
END $$;

-- 3. Create sample honey products for each category
DO $$
DECLARE
    beekeeper_id UUID;
    honey_id UUID;
BEGIN
    -- Get the beekeeper ID
    SELECT b."Id" INTO beekeeper_id 
    FROM "Beekeepers" b
    JOIN "AspNetUsers" u ON b."UserId" = u."Id"
    WHERE u."Email" = 'sample.beekeeper@savethebeebulgaria.com'
    LIMIT 1;
    
    IF beekeeper_id IS NOT NULL THEN
        -- Create Липов мед
        INSERT INTO "Honeys" (
            "Id", "Title", "Origin", "Description", "ImageUrl", "Price", 
            "NetWeight", "CreatedOn", "YearMade", "IsActive", "CategoryId", 
            "BeekeeperId", "IsPromoted"
        ) VALUES (
            gen_random_uuid(),
            'Липов мед от Северозападна България',
            'Враца, България',
            'Чист липов мед, събран от вековни липи в Северозападна България. Има нежен аромат и леко жълтеникав цвят. Произведен по традиционни методи от местни пчелари.',
            '/uploads/HoneyPictures/lipov-med-sample.jpg',
            25.90,
            500,
            NOW(),
            2024,
            true,
            1, -- Липов category
            beekeeper_id,
            true
        ) ON CONFLICT DO NOTHING;
        
        -- Create Слънчогледов мед
        INSERT INTO "Honeys" (
            "Id", "Title", "Origin", "Description", "ImageUrl", "Price", 
            "NetWeight", "CreatedOn", "YearMade", "IsActive", "CategoryId", 
            "BeekeeperId", "IsPromoted"
        ) VALUES (
            gen_random_uuid(),
            'Слънчогледов мед от Добруджа',
            'Добруджа, България',
            'Златист слънчогледов мед с богат вкус и аромат. Събран от слънчогледови полета в Добруджа. Идеален за захарене и готвене.',
            '/uploads/HoneyPictures/slunchogledov-med-sample.jpg',
            22.50,
            500,
            NOW(),
            2024,
            true,
            2, -- Слънчогледов category
            beekeeper_id,
            true
        ) ON CONFLICT DO NOTHING;
        
        -- Create Билков мед
        INSERT INTO "Honeys" (
            "Id", "Title", "Origin", "Description", "ImageUrl", "Price", 
            "NetWeight", "CreatedOn", "YearMade", "IsActive", "CategoryId", 
            "BeekeeperId", "IsPromoted"
        ) VALUES (
            gen_random_uuid(),
            'Билков мед от Родопите',
            'Родопи, България',
            'Многобилков мед от разнообразни планински билки в Родопите. Има сложен аромат и богат вкус. Съдържа нектар от над 20 вида билки.',
            '/uploads/HoneyPictures/bilkov-med-sample.jpg',
            28.90,
            500,
            NOW(),
            2024,
            true,
            3, -- Билков category
            beekeeper_id,
            true
        ) ON CONFLICT DO NOTHING;
        
        RAISE NOTICE 'Created sample honey products for beekeeper: %', beekeeper_id;
    ELSE
        RAISE NOTICE 'No beekeeper found, skipping honey creation';
    END IF;
END $$;

-- 4. Verify the setup
SELECT 'Setup Verification:' as Info;

-- Check categories
SELECT 'Categories:' as Info;
SELECT "Id", "Name" FROM "Categories" ORDER BY "Id";

-- Check beekeepers
SELECT 'Beekeepers:' as Info;
SELECT 
    b."Id" as BeekeeperId,
    u."FirstName",
    u."LastName",
    u."Email",
    b."PhoneNumber"
FROM "Beekeepers" b
JOIN "AspNetUsers" u ON b."UserId" = u."Id";

-- Check honey products
SELECT 'Honey Products:' as Info;
SELECT 
    h."Id",
    h."Title",
    h."Origin",
    h."Price",
    h."IsActive",
    h."IsPromoted",
    c."Name" as CategoryName
FROM "Honeys" h
JOIN "Categories" c ON h."CategoryId" = c."Id"
WHERE h."IsActive" = true
ORDER BY h."CreatedOn" DESC;

-- Check if we have promoted honeys for homepage
SELECT 'Promoted Honeys for Homepage:' as Info;
SELECT COUNT(*) as PromotedCount
FROM "Honeys" 
WHERE "IsActive" = true AND "IsPromoted" = true;

-- 5. Test data for order form
SELECT 'Order Form Test Data:' as Info;
SELECT 
    c."Id" as CategoryId,
    c."Name" as CategoryName,
    h."Title" as HoneyTitle,
    h."Price"
FROM "Categories" c
LEFT JOIN "Honeys" h ON c."Id" = h."CategoryId" AND h."IsActive" = true
WHERE c."Name" IN ('Липов', 'Слънчогледов', 'Билков')
ORDER BY c."Id";

-- 6. Check existing orders (if any)
SELECT 'Existing Orders:' as Info;
SELECT COUNT(*) as OrderCount FROM "Orders";

-- 7. Final status
SELECT 'Setup Complete!' as Status;
SELECT 
    'Categories: ' || COUNT(*) as CategoriesCount
FROM "Categories"
UNION ALL
SELECT 
    'Active Honeys: ' || COUNT(*) as HoneysCount
FROM "Honeys" 
WHERE "IsActive" = true
UNION ALL
SELECT 
    'Promoted Honeys: ' || COUNT(*) as PromotedCount
FROM "Honeys" 
WHERE "IsActive" = true AND "IsPromoted" = true
UNION ALL
SELECT 
    'Beekeepers: ' || COUNT(*) as BeekeepersCount
FROM "Beekeepers";
