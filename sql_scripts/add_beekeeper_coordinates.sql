-- Add Latitude and Longitude columns to Beekeepers table for Interactive Map feature
-- Run this script on Railway PostgreSQL database

ALTER TABLE "Beekeepers" 
ADD COLUMN IF NOT EXISTS "Latitude" REAL,
ADD COLUMN IF NOT EXISTS "Longitude" REAL;

-- Optionally, add some test coordinates for existing beekeepers
-- Uncomment and modify these as needed:
/*
UPDATE "Beekeepers" 
SET 
    "Latitude" = 43.2044,
    "Longitude" = 23.5074
WHERE "UserId" IN (
    SELECT "Id" FROM "AspNetUsers" 
    WHERE "FirstName" = 'Ивайло' AND "LastName" = 'Борисов'
    LIMIT 1
);
*/

-- Verify the columns were added
SELECT 
    column_name, 
    data_type, 
    is_nullable
FROM information_schema.columns 
WHERE table_name = 'Beekeepers' 
AND column_name IN ('Latitude', 'Longitude')
ORDER BY ordinal_position;

