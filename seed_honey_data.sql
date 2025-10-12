-- SQL Script to seed honey data in MySQL (Railway)
-- Run this script in your Railway MySQL database

-- First, ensure categories exist
INSERT IGNORE INTO Categories (Id, Name) VALUES 
(1, 'Linden'),
(2, 'Bio'),
(3, 'Sunflower'),
(4, 'Bouquet'),
(5, 'Honeydew');

-- Insert honey products
-- Note: You'll need to replace the BeekeeperId with an actual beekeeper ID from your database
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
) VALUES 
(
    UUID(),
    'Premium Linden Honey from Vratsa',
    'Vratsa, Bulgaria',
    'Exceptional quality linden honey collected from the pristine forests of Vratsa. This honey has a delicate floral aroma and smooth texture, perfect for tea and desserts.',
    'https://beehoneyportal.com/wp-content/uploads/2014/10/burkan-s-med-3.jpg',
    15.50,
    500,
    NOW(),
    2024,
    1,
    '7ADAF90E-FEC8-492E-8760-FE3190F1D689', -- Replace with actual BeekeeperId
    0
),
(
    UUID(),
    'Organic Sunflower Honey',
    'Dobrich, Bulgaria',
    'Pure organic sunflower honey from the sunflower fields of Dobrich. Rich in vitamins and minerals, this golden honey has a distinctive sunflower taste.',
    'https://example.com/sunflower-honey.jpg',
    12.00,
    450,
    NOW(),
    2024,
    3,
    '7ADAF90E-FEC8-492E-8760-FE3190F1D689', -- Replace with actual BeekeeperId
    0
),
(
    UUID(),
    'Bio Acacia Honey',
    'Plovdiv, Bulgaria',
    'Certified bio acacia honey from the acacia forests near Plovdiv. This light, crystal-clear honey is perfect for those who prefer mild sweetness.',
    'https://example.com/acacia-honey.jpg',
    18.00,
    500,
    NOW(),
    2024,
    2,
    '7ADAF90E-FEC8-492E-8760-FE3190F1D689', -- Replace with actual BeekeeperId
    0
),
(
    UUID(),
    'Wildflower Bouquet Honey',
    'Rila Mountains, Bulgaria',
    'Aromatic wildflower honey collected from the diverse flora of Rila Mountains. This multi-floral honey offers a complex taste profile with notes of various mountain flowers.',
    'https://example.com/wildflower-honey.jpg',
    16.50,
    400,
    NOW(),
    2024,
    4,
    '7ADAF90E-FEC8-492E-8760-FE3190F1D689', -- Replace with actual BeekeeperId
    0
),
(
    UUID(),
    'Forest Honeydew Honey',
    'Stara Planina, Bulgaria',
    'Dark, rich honeydew honey from the ancient forests of Stara Planina. This honey has a robust flavor and is rich in minerals and antioxidants.',
    'https://example.com/honeydew-honey.jpg',
    20.00,
    350,
    NOW(),
    2024,
    5,
    '7ADAF90E-FEC8-492E-8760-FE3190F1D689', -- Replace with actual BeekeeperId
    0
);

-- Verify the data was inserted
SELECT COUNT(*) as 'Total Honeys Inserted' FROM Honeys;
SELECT COUNT(*) as 'Total Categories' FROM Categories;
