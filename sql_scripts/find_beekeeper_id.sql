-- Script to find and work with BeekeeperId in PostgreSQL Railway database
-- Run these queries step by step to locate your BeekeeperId

-- Step 1: Check if you have any beekeepers at all
SELECT 'Checking Beekeepers Table...' as "Status";
SELECT COUNT(*) as "Total Beekeepers" FROM "Beekeepers";

-- Step 2: If you have beekeepers, show them
SELECT 
    "Id" as "BeekeeperId",
    "PhoneNumber",
    "UserId",
    "HiveFarmPicturePaths"
FROM "Beekeepers" 
LIMIT 10;

-- Step 3: Check if you have any users (potential beekeepers)
SELECT 'Checking Users Table...' as "Status";
SELECT COUNT(*) as "Total Users" FROM "AspNetUsers";

-- Step 4: Show users who could become beekeepers
SELECT 
    "Id" as "UserId",
    "UserName",
    "Email",
    "FirstName",
    "LastName",
    "EmailConfirmed"
FROM "AspNetUsers" 
LIMIT 10;

-- Step 5: Check if any users are already beekeepers
SELECT 
    u."Id" as "UserId",
    u."UserName",
    u."Email",
    b."Id" as "BeekeeperId",
    b."PhoneNumber"
FROM "AspNetUsers" u
LEFT JOIN "Beekeepers" b ON u."Id" = b."UserId"
ORDER BY u."CreatedOn" DESC
LIMIT 10;

-- Step 6: If you have NO beekeepers, create one from existing user
-- First, let's see what users exist:
SELECT 'Available Users for Beekeeper Creation:' as "Status";
SELECT 
    "Id",
    "UserName", 
    "Email",
    "FirstName",
    "LastName"
FROM "AspNetUsers" 
WHERE "EmailConfirmed" = true
LIMIT 5;

-- Step 7: Create a beekeeper from the first available user
-- Replace 'USER_ID_HERE' with an actual UserId from Step 6
-- Uncomment and modify the line below:
-- INSERT INTO "Beekeepers" ("Id", "PhoneNumber", "UserId") 
-- VALUES (gen_random_uuid(), '+359888888888', 'USER_ID_HERE');

-- Step 8: Verify the beekeeper was created
-- SELECT "Id" as "NewBeekeeperId" FROM "Beekeepers" ORDER BY "Id" DESC LIMIT 1;
