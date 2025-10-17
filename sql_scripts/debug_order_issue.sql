-- Debug Order Issue Script
-- This script helps troubleshoot why orders aren't being saved to PostgreSQL

-- 1. Check if Orders table exists and has correct structure
SELECT 'Checking Orders Table Structure...' as Step;
SELECT 
    column_name,
    data_type,
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_name = 'Orders' 
ORDER BY ordinal_position;

-- 2. Check if OrderItems table exists and has correct structure
SELECT 'Checking OrderItems Table Structure...' as Step;
SELECT 
    column_name,
    data_type,
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_name = 'OrderItems' 
ORDER BY ordinal_position;

-- 3. Check for any existing orders
SELECT 'Checking Existing Orders...' as Step;
SELECT 
    COUNT(*) as TotalOrders,
    MAX("CreatedOn") as LatestOrder,
    MIN("CreatedOn") as OldestOrder
FROM "Orders";

-- 4. Show sample orders if any exist
SELECT 'Sample Orders (if any):' as Step;
SELECT 
    "Id",
    "Email",
    "PhoneNumber",
    "TotalPrice",
    "Status",
    "CreatedOn"
FROM "Orders"
ORDER BY "CreatedOn" DESC
LIMIT 5;

-- 5. Check for any existing order items
SELECT 'Checking OrderItems...' as Step;
SELECT 
    COUNT(*) as TotalOrderItems,
    COUNT(DISTINCT "OrderId") as UniqueOrders
FROM "OrderItems";

-- 6. Check if we have the required data for order creation
SELECT 'Checking Required Data...' as Step;

-- Check categories
SELECT 
    'Categories Count: ' || COUNT(*) as CategoriesStatus
FROM "Categories"
WHERE "Name" IN ('Липов', 'Слънчогледов', 'Билков');

-- Check active honeys
SELECT 
    'Active Honeys Count: ' || COUNT(*) as HoneysStatus
FROM "Honeys" 
WHERE "IsActive" = true;

-- Check beekeepers
SELECT 
    'Beekeepers Count: ' || COUNT(*) as BeekeepersStatus
FROM "Beekeepers";

-- 7. Check for any constraints that might be preventing inserts
SELECT 'Checking Constraints...' as Step;
SELECT 
    tc.table_name,
    tc.constraint_name,
    tc.constraint_type,
    kcu.column_name
FROM information_schema.table_constraints AS tc
JOIN information_schema.key_column_usage AS kcu
    ON tc.constraint_name = kcu.constraint_name
    AND tc.table_schema = kcu.table_schema
WHERE tc.table_name IN ('Orders', 'OrderItems')
ORDER BY tc.table_name, tc.constraint_name;

-- 8. Check for any NOT NULL constraints that might be failing
SELECT 'Checking NOT NULL Constraints...' as Step;
SELECT 
    table_name,
    column_name,
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_name IN ('Orders', 'OrderItems') 
    AND is_nullable = 'NO'
    AND column_default IS NULL
ORDER BY table_name, column_name;

-- 9. Test manual order creation (this will help identify the issue)
SELECT 'Testing Manual Order Creation...' as Step;
DO $$
DECLARE
    order_id UUID;
    order_item_id UUID;
    honey_id UUID;
    beekeeper_id UUID;
    test_result TEXT;
BEGIN
    -- Get a sample honey ID
    SELECT "Id" INTO honey_id 
    FROM "Honeys" 
    WHERE "IsActive" = true 
    LIMIT 1;
    
    -- Get a beekeeper ID
    SELECT "Id" INTO beekeeper_id 
    FROM "Beekeepers" 
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

-- 10. Check application logs (simulation)
SELECT 'Next Steps for Debugging:' as Step;
SELECT '1. Check Railway application logs for error messages' as Step1
UNION ALL
SELECT '2. Verify form data is being submitted correctly' as Step2
UNION ALL
SELECT '3. Check if PlaceOrderFromHomepage method is being called' as Step3
UNION ALL
SELECT '4. Verify database connection is working' as Step4
UNION ALL
SELECT '5. Check for any validation errors' as Step5;
