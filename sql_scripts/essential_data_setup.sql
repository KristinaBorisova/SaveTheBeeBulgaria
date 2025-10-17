-- Essential Data Setup for Order System
-- This script creates the minimum required data for orders to work

-- 1. Create Categories (Honey Types) - Use simple IDs
INSERT INTO "Categories" ("Id", "Name") VALUES 
(1, 'Липов'),
(2, 'Слънчогледов'),
(3, 'Билков')
ON CONFLICT ("Id") DO UPDATE SET "Name" = EXCLUDED."Name";

-- 2. Create a simple user and beekeeper
DO $$
DECLARE
    user_id UUID;
    beekeeper_id UUID;
BEGIN
    -- Create or get user
    SELECT "Id" INTO user_id FROM "AspNetUsers" WHERE "Email" = 'test@example.com';
    
    IF user_id IS NULL THEN
        user_id := gen_random_uuid();
        INSERT INTO "AspNetUsers" (
            "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail",
            "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
            "FirstName", "LastName", "PhoneNumber", "PhoneNumberConfirmed",
            "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount"
        ) VALUES (
            user_id,
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
        );
    END IF;
    
    -- Create or get beekeeper
    SELECT "Id" INTO beekeeper_id FROM "Beekeepers" WHERE "UserId" = user_id;
    
    IF beekeeper_id IS NULL THEN
        beekeeper_id := gen_random_uuid();
        INSERT INTO "Beekeepers" (
            "Id", "PhoneNumber", "UserId", "HiveFarmPicturePaths"
        ) VALUES (
            beekeeper_id,
            '+359888123456',
            user_id,
            '/uploads/beekeeper-farm.jpg'
        );
    END IF;
    
    RAISE NOTICE 'User ID: %, Beekeeper ID: %', user_id, beekeeper_id;
END $$;

-- 3. Create sample honey products
DO $$
DECLARE
    honey_id UUID;
    beekeeper_id UUID;
BEGIN
    -- Get beekeeper ID
    SELECT "Id" INTO beekeeper_id FROM "Beekeepers" LIMIT 1;
    
    IF beekeeper_id IS NOT NULL THEN
        -- Create Липов мед
        honey_id := gen_random_uuid();
        INSERT INTO "Honeys" (
            "Id", "Title", "Origin", "Description", "ImageUrl", "Price", 
            "NetWeight", "CreatedOn", "YearMade", "IsActive", "CategoryId", 
            "BeekeeperId", "IsPromoted"
        ) VALUES (
            honey_id,
            'Липов мед от Северозападна България',
            'Враца, България',
            'Чист липов мед, събран от вековни липи в Северозападна България.',
            '/uploads/HoneyPictures/lipov-med-sample.jpg',
            25.90,
            500,
            NOW(),
            2024,
            true,
            1, -- Липов category
            beekeeper_id,
            true
        ) ON CONFLICT ("Id") DO NOTHING;
        
        -- Create Слънчогледов мед
        honey_id := gen_random_uuid();
        INSERT INTO "Honeys" (
            "Id", "Title", "Origin", "Description", "ImageUrl", "Price", 
            "NetWeight", "CreatedOn", "YearMade", "IsActive", "CategoryId", 
            "BeekeeperId", "IsPromoted"
        ) VALUES (
            honey_id,
            'Слънчогледов мед от Добруджа',
            'Добруджа, България',
            'Златист слънчогледов мед с богат вкус и аромат.',
            '/uploads/HoneyPictures/slunchogledov-med-sample.jpg',
            22.50,
            500,
            NOW(),
            2024,
            true,
            2, -- Слънчогледов category
            beekeeper_id,
            true
        ) ON CONFLICT ("Id") DO NOTHING;
        
        -- Create Билков мед
        honey_id := gen_random_uuid();
        INSERT INTO "Honeys" (
            "Id", "Title", "Origin", "Description", "ImageUrl", "Price", 
            "NetWeight", "CreatedOn", "YearMade", "IsActive", "CategoryId", 
            "BeekeeperId", "IsPromoted"
        ) VALUES (
            honey_id,
            'Билков мед от Родопите',
            'Родопи, България',
            'Многобилков мед от разнообразни планински билки в Родопите.',
            '/uploads/HoneyPictures/bilkov-med-sample.jpg',
            28.90,
            500,
            NOW(),
            2024,
            true,
            3, -- Билков category
            beekeeper_id,
            true
        ) ON CONFLICT ("Id") DO NOTHING;
        
        RAISE NOTICE 'Created sample honey products';
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
LIMIT 3;

-- 6. Test order creation manually
DO $$
DECLARE
    order_id UUID;
    order_item_id UUID;
    honey_id UUID;
    test_result TEXT;
BEGIN
    -- Get a honey ID
    SELECT "Id" INTO honey_id 
    FROM "Honeys" 
    WHERE "IsActive" = true 
    LIMIT 1;
    
    IF honey_id IS NOT NULL THEN
        -- Try to create test order
        BEGIN
            order_id := gen_random_uuid();
            
            INSERT INTO "Orders" (
                "Id", "UserId", "PhoneNumber", "CreatedOn", "Email", 
                "Address", "TotalPrice", "Status"
            ) VALUES (
                order_id,
                gen_random_uuid(),
                '+359888123456',
                NOW(),
                'test@example.com',
                'Test Address',
                25.90,
                0
            );
            
            test_result := 'SUCCESS: Order created with ID ' || order_id;
            
            -- Try to create test order item
            order_item_id := gen_random_uuid();
            
            INSERT INTO "OrderItems" (
                "Id", "ProductId", "ProductName", "Quantity", "Price", "OrderId"
            ) VALUES (
                order_item_id,
                honey_id,
                'Test Honey Product',
                1,
                25.90,
                order_id
            );
            
            test_result := test_result || ', OrderItem created with ID ' || order_item_id;
            
            -- Clean up test data
            DELETE FROM "OrderItems" WHERE "Id" = order_item_id;
            DELETE FROM "Orders" WHERE "Id" = order_id;
            
            test_result := test_result || ' (Test data cleaned up)';
            
        EXCEPTION WHEN OTHERS THEN
            test_result := 'ERROR: ' || SQLERRM;
        END;
    ELSE
        test_result := 'ERROR: No active honeys found, cannot test order creation';
    END IF;
    
    RAISE NOTICE '%', test_result;
END $$;

SELECT 'Setup Complete! Ready for order testing.' as Status;
