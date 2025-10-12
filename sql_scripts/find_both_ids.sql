-- Complete script to find both AuthorId and BeekeeperId
-- Run this in your Railway PostgreSQL database

-- ============================================
-- FIND AUTHORID (for Posts)
-- ============================================
SELECT '=== AUTHORID FOR POSTS ===' as "Section";

-- Show all users who can be post authors
SELECT 
    "Id" as "AuthorId",
    "UserName",
    "Email",
    "FirstName",
    "LastName"
FROM "AspNetUsers" 
ORDER BY "CreatedOn" DESC
LIMIT 5;

-- ============================================
-- FIND BEEKEEPERID (for Honey/Propolis)
-- ============================================
SELECT '=== BEEKEEPERID FOR HONEY/PROPOLIS ===' as "Section";

-- Show all beekeepers
SELECT 
    "Id" as "BeekeeperId",
    "PhoneNumber",
    "UserId"
FROM "Beekeepers"
LIMIT 5;

-- ============================================
-- RELATIONSHIP CHECK
-- ============================================
SELECT '=== USER-BEEKEEPER RELATIONSHIPS ===' as "Section";

-- Show users who are also beekeepers
SELECT 
    u."Id" as "UserId",
    u."UserName",
    u."Email",
    b."Id" as "BeekeeperId",
    b."PhoneNumber"
FROM "AspNetUsers" u
LEFT JOIN "Beekeepers" b ON u."Id" = b."UserId"
ORDER BY u."CreatedOn" DESC
LIMIT 5;

-- ============================================
-- QUICK RECOMMENDATIONS
-- ============================================
SELECT '=== RECOMMENDED IDs ===' as "Section";

-- Get first user for AuthorId
SELECT "Id" as "RecommendedAuthorId" FROM "AspNetUsers" LIMIT 1;

-- Get first beekeeper for BeekeeperId (if exists)
SELECT "Id" as "RecommendedBeekeeperId" FROM "Beekeepers" LIMIT 1;
