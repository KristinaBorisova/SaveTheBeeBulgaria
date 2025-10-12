-- ONE-SHOT SCRIPT: Complete sample data insertion
-- Run this entire script in Railway PostgreSQL console

-- ============================================
-- Create sample user with specific UUID
-- ============================================
INSERT INTO "AspNetUsers" (
    "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail",
    "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
    "FirstName", "LastName", "PhoneNumber", "PhoneNumberConfirmed",
    "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount"
) VALUES (
    'a1b2c3d4-e5f6-7890-abcd-ef1234567890', -- Fixed UUID for easy reference
    'asen_beekeeper',
    'ASEN_BEEKEEPER', 
    'asen.beekeeper@example.com',
    'ASEN.BEEKEEPER@EXAMPLE.COM',
    true,
    'AQAAAAEAACcQAAAAEExampleHash123456789',
    'SecurityStamp123456789',
    'ConcurrencyStamp123456789',
    'Asen',
    'Asenev',
    '+359888123456',
    true,
    false,
    true,
    0
);

-- ============================================
-- Create beekeeper record
-- ============================================
INSERT INTO "Beekeepers" (
    "Id", "PhoneNumber", "UserId", "HiveFarmPicturePaths"
) VALUES (
    'b2c3d4e5-f6g7-8901-bcde-f23456789012', -- Fixed UUID for easy reference
    '+359888123456',
    'a1b2c3d4-e5f6-7890-abcd-ef1234567890', -- References the user above
    '/uploads/beekeeper-farm.jpg'
);

-- ============================================
-- Create categories
-- ============================================
INSERT INTO "Categories" ("Id", "Name") VALUES 
(1, 'Linden'),
(2, 'Bio'),
(3, 'Sunflower'),
(4, 'Bouquet'),
(5, 'Honeydew')
ON CONFLICT ("Id") DO NOTHING;

-- ============================================
-- Create sample honey product
-- ============================================
INSERT INTO "Honeys" (
    "Id", "Title", "Origin", "Description", "ImageUrl", "Price", 
    "NetWeight", "CreatedOn", "YearMade", "IsActive", "CategoryId", 
    "BeekeeperId", "IsPromoted"
) VALUES (
    'c3d4e5f6-g7h8-9012-cdef-345678901234', -- Fixed UUID for easy reference
    'Premium Linden Honey from Vratsa',
    'Vratsa, Bulgaria',
    'Exceptional quality linden honey collected from the pristine forests of Vratsa. This honey has a delicate floral aroma and smooth texture, perfect for tea and desserts. Produced by experienced beekeeper Asen Asenev using traditional methods.',
    'https://beehoneyportal.com/wp-content/uploads/2014/10/burkan-s-med-3.jpg',
    15.50,
    500,
    NOW(),
    2024,
    true,
    1, -- Linden category
    'b2c3d4e5-f6g7-8901-bcde-f23456789012', -- References the beekeeper above
    false
);

-- ============================================
-- Verification queries
-- ============================================
-- Check if everything was created successfully
SELECT 'User Created:' as "Status", "UserName", "Email", "FirstName", "LastName" 
FROM "AspNetUsers" 
WHERE "Id" = 'a1b2c3d4-e5f6-7890-abcd-ef1234567890'

UNION ALL

SELECT 'Beekeeper Created:' as "Status", "PhoneNumber" as "UserName", "UserId" as "Email", "HiveFarmPicturePaths" as "FirstName", '' as "LastName"
FROM "Beekeepers" 
WHERE "Id" = 'b2c3d4e5-f6g7-8901-bcde-f23456789012'

UNION ALL

SELECT 'Honey Created:' as "Status", "Title" as "UserName", "Origin" as "Email", "Price"::text as "FirstName", "CategoryId"::text as "LastName"
FROM "Honeys" 
WHERE "Id" = 'c3d4e5f6-g7h8-9012-cdef-345678901234';

-- Show all categories
SELECT 'Categories Available:' as "Info", "Id"::text as "CategoryId", "Name" as "CategoryName"
FROM "Categories" 
ORDER BY "Id";
