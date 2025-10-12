-- PostgreSQL Script to test order functionality
-- Run this in your Railway PostgreSQL database

-- ============================================
-- STEP 1: Verify Orders table structure
-- ============================================
SELECT 
    column_name,
    data_type,
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_name = 'Orders' 
ORDER BY ordinal_position;

-- ============================================
-- STEP 2: Create a test order manually
-- ============================================
INSERT INTO "Orders" (
    "Id",
    "UserId",
    "PhoneNumber",
    "CreatedOn",
    "Email",
    "Address",
    "TotalPrice",
    "Status"
) VALUES (
    gen_random_uuid(),
    gen_random_uuid(), -- Temporary user ID for guest orders
    '+359888999999',
    NOW(),
    'test.customer@example.com',
    'Sofia, Bulgaria, Test Address 123',
    25.50,
    'Обработван'
);

-- ============================================
-- STEP 3: Verify the order was created
-- ============================================
SELECT 
    "Id",
    "Email",
    "PhoneNumber",
    "Address",
    "TotalPrice",
    "Status",
    "CreatedOn"
FROM "Orders" 
WHERE "Email" = 'test.customer@example.com'
ORDER BY "CreatedOn" DESC;

-- ============================================
-- STEP 4: Create OrderItems for the test order
-- ============================================
-- First, get the order ID from the previous query
-- Replace 'ORDER_ID_HERE' with the actual order ID
INSERT INTO "OrderItems" (
    "Id",
    "ProductId",
    "ProductName",
    "Quantity",
    "Price",
    "OrderId"
) VALUES (
    gen_random_uuid(),
    gen_random_uuid(), -- Product ID
    'Premium Linden Honey from Vratsa',
    2,
    12.75,
    'ORDER_ID_HERE' -- Replace with actual order ID
);

-- ============================================
-- STEP 5: Verify complete order with items
-- ============================================
SELECT 
    o."Id" as "OrderId",
    o."Email",
    o."PhoneNumber",
    o."Address",
    o."TotalPrice",
    o."Status",
    o."CreatedOn",
    oi."ProductName",
    oi."Quantity",
    oi."Price"
FROM "Orders" o
LEFT JOIN "OrderItems" oi ON o."Id" = oi."OrderId"
WHERE o."Email" = 'test.customer@example.com'
ORDER BY o."CreatedOn" DESC;

-- ============================================
-- STEP 6: Admin queries for order management
-- ============================================
-- View all orders (admin function)
SELECT 
    "Id",
    "Email",
    "PhoneNumber",
    "Address",
    "TotalPrice",
    "Status",
    "CreatedOn"
FROM "Orders" 
ORDER BY "CreatedOn" DESC;

-- Count orders by status
SELECT 
    "Status",
    COUNT(*) as "Count"
FROM "Orders" 
GROUP BY "Status"
ORDER BY "Count" DESC;

-- ============================================
-- STEP 7: Update order status (admin function)
-- ============================================
-- Update order status example
-- UPDATE "Orders" 
-- SET "Status" = 'Приготвен' 
-- WHERE "Id" = 'ORDER_ID_HERE';

-- ============================================
-- STEP 8: Clean up test data (optional)
-- ============================================
-- DELETE FROM "OrderItems" WHERE "OrderId" IN (
--     SELECT "Id" FROM "Orders" WHERE "Email" = 'test.customer@example.com'
-- );
-- DELETE FROM "Orders" WHERE "Email" = 'test.customer@example.com';
