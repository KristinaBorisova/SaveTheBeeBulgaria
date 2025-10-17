-- Check honey data and image URLs to diagnose homepage image issues
-- This script will help identify why honey product images are sometimes breaking

-- Check all honeys and their image URLs
SELECT 
    'All Honeys:' as Info,
    COUNT(*) as TotalCount
FROM "Honeys";

-- Check active honeys
SELECT 
    'Active Honeys:' as Info,
    COUNT(*) as ActiveCount
FROM "Honeys" 
WHERE "IsActive" = true;

-- Check promoted honeys
SELECT 
    'Promoted Honeys:' as Info,
    COUNT(*) as PromotedCount
FROM "Honeys" 
WHERE "IsActive" = true AND "IsPromoted" = true;

-- Check honeys with valid image URLs
SELECT 
    'Honeys with Image URLs:' as Info,
    COUNT(*) as WithImagesCount
FROM "Honeys" 
WHERE "IsActive" = true AND "ImageUrl" IS NOT NULL AND "ImageUrl" != '';

-- Check honeys with invalid/empty image URLs
SELECT 
    'Honeys with Invalid Image URLs:' as Info,
    COUNT(*) as InvalidImagesCount
FROM "Honeys" 
WHERE "IsActive" = true AND ("ImageUrl" IS NULL OR "ImageUrl" = '');

-- Show sample honey data
SELECT 
    'Sample Honey Data:' as Info;
    
SELECT 
    "Id",
    "Title",
    "Origin",
    "ImageUrl",
    "IsActive",
    "IsPromoted",
    "CreatedOn"
FROM "Honeys" 
WHERE "IsActive" = true
ORDER BY "CreatedOn" DESC
LIMIT 10;

-- Check for common image URL issues
SELECT 
    'Image URL Issues:' as Info;

SELECT 
    "Title",
    "ImageUrl",
    CASE 
        WHEN "ImageUrl" IS NULL THEN 'NULL ImageUrl'
        WHEN "ImageUrl" = '' THEN 'Empty ImageUrl'
        WHEN "ImageUrl" NOT LIKE '/%' AND "ImageUrl" NOT LIKE 'http%' THEN 'Invalid path format'
        ELSE 'Valid format'
    END as ImageIssue
FROM "Honeys" 
WHERE "IsActive" = true
ORDER BY "CreatedOn" DESC;
