-- Simple Order Creation Test
-- This script tests if we can create orders manually

-- 1. First, ensure we have basic data
INSERT INTO "Categories" ("Id", "Name") VALUES 
(1, 'Липов'),
(2, 'Слънчогледов'),
(3, 'Билков')
ON CONFLICT ("Id") DO UPDATE SET "Name" = EXCLUDED."Name";

-- 2. Create a test user and beekeeper
DO $$
DECLARE
    user_id UUID;
    beekeeper_id UUID;
BEGIN
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
END $$;

-- 3. Create test honey
DO $$
DECLARE
    honey_id UUID;
    beekeeper_id UUID;
BEGIN
    -- Get beekeeper ID
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
END $$;

-- 4. Test order creation
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
        -- Create test order
        BEGIN
            INSERT INTO "Orders" (
                "UserId", "PhoneNumber", "CreatedOn", "Email", 
                "Address", "TotalPrice", "Status"
            ) VALUES (
                gen_random_uuid(),
                '+359888123456',
                NOW(),
                'test@example.com',
                'Test Address, Sofia',
                25.90,
                0
            ) RETURNING "Id" INTO order_id;
            
            test_result := 'SUCCESS: Order created with ID ' || order_id;
            
            -- Create test order item
            INSERT INTO "OrderItems" (
                "ProductId", "ProductName", "Quantity", "Price", "OrderId"
            ) VALUES (
                honey_id,
                'Тестов липов мед',
                1,
                25.90,
                order_id
            ) RETURNING "Id" INTO order_item_id;
            
            test_result := test_result || ', OrderItem created with ID ' || order_item_id;
            
            RAISE NOTICE '%', test_result;
            
        EXCEPTION WHEN OTHERS THEN
            test_result := 'ERROR: ' || SQLERRM;
            RAISE NOTICE '%', test_result;
        END;
    ELSE
        RAISE NOTICE 'ERROR: No active honeys found, cannot test order creation';
    END IF;
END $$;

-- 5. Check results
SELECT 'Test Results:' as Info;
SELECT 
    'Orders Count: ' || COUNT(*) as OrdersCount
FROM "Orders"
UNION ALL
SELECT 
    'OrderItems Count: ' || COUNT(*) as OrderItemsCount
FROM "OrderItems"
UNION ALL
SELECT 
    'Honeys Count: ' || COUNT(*) as HoneysCount
FROM "Honeys" 
WHERE "IsActive" = true
UNION ALL
SELECT 
    'Categories Count: ' || COUNT(*) as CategoriesCount
FROM "Categories";

-- 6. Show sample data
SELECT 'Sample Orders:' as Info;
SELECT 
    "Id",
    "Email",
    "PhoneNumber",
    "TotalPrice",
    "Status",
    "CreatedOn"
FROM "Orders"
ORDER BY "CreatedOn" DESC
LIMIT 3;
