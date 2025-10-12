-- PostgreSQL Script to create admin user and manage beekeepers
-- Run this in your Railway PostgreSQL database

-- ============================================
-- STEP 1: Create Admin User
-- ============================================
INSERT INTO "AspNetUsers" (
    "Id",
    "UserName",
    "NormalizedUserName",
    "Email",
    "NormalizedEmail",
    "EmailConfirmed",
    "PasswordHash",
    "SecurityStamp",
    "ConcurrencyStamp",
    "FirstName",
    "LastName",
    "PhoneNumber",
    "PhoneNumberConfirmed",
    "TwoFactorEnabled",
    "LockoutEnabled",
    "AccessFailedCount"
) VALUES (
    'admin-user-id-1234-5678-9abc-def012345678', -- Fixed UUID for admin
    'admin',
    'ADMIN',
    'admin@honeyplatform.bg',
    'ADMIN@HONEYPLATFORM.BG',
    true,
    'AQAAAAEAACcQAAAAEAdminHash123456789', -- Placeholder hash
    'AdminSecurityStamp123',
    'AdminConcurrencyStamp123',
    'Admin',
    'User',
    '+359888000000',
    true,
    false,
    true,
    0
);

-- ============================================
-- STEP 2: Ensure Administrator role exists
-- ============================================
INSERT INTO "AspNetRoles" (
    "Id",
    "Name",
    "NormalizedName",
    "ConcurrencyStamp"
) VALUES (
    'admin-role-id-1234-5678-9abc-def012345678',
    'Administrator',
    'ADMINISTRATOR',
    'AdminRoleConcurrencyStamp123'
) ON CONFLICT ("Id") DO NOTHING;

-- ============================================
-- STEP 3: Assign Administrator role to admin user
-- ============================================
INSERT INTO "AspNetUserRoles" (
    "UserId",
    "RoleId"
) VALUES (
    'admin-user-id-1234-5678-9abc-def012345678', -- Admin user ID
    'admin-role-id-1234-5678-9abc-def012345678'  -- Administrator role ID
) ON CONFLICT DO NOTHING;

-- ============================================
-- STEP 4: Verify admin user creation
-- ============================================
SELECT 
    u."UserName",
    u."Email",
    u."FirstName",
    u."LastName",
    r."Name" as "Role"
FROM "AspNetUsers" u
LEFT JOIN "AspNetUserRoles" ur ON u."Id" = ur."UserId"
LEFT JOIN "AspNetRoles" r ON ur."RoleId" = r."Id"
WHERE u."UserName" = 'admin';

-- ============================================
-- STEP 5: Create sample beekeeper for management
-- ============================================
-- First create a regular user
INSERT INTO "AspNetUsers" (
    "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail",
    "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
    "FirstName", "LastName", "PhoneNumber", "PhoneNumberConfirmed",
    "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount"
) VALUES (
    'beekeeper-user-id-1234-5678-9abc-def012345678',
    'test_beekeeper',
    'TEST_BEEKEEPER',
    'test.beekeeper@example.com',
    'TEST.BEEKEEPER@EXAMPLE.COM',
    true,
    'AQAAAAEAACcQAAAAEBeekeeperHash123456789',
    'BeekeeperSecurityStamp123',
    'BeekeeperConcurrencyStamp123',
    'Test',
    'Beekeeper',
    '+359888111111',
    true,
    false,
    true,
    0
);

-- Then create beekeeper record
INSERT INTO "Beekeepers" (
    "Id", "PhoneNumber", "UserId", "HiveFarmPicturePaths"
) VALUES (
    'beekeeper-record-id-1234-5678-9abc-def012345678',
    '+359888111111',
    'beekeeper-user-id-1234-5678-9abc-def012345678',
    '/uploads/test-beekeeper-farm.jpg'
);

-- ============================================
-- STEP 6: Verify beekeeper creation
-- ============================================
SELECT 
    u."UserName",
    u."Email",
    u."FirstName",
    u."LastName",
    b."PhoneNumber",
    b."HiveFarmPicturePaths"
FROM "AspNetUsers" u
JOIN "Beekeepers" b ON u."Id" = b."UserId"
WHERE u."UserName" = 'test_beekeeper';

-- ============================================
-- STEP 7: Admin management queries
-- ============================================
-- View all users (admin can see this)
SELECT 
    "Id",
    "UserName",
    "Email",
    "FirstName",
    "LastName",
    "EmailConfirmed",
    "PhoneNumber"
FROM "AspNetUsers"
ORDER BY "CreatedOn" DESC;

-- View all beekeepers (admin can manage these)
SELECT 
    b."Id" as "BeekeeperId",
    b."PhoneNumber",
    b."HiveFarmPicturePaths",
    u."UserName",
    u."Email",
    u."FirstName",
    u."LastName"
FROM "Beekeepers" b
JOIN "AspNetUsers" u ON b."UserId" = u."Id"
ORDER BY u."CreatedOn" DESC;

-- ============================================
-- STEP 8: Remove beekeeper (admin function)
-- ============================================
-- To remove a beekeeper, uncomment and modify:
-- DELETE FROM "Beekeepers" WHERE "Id" = 'beekeeper-record-id-1234-5678-9abc-def012345678';
-- DELETE FROM "AspNetUsers" WHERE "Id" = 'beekeeper-user-id-1234-5678-9abc-def012345678';
