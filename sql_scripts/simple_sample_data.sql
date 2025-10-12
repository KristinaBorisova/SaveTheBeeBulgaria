-- SIMPLE STEP-BY-STEP SCRIPT
-- Execute each section one at a time in Railway PostgreSQL console

-- ============================================
-- STEP 1: Create sample user
-- ============================================
INSERT INTO "AspNetUsers" (
    "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail",
    "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
    "FirstName", "LastName", "PhoneNumber", "PhoneNumberConfirmed",
    "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount"
) VALUES (
    gen_random_uuid(),
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
-- STEP 2: Get the user ID (copy this ID for next step)
-- ============================================
SELECT "Id" as "UserId" FROM "AspNetUsers" WHERE "UserName" = 'asen_beekeeper';

-- ============================================
-- STEP 3: Create beekeeper (replace USER_ID with actual ID from Step 2)
-- ============================================
INSERT INTO "Beekeepers" ("Id", "PhoneNumber", "UserId", "HiveFarmPicturePaths") 
VALUES (
    gen_random_uuid(),
    '+359888123456',
    'USER_ID_FROM_STEP_2', -- Replace this!
    '/uploads/beekeeper-farm.jpg'
);

-- ============================================
-- STEP 4: Create categories
-- ============================================
INSERT INTO "Categories" ("Id", "Name") VALUES 
(1, 'Linden'), (2, 'Bio'), (3, 'Sunflower'), (4, 'Bouquet'), (5, 'Honeydew')
ON CONFLICT ("Id") DO NOTHING;

-- ============================================
-- STEP 5: Get beekeeper ID (copy this ID for next step)
-- ============================================
SELECT "Id" as "BeekeeperId" FROM "Beekeepers" WHERE "PhoneNumber" = '+359888123456';

-- ============================================
-- STEP 6: Create honey (replace BEEKEEPER_ID with actual ID from Step 5)
-- ============================================
INSERT INTO "Honeys" (
    "Id", "Title", "Origin", "Description", "ImageUrl", "Price", 
    "NetWeight", "CreatedOn", "YearMade", "IsActive", "CategoryId", 
    "BeekeeperId", "IsPromoted"
) VALUES (
    gen_random_uuid(),
    'Premium Linden Honey from Vratsa',
    'Vratsa, Bulgaria',
    'Exceptional quality linden honey collected from the pristine forests of Vratsa. This honey has a delicate floral aroma and smooth texture, perfect for tea and desserts.',
    'https://beehoneyportal.com/wp-content/uploads/2014/10/burkan-s-med-3.jpg',
    15.50,
    500,
    NOW(),
    2024,
    true,
    1, -- Linden category
    'BEEKEEPER_ID_FROM_STEP_5', -- Replace this!
    false
);

-- ============================================
-- STEP 7: Verify everything was created
-- ============================================
-- Check user
SELECT "UserName", "Email", "FirstName", "LastName" FROM "AspNetUsers" WHERE "UserName" = 'asen_beekeeper';

-- Check beekeeper  
SELECT "PhoneNumber", "UserId" FROM "Beekeepers" WHERE "PhoneNumber" = '+359888123456';

-- Check honey
SELECT "Title", "Price", "Origin", "CategoryId" FROM "Honeys" WHERE "Title" = 'Premium Linden Honey from Vratsa';

-- Check categories
SELECT "Id", "Name" FROM "Categories" ORDER BY "Id";
