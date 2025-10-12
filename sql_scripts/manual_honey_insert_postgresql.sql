-- PostgreSQL Script to manually add honey data to Railway database
-- This script handles PostgreSQL-specific syntax and constraints

-- Step 1: Ensure categories exist (with conflict handling)
INSERT INTO "Categories" ("Id", "Name") VALUES 
(1, 'Linden'),
(2, 'Bio'),
(3, 'Sunflower'),
(4, 'Bouquet'),
(5, 'Honeydew')
ON CONFLICT ("Id") DO NOTHING;

-- Step 2: Get or create a beekeeper (you'll need to replace this with actual beekeeper ID)
-- First, let's see what beekeepers exist:
-- SELECT "Id", "UserId" FROM "Beekeepers" LIMIT 5;

-- Step 3: Insert honey products
-- Replace 'YOUR_BEEKEEPER_ID_HERE' with an actual beekeeper ID from your database
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
) VALUES 
(
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
    1,
    'YOUR_BEEKEEPER_ID_HERE', -- Replace with actual BeekeeperId
    false
),
(
    gen_random_uuid(),
    'Organic Sunflower Honey',
    'Dobrich, Bulgaria',
    'Pure organic sunflower honey from the sunflower fields of Dobrich. Rich in vitamins and minerals, this golden honey has a distinctive sunflower taste.',
    'https://example.com/sunflower-honey.jpg',
    12.00,
    450,
    NOW(),
    2024,
    true,
    3,
    'YOUR_BEEKEEPER_ID_HERE', -- Replace with actual BeekeeperId
    false
),
(
    gen_random_uuid(),
    'Bio Acacia Honey',
    'Plovdiv, Bulgaria',
    'Certified bio acacia honey from the acacia forests near Plovdiv. This light, crystal-clear honey is perfect for those who prefer mild sweetness.',
    'https://example.com/acacia-honey.jpg',
    18.00,
    500,
    NOW(),
    2024,
    true,
    2,
    'YOUR_BEEKEEPER_ID_HERE', -- Replace with actual BeekeeperId
    false
),
(
    gen_random_uuid(),
    'Wildflower Bouquet Honey',
    'Rila Mountains, Bulgaria',
    'Aromatic wildflower honey collected from the diverse flora of Rila Mountains. This multi-floral honey offers a complex taste profile with notes of various mountain flowers.',
    'https://example.com/wildflower-honey.jpg',
    16.50,
    400,
    NOW(),
    2024,
    true,
    4,
    'YOUR_BEEKEEPER_ID_HERE', -- Replace with actual BeekeeperId
    false
),
(
    gen_random_uuid(),
    'Forest Honeydew Honey',
    'Stara Planina, Bulgaria',
    'Dark, rich honeydew honey from the ancient forests of Stara Planina. This honey has a robust flavor and is rich in minerals and antioxidants.',
    'https://example.com/honeydew-honey.jpg',
    20.00,
    350,
    NOW(),
    2024,
    true,
    5,
    'YOUR_BEEKEEPER_ID_HERE', -- Replace with actual BeekeeperId
    false
);

-- Step 4: Verify the data was inserted
SELECT COUNT(*) as "Total Honeys" FROM "Honeys";
SELECT COUNT(*) as "Total Categories" FROM "Categories";
SELECT "Title", "Price", "Origin" FROM "Honeys" ORDER BY "CreatedOn" DESC LIMIT 5;
