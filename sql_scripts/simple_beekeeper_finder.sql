-- SIMPLE STEP-BY-STEP GUIDE TO FIND BEEKEEPERID
-- Run each section one at a time in Railway PostgreSQL console

-- ============================================
-- STEP 1: Check if you have any beekeepers
-- ============================================
SELECT COUNT(*) as "Do I have beekeepers?" FROM "Beekeepers";

-- ============================================
-- STEP 2A: If count > 0, show beekeeper IDs
-- ============================================
SELECT "Id" as "BeekeeperId", "PhoneNumber" FROM "Beekeepers";

-- ============================================
-- STEP 2B: If count = 0, check users first
-- ============================================
SELECT "Id" as "UserId", "UserName", "Email" FROM "AspNetUsers" LIMIT 3;

-- ============================================
-- STEP 3: Create a beekeeper (if needed)
-- ============================================
-- Replace 'USER_ID_FROM_STEP_2B' with actual UserId
-- INSERT INTO "Beekeepers" ("Id", "PhoneNumber", "UserId") 
-- VALUES (gen_random_uuid(), '+359888888888', 'USER_ID_FROM_STEP_2B');

-- ============================================
-- STEP 4: Get your BeekeeperId for honey insertion
-- ============================================
SELECT "Id" as "YourBeekeeperId" FROM "Beekeepers" LIMIT 1;
