-- PostgreSQL script to create posts manually
-- Run this in your Railway PostgreSQL database

-- Step 1: Check available authors (users)
SELECT "Id" as "AuthorId", "UserName", "Email" FROM "AspNetUsers" LIMIT 5;

-- Step 2: Create sample posts
-- Replace 'YOUR_AUTHOR_ID_HERE' with actual UserId from Step 1

INSERT INTO "Posts" (
    "Id",
    "Title", 
    "Content",
    "ImageUrl",
    "IsActive",
    "AuthorId",
    "CreatedOn"
) VALUES 
(
    gen_random_uuid(),
    'Welcome to Save the Bee Bulgaria',
    'Welcome to our honey platform! We are dedicated to supporting Bulgarian beekeepers and promoting high-quality honey products. Join our community and discover the finest honey from across Bulgaria.',
    'https://th.bing.com/th/id/OIP.eYhgoQcmVrOQG4mTZWpdLwHaE6?rs=1&pid=ImgDetMain',
    true,
    'YOUR_AUTHOR_ID_HERE', -- Replace with actual UserId
    NOW()
),
(
    gen_random_uuid(),
    'The Art of Beekeeping in Bulgaria',
    'Bulgaria has a rich tradition of beekeeping dating back centuries. Our beekeepers use traditional methods combined with modern techniques to produce exceptional honey. Learn about our sustainable practices and commitment to bee conservation.',
    'https://example.com/beekeeping.jpg',
    true,
    'YOUR_AUTHOR_ID_HERE', -- Replace with actual UserId
    NOW()
),
(
    gen_random_uuid(),
    'Types of Bulgarian Honey',
    'Bulgaria produces diverse types of honey including Linden, Acacia, Sunflower, and Forest honey. Each type has unique characteristics and health benefits. Discover the different varieties and their distinctive flavors.',
    'https://example.com/honey-types.jpg',
    true,
    'YOUR_AUTHOR_ID_HERE', -- Replace with actual UserId
    NOW()
);

-- Step 3: Verify posts were created
SELECT 
    "Id",
    "Title", 
    "CreatedOn",
    "IsActive"
FROM "Posts" 
ORDER BY "CreatedOn" DESC 
LIMIT 5;
