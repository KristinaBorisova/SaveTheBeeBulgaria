-- Fix honey search functionality
-- Check current honey data
SELECT Id, Title, Origin, Price, IsActive, CreatedOn FROM Honeys;

-- Update all existing honeys to be active if they're not
UPDATE Honeys SET IsActive = 1 WHERE IsActive = 0 OR IsActive IS NULL;

-- Add some sample honeys for testing if none exist
INSERT OR IGNORE INTO Honeys (Id, Title, Origin, Description, Price, ImageUrl, IsActive, IsPromoted, CreatedOn, BeekeeperId, CategoryId)
VALUES 
    (NEWID(), 'Липов мед Тест', 'Врачански Предбалкан', 'Нежен липов мед с цветен аромат', 5.00, '/dingo-master/dingo-master/img/food_item/honey_item_1_Lipov.jpeg', 1, 1, GETDATE(), (SELECT TOP 1 Id FROM Beekeepers), (SELECT Id FROM Categories WHERE Name = 'Липов')),
    (NEWID(), 'Липов Мед Борисов', 'Врачански Предбалкан', 'Висококачествен липов мед от семейство Борисов', 13.00, '/dingo-master/dingo-master/img/food_item/honey_item_1_Lipov.jpeg', 1, 1, GETDATE(), (SELECT TOP 1 Id FROM Beekeepers), (SELECT Id FROM Categories WHERE Name = 'Липов')),
    (NEWID(), 'Слънчогледов мед', 'Добруджа', 'Слънчогледов мед с богат вкус', 12.00, '/dingo-master/dingo-master/img/food_item/honey_item_2_Sunflower.jpeg', 1, 1, GETDATE(), (SELECT TOP 1 Id FROM Beekeepers), (SELECT Id FROM Categories WHERE Name = 'Слънчогледов')),
    (NEWID(), 'Билков мед', 'Родопи', 'Билков мед от различни лечебни билки', 18.00, '/dingo-master/dingo-master/img/food_item/honey_item_3_Herbal.jpeg', 1, 1, GETDATE(), (SELECT TOP 1 Id FROM Beekeepers), (SELECT Id FROM Categories WHERE Name = 'Билков'));

-- Verify the data
SELECT Id, Title, Origin, Price, IsActive, IsPromoted, CreatedOn FROM Honeys WHERE IsActive = 1;
