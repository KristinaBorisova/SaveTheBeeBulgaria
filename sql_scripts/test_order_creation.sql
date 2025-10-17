-- Test Order Creation Script
-- This script tests if we can create an order manually to verify the database constraints

-- 1. First, ensure we have the required data
-- Check if we have categories
SELECT 'Checking Categories...' as Step;
SELECT COUNT(*) as CategoryCount FROM "Categories" WHERE "Name" IN ('Липов', 'Слънчогледов', 'Билков');

-- Check if we have active honeys
SELECT 'Checking Active Honeys...' as Step;
SELECT COUNT(*) as HoneyCount FROM "Honeys" WHERE "IsActive" = true;

-- 2. Test order creation manually
DO $$
DECLARE
    order_id UUID;
    order_item_id UUID;
    honey_id UUID;
    beekeeper_id UUID;
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
        -- Create test order
        order_id := gen_random_uuid();
        
        INSERT INTO "Orders" (
            "Id", "UserId", "PhoneNumber", "CreatedOn", "Email", 
            "Address", "TotalPrice", "Status"
        ) VALUES (
            order_id,
            gen_random_uuid(), -- Random user ID for guest order
            '+359888123456',
            NOW(),
            'test@example.com',
            'Test Address, Sofia, Bulgaria',
            25.90,
            0 -- OrderStatus.Обработван
        );
        
        RAISE NOTICE 'Order created successfully with ID: %', order_id;
        
        -- Create test order item
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
        
        RAISE NOTICE 'OrderItem created successfully with ID: %', order_item_id;
        
        -- Clean up test data
        DELETE FROM "OrderItems" WHERE "Id" = order_item_id;
        DELETE FROM "Orders" WHERE "Id" = order_id;
        
        RAISE NOTICE 'Test data cleaned up successfully';
        
    ELSE
        RAISE NOTICE 'No active honeys found, cannot test order creation';
    END IF;
END $$;

-- 3. Check the results
SELECT 'Test Results:' as Info;
SELECT 
    'Orders Table: ' || COUNT(*) as OrdersCount
FROM "Orders"
UNION ALL
SELECT 
    'OrderItems Table: ' || COUNT(*) as OrderItemsCount
FROM "OrderItems";

-- 4. Show sample data structure
SELECT 'Sample Order Structure:' as Info;
SELECT 
    'Order fields: Id, UserId, PhoneNumber, CreatedOn, Email, Address, TotalPrice, Status' as OrderFields
UNION ALL
SELECT 
    'OrderItem fields: Id, ProductId, ProductName, Quantity, Price, OrderId' as OrderItemFields;
