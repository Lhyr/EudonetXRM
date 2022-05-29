DECLARE @tabInteraction AS INT = 118000

--Rubrique Historique
UPDATE [PREF] SET
[HistoDescId] = @tabInteraction + 4
WHERE [Tab] = @tabInteraction
AND [UserId] = 0


--Taille des colonnes
UPDATE [DESC] SET
[Columns] = '210,A,25,210,A,25'
WHERE [DescId] = @tabInteraction


/*********************/
/* Permission import */
/*********************/

--Suppression des anciennes permissions si existantes
DECLARE @rightImportTab AS INT = 18 
DECLARE @rightImportBkm AS INT = 19

IF NOT EXISTS (SELECT (1) from TRAIT where  traitid = (@tabInteraction + @rightImportTab) and isnull(PermId,0) <>0)
BEGIN

DELETE FROM [TRAIT]
WHERE [TraitId] = (@tabInteraction + @rightImportTab)

--Insertion des nouvelles permissions
DECLARE @permissionIdTab AS NUMERIC(18)
DECLARE @traitIdTab AS NUMERIC(18)
INSERT INTO [PERMISSION] ([Mode], [Level], [User])
VALUES (1, 0, '')
SELECT @permissionIdTab = SCOPE_IDENTITY()

IF ISNULL(@permissionIdTab, 0) <> 0
BEGIN
	INSERT INTO [TRAIT] ([TraitId], [UserLevel], [TraitLevel], [PermId], [Sort])
	VALUES (@tabInteraction + @rightImportTab, 0, 1, @permissionIdTab, @rightImportTab)	
END
END

IF NOT EXISTS (SELECT (1) from TRAIT where  traitid = (@tabInteraction + @rightImportBkm) and isnull(PermId,0) <> 0)
BEGIN
--Insertion des nouvelles permissions
DELETE FROM [TRAIT]
WHERE [TraitId] = (@tabInteraction + @rightImportBkm)

DECLARE @permissionIdBkm AS NUMERIC(18)
DECLARE @traitIdBkm AS NUMERIC(18)

INSERT INTO [PERMISSION] ([Mode], [Level], [User])
VALUES (1, 0, '')
SELECT @permissionIdBkm = SCOPE_IDENTITY()

IF ISNULL(@permissionIdBkm, 0) <> 0
BEGIN
	INSERT INTO [TRAIT] ([TraitId], [UserLevel], [TraitLevel], [PermId], [Sort])
	VALUES (@tabInteraction + @rightImportBkm, 0, 1, @permissionIdBkm, @rightImportBkm)
END
END
