-- Add sample honey products to fix the catalog section
-- This script adds honey products with IsPromoted = true

-- First, let's check what we have
SELECT 'Current Honeys:' as Info;
SELECT Id, Title, IsActive, IsPromoted, ImageUrl FROM Honeys;

-- Check if we have any beekeepers
SELECT 'Current Beekeepers:' as Info;
SELECT Id, PhoneNumber FROM Beekeepers LIMIT 5;

-- Check categories
SELECT 'Current Categories:' as Info;
SELECT Id, Name FROM Categories;

-- Add sample honey products with IsPromoted = true
-- We'll use the first beekeeper ID we find and create categories if needed

-- Ensure categories exist
INSERT OR IGNORE INTO Categories (Id, Name) VALUES 
(1, 'Липов'),
(2, 'Слънчогледов'),
(3, 'Билков'),
(4, 'Акациев'),
(5, 'Горски');

-- Add sample honey products
INSERT INTO Honeys (
    Id, 
    Title, 
    Origin, 
    Description, 
    ImageUrl, 
    Price, 
    NetWeight, 
    CreatedOn, 
    YearMade, 
    IsActive, 
    CategoryId, 
    BeekeeperId, 
    IsPromoted
) 
SELECT 
    '11111111-1111-1111-1111-111111111111',
    'Липов мед от Враца',
    'Враца, България',
    'Исключително качествен липов мед, събран от девствените гори на Враца. Този мед има нежен цветен аромат и гладка текстура, перфектен за чай и десерти.',
    '/uploads/HoneyPictures/4db4e509-4aec-4b3a-98f3-259f0f94c44a_16797030_1868820710027754_295953195351026834_o.jpg',
    15.50,
    500,
    datetime('now'),
    2024,
    1,
    1, -- Linden category
    (SELECT Id FROM Beekeepers LIMIT 1), -- Use first beekeeper
    1 -- IsPromoted = true
WHERE EXISTS (SELECT 1 FROM Beekeepers LIMIT 1);

-- Add sunflower honey
INSERT INTO Honeys (
    Id, 
    Title, 
    Origin, 
    Description, 
    ImageUrl, 
    Price, 
    NetWeight, 
    CreatedOn, 
    YearMade, 
    IsActive, 
    CategoryId, 
    BeekeeperId, 
    IsPromoted
) 
SELECT 
    '22222222-2222-2222-2222-222222222222',
    'Слънчогледов мед от Добрич',
    'Добрич, България',
    'Чист органичен слънчогледов мед от слънчогледовите полета на Добрич. Богат на витамини и минерали, този златист мед има отличителен слънчогледов вкус.',
    '/uploads/HoneyPictures/7f771008-d64c-465e-b311-c326551d7d2a_Мецан.png',
    12.00,
    450,
    datetime('now'),
    2024,
    1,
    2, -- Sunflower category
    (SELECT Id FROM Beekeepers LIMIT 1),
    1
WHERE EXISTS (SELECT 1 FROM Beekeepers LIMIT 1);

-- Add herbal honey
INSERT INTO Honeys (
    Id, 
    Title, 
    Origin, 
    Description, 
    ImageUrl, 
    Price, 
    NetWeight, 
    CreatedOn, 
    YearMade, 
    IsActive, 
    CategoryId, 
    BeekeeperId, 
    IsPromoted
) 
SELECT 
    '33333333-3333-3333-3333-333333333333',
    'Билков мед от Рила',
    'Рила, България',
    'Ароматен билков мед, събран от разнообразната флора на Рила. Този многобукетен мед предлага сложен вкусов профил с нотки от различни планински цветя.',
    '/uploads/HoneyPictures/81e9d597-a7d6-4aa6-bb7f-c03d6eda4fbb_Мецан.png',
    16.50,
    400,
    datetime('now'),
    2024,
    1,
    3, -- Herbal category
    (SELECT Id FROM Beekeepers LIMIT 1),
    1
WHERE EXISTS (SELECT 1 FROM Beekeepers LIMIT 1);

-- Add acacia honey
INSERT INTO Honeys (
    Id, 
    Title, 
    Origin, 
    Description, 
    ImageUrl, 
    Price, 
    NetWeight, 
    CreatedOn, 
    YearMade, 
    IsActive, 
    CategoryId, 
    BeekeeperId, 
    IsPromoted
) 
SELECT 
    '44444444-4444-4444-4444-444444444444',
    'Акациев мед от Пловдив',
    'Пловдив, България',
    'Сертифициран био акациев мед от акациевите гори край Пловдив. Този светъл, кристално чист мед е перфектен за тези, които предпочитат мека сладост.',
    '/uploads/HoneyPictures/f4825bcb-3795-4217-8e43-1c033a0ec8fd_Мецан.png',
    18.00,
    500,
    datetime('now'),
    2024,
    1,
    4, -- Acacia category
    (SELECT Id FROM Beekeepers LIMIT 1),
    1
WHERE EXISTS (SELECT 1 FROM Beekeepers LIMIT 1);

-- Verify the data was inserted
SELECT 'After Insert - Promoted Honeys:' as Info;
SELECT Id, Title, IsActive, IsPromoted, ImageUrl FROM Honeys WHERE IsPromoted = 1;

SELECT 'Total Honeys:' as Info;
SELECT COUNT(*) as Total FROM Honeys;
