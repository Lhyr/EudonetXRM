-- Retrait de la table XrmProduct de [DESC] et [RES]

DECLARE @nTab INT = 116000

DELETE [RES]
WHERE ResId >= @nTab AND ResId < @nTab + 100

DELETE [DESCADV]
WHERE DescID >= @nTab AND DescID < @nTab + 100

DELETE [DESC]
WHERE DescID >= @nTab AND DescID < @nTab + 100

-- Ajout de la colonne [USER].[Product]

DECLARE @nProductDescid INT = 101034
	
IF NOT EXISTS (SELECT 1
	FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
	WHERE sys.tables.name like 'USER' and syscolumns.name like 'Product' )
BEGIN
ALTER TABLE [dbo].[USER] ADD [Product] VARCHAR(100) NULL;
END

DELETE [DESC] WHERE DESCID = @nProductDescid
DELETE [RES] WHERE ResId = @nProductDescid

-- On décale les disporder des derniers champs 
UPDATE [DESC] SET Disporder = Disporder + 1
WHERE Disporder >= 24
AND [File] = 'USER' 
AND EXISTS(SELECT Descid FROM [DESC] WHERE Descid = 101026 AND Disporder = 24)

UPDATE [DESC] SET y = y + 1
WHERE y >= 20 
AND [File] = 'USER' 
AND EXISTS(SELECT Descid FROM [DESC] WHERE Descid = 101026 AND y = 20)

INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length], [Disporder], x, y)
VALUES (@nProductDescid, 'USER', 'Product', 1, 100, 24, 1, 20)
INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05)
SELECT @nProductDescid, 'Produit', 'Product', 'Product', 'Product', 'Product', 'Product'

-- Bonus

UPDATE [DESC] SET [Columns] = '100,A,25,140,A,25' WHERE DescId = 101000