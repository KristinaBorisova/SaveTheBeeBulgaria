-- Quick PostgreSQL script to add a single honey product
-- Run this first to test, then use the full script

-- 1. Check existing beekeepers
SELECT "Id", "UserId" FROM "Beekeepers" LIMIT 3;

-- 2. Check existing categories  
SELECT "Id", "Name" FROM "Categories";

-- 3. Add one test honey (replace BEEKEEPER_ID with actual ID from step 1)
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
    'Test Linden Honey',
    'Sofia, Bulgaria',
    'Test honey for manual insertion. High quality linden honey with excellent taste.',
    'https://beehoneyportal.com/wp-content/uploads/2014/10/burkan-s-med-3.jpg',
    12.50,
    450,
    NOW(),
    2024,
    true,
    1, -- Linden category
    'REPLACE_WITH_ACTUAL_BEEKEEPER_ID', -- Replace this!
    false
);

-- 4. Verify insertion
SELECT "Title", "Price", "Origin", "CreatedOn" FROM "Honeys" ORDER BY "CreatedOn" DESC LIMIT 1;
