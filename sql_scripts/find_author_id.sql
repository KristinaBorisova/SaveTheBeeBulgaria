-- Script to find AuthorId for Posts in PostgreSQL Railway database
-- AuthorId = UserId (from AspNetUsers table)

-- ============================================
-- STEP 1: Check what users exist (these are potential authors)
-- ============================================
SELECT 'Available Users (Potential Authors):' as "Status";
SELECT 
    "Id" as "AuthorId",
    "UserName",
    "Email",
    "FirstName",
    "LastName",
    "EmailConfirmed"
FROM "AspNetUsers" 
ORDER BY "CreatedOn" DESC
LIMIT 10;

-- ============================================
-- STEP 2: Check if any users have already written posts
-- ============================================
SELECT 'Users who have written posts:' as "Status";
SELECT 
    u."Id" as "AuthorId",
    u."UserName",
    u."Email",
    COUNT(p."Id") as "PostCount"
FROM "AspNetUsers" u
LEFT JOIN "Posts" p ON u."Id" = p."AuthorId"
GROUP BY u."Id", u."UserName", u."Email"
HAVING COUNT(p."Id") > 0
ORDER BY "PostCount" DESC;

-- ============================================
-- STEP 3: Check existing posts and their authors
-- ============================================
SELECT 'Existing Posts and Authors:' as "Status";
SELECT 
    p."Id" as "PostId",
    p."Title",
    p."CreatedOn",
    u."Id" as "AuthorId",
    u."UserName" as "AuthorName",
    u."Email" as "AuthorEmail"
FROM "Posts" p
JOIN "AspNetUsers" u ON p."AuthorId" = u."Id"
ORDER BY p."CreatedOn" DESC
LIMIT 5;

-- ============================================
-- STEP 4: Get a specific AuthorId for creating new posts
-- ============================================
SELECT 'Use this AuthorId for new posts:' as "Status";
SELECT "Id" as "RecommendedAuthorId" 
FROM "AspNetUsers" 
WHERE "EmailConfirmed" = true
ORDER BY "CreatedOn" DESC
LIMIT 1;

-- ============================================
-- STEP 5: Create a test post (replace AUTHOR_ID)
-- ============================================
-- Uncomment and modify the line below:
-- INSERT INTO "Posts" (
--     "Id", "Title", "Content", "ImageUrl", "IsActive", "AuthorId", "CreatedOn"
-- ) VALUES (
--     gen_random_uuid(),
--     'Test Post by Author',
--     'This is a test post created manually with AuthorId.',
--     'https://example.com/test.jpg',
--     true,
--     'AUTHOR_ID_FROM_STEP_4', -- Replace with actual AuthorId
--     NOW()
-- );
