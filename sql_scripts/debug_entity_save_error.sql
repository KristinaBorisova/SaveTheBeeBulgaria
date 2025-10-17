-- Debug Entity Save Error Script
-- This script will help identify what's causing the "An error occurred while saving the entity changes" error

-- 1. Check if Orders table exists and has correct structure
SELECT 'Orders Table Structure:' as Info;
SELECT 
    column_name,
    data_type,
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_name = 'Orders' 
ORDER BY ordinal_position;

-- 2. Check if OrderItems table exists and has correct structure
SELECT 'OrderItems Table Structure:' as Info;
SELECT 
    column_name,
    data_type,
    is_nullable,
    column_default
FROM information_schema.columns 
WHERE table_name = 'OrderItems' 
ORDER BY ordinal_position;

-- 3. Check foreign key constraints
SELECT 'Foreign Key Constraints:' as Info;
SELECT 
    tc.table_name,
    tc.constraint_name,
    tc.constraint_type,
    kcu.column_name,
    ccu.table_name AS foreign_table_name,
    ccu.column_name AS foreign_column_name
FROM information_schema.table_constraints AS tc
JOIN information_schema.key_column_usage AS kcu
    ON tc.constraint_name = kcu.constraint_name
    AND tc.table_schema = kcu.table_schema
JOIN information_schema.constraint_column_usage AS ccu
    ON ccu.constraint_name = tc.constraint_name
    AND ccu.table_schema = tc.table_schema
WHERE tc.constraint_type = 'FOREIGN KEY' 
    AND tc.table_name IN ('Orders', 'OrderItems');

-- 4. Check if we have the required data for order creation
SELECT 'Required Data Check:' as Info;

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

-- 5. Check for any existing orders to see the structure
SELECT 'Existing Orders Sample:' as Info;
SELECT 
    "Id",
    "UserId",
    "PhoneNumber",
    "Email",
    "Address",
    "TotalPrice",
    "Status",
    "CreatedOn"
FROM "Orders"
ORDER BY "CreatedOn" DESC
LIMIT 3;

-- 6. Check for any existing order items
SELECT 'Existing Order Items Sample:' as Info;
SELECT 
    "Id",
    "ProductId",
    "ProductName",
    "Quantity",
    "Price",
    "OrderId"
FROM "OrderItems"
ORDER BY "Id" DESC
LIMIT 3;

-- 7. Test data that would be used in order creation
SELECT 'Test Order Data:' as Info;
SELECT 
    'Sample Order ID: ' || gen_random_uuid()::text as OrderId,
    'Sample User ID: ' || gen_random_uuid()::text as UserId,
    'Sample Phone: +359888123456' as Phone,
    'Sample Email: test@example.com' as Email,
    'Sample Address: Test Address' as Address,
    'Sample Total: 25.90' as TotalPrice;

-- 8. Check if there are any NOT NULL constraints that might be failing
SELECT 'NOT NULL Constraints:' as Info;
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

-- 9. Check for any unique constraints that might be violated
SELECT 'Unique Constraints:' as Info;
SELECT 
    tc.table_name,
    tc.constraint_name,
    tc.constraint_type,
    kcu.column_name
FROM information_schema.table_constraints AS tc
JOIN information_schema.key_column_usage AS kcu
    ON tc.constraint_name = kcu.constraint_name
    AND tc.table_schema = kcu.table_schema
WHERE tc.constraint_type IN ('UNIQUE', 'PRIMARY KEY')
    AND tc.table_name IN ('Orders', 'OrderItems')
ORDER BY tc.table_name, tc.constraint_name;
