-- Fix category names to match Bulgarian names used in the application
-- This script updates the categories to use Bulgarian names

-- Update existing categories to Bulgarian names
UPDATE "Categories" SET "Name" = 'Липов' WHERE "Name" = 'Linden';
UPDATE "Categories" SET "Name" = 'Слънчогледов' WHERE "Name" = 'Sunflower';
UPDATE "Categories" SET "Name" = 'Билков' WHERE "Name" = 'Bio';
UPDATE "Categories" SET "Name" = 'Акациев' WHERE "Name" = 'Bouquet';
UPDATE "Categories" SET "Name" = 'Горски' WHERE "Name" = 'Honeydew';

-- If categories don't exist, create them
INSERT INTO "Categories" ("Id", "Name") VALUES 
(1, 'Липов'),
(2, 'Слънчогледов'),
(3, 'Билков'),
(4, 'Акациев'),
(5, 'Горски')
ON CONFLICT ("Id") DO UPDATE SET "Name" = EXCLUDED."Name";

-- Verify the categories
SELECT "Id", "Name" FROM "Categories" ORDER BY "Id";

-- Check if we have any active honeys
SELECT COUNT(*) as "Active Honeys Count" FROM "Honeys" WHERE "IsActive" = true;

-- If no active honeys, create some sample ones
-- First, get a beekeeper ID
DO $$
DECLARE
    beekeeper_id UUID;
BEGIN
    -- Get the first beekeeper ID
    SELECT "Id" INTO beekeeper_id FROM "Beekeepers" LIMIT 1;
    
    -- If we have a beekeeper, create sample honeys
    IF beekeeper_id IS NOT NULL THEN
        -- Insert sample honey products
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
            'Липов мед от Враца',
            'Враца, България',
            'Исключително качествен липов мед, събран от девствените гори на Враца. Този мед има нежен цветен аромат и гладка текстура.',
            '/uploads/HoneyPictures/linden_honey.jpg',
            15.50,
            500,
            NOW(),
            2024,
            true,
            1, -- Липов category
            beekeeper_id,
            true
        ),
        (
            gen_random_uuid(),
            'Слънчогледов мед от Добрич',
            'Добрич, България',
            'Чист слънчогледов мед от слънчогледовите полета на Добрич. Богат на витамини и минерали.',
            '/uploads/HoneyPictures/sunflower_honey.jpg',
            12.00,
            450,
            NOW(),
            2024,
            true,
            2, -- Слънчогледов category
            beekeeper_id,
            true
        ),
        (
            gen_random_uuid(),
            'Билков мед от Родопите',
            'Родопи, България',
            'Билков мед от различни лечебни билки от Родопите. Има сложен вкус и аромат.',
            '/uploads/HoneyPictures/herbal_honey.jpg',
            18.00,
            400,
            NOW(),
            2024,
            true,
            3, -- Билков category
            beekeeper_id,
            true
        )
        ON CONFLICT DO NOTHING;
    END IF;
END $$;

-- Final verification
SELECT 'Categories:' as Info;
SELECT "Id", "Name" FROM "Categories" ORDER BY "Id";

SELECT 'Active Honeys:' as Info;
SELECT "Id", "Title", "CategoryId", "IsActive", "IsPromoted" FROM "Honeys" WHERE "IsActive" = true;
