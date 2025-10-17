-- Debug order form issues
-- This script will help identify what's causing the order creation error

-- Check if we have honey types (categories) available
SELECT 'Honey Types (Categories):' as Info;
SELECT "Id", "Name" FROM "Categories" ORDER BY "Id";

-- Check if we have active honeys
SELECT 'Active Honeys:' as Info;
SELECT 
    "Id",
    "Title", 
    "CategoryId",
    "IsActive",
    "IsPromoted",
    "Price"
FROM "Honeys" 
WHERE "IsActive" = true
ORDER BY "CreatedOn" DESC
LIMIT 10;

-- Check if we have beekeepers
SELECT 'Beekeepers:' as Info;
SELECT 
    b."Id" as BeekeeperId,
    u."FirstName",
    u."LastName",
    b."PhoneNumber"
FROM "Beekeepers" b
JOIN "AspNetUsers" u ON b."UserId" = u."Id"
LIMIT 5;

-- Check if we have any orders (to see if the table structure is correct)
SELECT 'Existing Orders:' as Info;
SELECT COUNT(*) as OrderCount FROM "Orders";

-- Check OrderItems table
SELECT 'Order Items:' as Info;
SELECT COUNT(*) as OrderItemCount FROM "OrderItems";

-- Check if there are any constraint issues
SELECT 'Table Constraints:' as Info;
SELECT 
    tc.table_name,
    tc.constraint_name,
    tc.constraint_type
FROM information_schema.table_constraints tc
WHERE tc.table_name IN ('Orders', 'OrderItems')
ORDER BY tc.table_name, tc.constraint_type;
