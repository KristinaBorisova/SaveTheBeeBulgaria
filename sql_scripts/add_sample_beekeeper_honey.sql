-- PostgreSQL Script to manually add sample beekeeper and honey data
-- Run this in your Railway PostgreSQL database

-- ============================================
-- STEP 1: Create a sample user (beekeeper)
-- ============================================
INSERT INTO "AspNetUsers" (
    "Id",
    "UserName",
    "NormalizedUserName",
    "Email",
    "NormalizedEmail",
    "EmailConfirmed",
    "PasswordHash",
    "SecurityStamp",
    "ConcurrencyStamp",
    "FirstName",
    "LastName",
    "PhoneNumber",
    "PhoneNumberConfirmed",
    "TwoFactorEnabled",
    "LockoutEnabled",
    "AccessFailedCount"
) VALUES (
    gen_random_uuid(),
    'asen_beekeeper',
    'ASEN_BEEKEEPER',
    'asen.beekeeper@example.com',
    'ASEN.BEEKEEPER@EXAMPLE.COM',
    true,
    'AQAAAAEAACcQAAAAEExampleHash123456789', -- Placeholder hash
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
-- STEP 2: Get the created user ID
-- ============================================
-- Note: Replace 'USER_ID_HERE' in the next step with the actual ID from this query
SELECT "Id" as "CreatedUserId", "UserName", "Email" 
FROM "AspNetUsers" 
WHERE "UserName" = 'asen_beekeeper';

-- ============================================
-- STEP 3: Create beekeeper record for the user
-- ============================================
-- Replace 'USER_ID_HERE' with the actual UserId from Step 2
INSERT INTO "Beekeepers" (
    "Id",
    "PhoneNumber",
    "UserId",
    "HiveFarmPicturePaths"
) VALUES (
    gen_random_uuid(),
    '+359888123456',
    'USER_ID_HERE', -- Replace with actual UserId from Step 2
    '/uploads/beekeeper-farm.jpg'
);

-- ============================================
-- STEP 4: Ensure categories exist
-- ============================================
INSERT INTO "Categories" ("Id", "Name") VALUES 
(1, 'Linden'),
(2, 'Bio'),
(3, 'Sunflower'),
(4, 'Bouquet'),
(5, 'Honeydew')
ON CONFLICT ("Id") DO NOTHING;

-- ============================================
-- STEP 5: Get the beekeeper ID
-- ============================================
-- Note: Replace 'BEEKEEPER_ID_HERE' in the next step with the actual ID from this query
SELECT "Id" as "CreatedBeekeeperId", "PhoneNumber", "UserId"
FROM "Beekeepers" 
WHERE "PhoneNumber" = '+359888123456';

-- ============================================
-- STEP 6: Create sample honey product
-- ============================================
-- Replace 'BEEKEEPER_ID_HERE' with the actual BeekeeperId from Step 5
INSERT INTO "Honeys" (
    "Id",
    "Title",
    "Origin",
    "Description",
    "ImageUrl",
    "Price",
    "NetWeight",
    "CreatedOn",
    "YearMade",
    "IsActive",
    "CategoryId",
    "BeekeeperId",
    "IsPromoted"
) VALUES (
    gen_random_uuid(),
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
    'BEEKEEPER_ID_HERE', -- Replace with actual BeekeeperId from Step 5
    false
);

-- ============================================
-- STEP 7: Verify the data was created
-- ============================================
-- Check user
SELECT "Id", "UserName", "Email", "FirstName", "LastName" 
FROM "AspNetUsers" 
WHERE "UserName" = 'asen_beekeeper';

-- Check beekeeper
SELECT "Id", "PhoneNumber", "UserId"
FROM "Beekeepers" 
WHERE "PhoneNumber" = '+359888123456';

-- Check honey
SELECT "Id", "Title", "Price", "Origin", "CategoryId", "BeekeeperId"
FROM "Honeys" 
WHERE "Title" = 'Premium Linden Honey from Vratsa';

-- Check categories
SELECT "Id", "Name" FROM "Categories" ORDER BY "Id";
