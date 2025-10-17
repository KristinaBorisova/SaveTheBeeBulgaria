-- Add missing columns to Beekeepers table
-- This script adds the new columns that were added to the Beekeeper model

-- Add the missing columns to the Beekeepers table
ALTER TABLE "Beekeepers" 
ADD COLUMN IF NOT EXISTS "Story" VARCHAR(500),
ADD COLUMN IF NOT EXISTS "Region" VARCHAR(100),
ADD COLUMN IF NOT EXISTS "NumberOfHives" INTEGER,
ADD COLUMN IF NOT EXISTS "ExperienceYears" VARCHAR(100),
ADD COLUMN IF NOT EXISTS "Specialties" VARCHAR(500);

-- Update existing beekeepers with sample data
UPDATE "Beekeepers" 
SET 
    "Story" = 'Страстен пчелар с дългогодишен опит в отглеждането на пчели и производството на качествен мед.',
    "Region" = 'България',
    "NumberOfHives" = 15,
    "ExperienceYears" = '10+ години',
    "Specialties" = 'Липов мед, Слънчогледов мед, Билков мед'
WHERE "Story" IS NULL;

-- Verify the columns were added
SELECT 
    column_name, 
    data_type, 
    is_nullable
FROM information_schema.columns 
WHERE table_name = 'Beekeepers' 
ORDER BY ordinal_position;
