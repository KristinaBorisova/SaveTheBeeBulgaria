-- Test Order Form Data Script
-- This script verifies that all data needed for the order form exists

-- 1. Check if GetHoneyTypesAsync() will return data
SELECT 'Honey Types for Order Form:' as Info;
SELECT 
    c."Id",
    c."Name",
    CASE 
        WHEN c."Name" = 'Липов' THEN 25.90
        WHEN c."Name" = 'Слънчогледов' THEN 22.50
        WHEN c."Name" = 'Билков' THEN 28.90
        ELSE 0
    END as Price
FROM "Categories" c
WHERE c."Name" IN ('Липов', 'Слънчогледов', 'Билков')
ORDER BY c."Id";

-- 2. Check if GetBeekeepersByCategoryAsync() will return data
SELECT 'Beekeepers for Order Form:' as Info;
SELECT 
    b."Id",
    u."FirstName" || ' ' || u."LastName" as FullName,
    'България' as Location,
    b."PhoneNumber"
FROM "Beekeepers" b
JOIN "AspNetUsers" u ON b."UserId" = u."Id"
LIMIT 3;

-- 3. Check if we have active honeys for each category
SELECT 'Active Honeys by Category:' as Info;
SELECT 
    c."Name" as CategoryName,
    COUNT(h."Id") as HoneyCount,
    STRING_AGG(h."Title", ', ') as HoneyTitles
FROM "Categories" c
LEFT JOIN "Honeys" h ON c."Id" = h."CategoryId" AND h."IsActive" = true
WHERE c."Name" IN ('Липов', 'Слънчогледов', 'Билков')
GROUP BY c."Id", c."Name"
ORDER BY c."Id";

-- 4. Test order creation (simulation)
SELECT 'Order Creation Test:' as Info;
SELECT 
    'Sample Order Data:' as TestType,
    'FullName: Test Customer' as CustomerName,
    'Email: test@example.com' as Email,
    'Phone: +359888123456' as Phone,
    'Address: Test Address' as Address,
    'HoneyTypeId: 1' as HoneyTypeId,
    'Quantity: 2' as Quantity,
    'Total Price: 51.80 лв' as TotalPrice;

-- 5. Check email configuration (simulation)
SELECT 'Email Configuration Test:' as Info;
SELECT 
    'SMTP Server: smtp.gmail.com' as SmtpServer,
    'Port: 587' as Port,
    'Username: savethebeebulgaria@gmail.com' as Username,
    'Admin Email: savethebeebulgaria@gmail.com' as AdminEmail;

-- 6. Final verification
SELECT 'Order Form Readiness Check:' as Info;
SELECT 
    CASE 
        WHEN (SELECT COUNT(*) FROM "Categories" WHERE "Name" IN ('Липов', 'Слънчогледов', 'Билков')) = 3 
        THEN '✅ Categories: OK' 
        ELSE '❌ Categories: Missing' 
    END as CategoriesStatus
UNION ALL
SELECT 
    CASE 
        WHEN (SELECT COUNT(*) FROM "Honeys" WHERE "IsActive" = true AND "IsPromoted" = true) > 0 
        THEN '✅ Promoted Honeys: OK' 
        ELSE '❌ Promoted Honeys: Missing' 
    END as HoneysStatus
UNION ALL
SELECT 
    CASE 
        WHEN (SELECT COUNT(*) FROM "Beekeepers") > 0 
        THEN '✅ Beekeepers: OK' 
        ELSE '❌ Beekeepers: Missing' 
    END as BeekeepersStatus
UNION ALL
SELECT 
    CASE 
        WHEN (SELECT COUNT(*) FROM "Honeys" WHERE "IsActive" = true) > 0 
        THEN '✅ Active Honeys: OK' 
        ELSE '❌ Active Honeys: Missing' 
    END as ActiveHoneysStatus;
