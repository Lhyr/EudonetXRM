SET NOCOUNT ON;

/* #64131 - CRU : Création d'une nouvelle colonne NotificationTrigger.BypassRights pour ne pas prendre en compte les droits lors de la création d'une notification */

DECLARE @nTab INT = 114200
DECLARE @nDescID INT = 114227

-- Création de la colonne SQL 
IF NOT EXISTS (SELECT 1
		  FROM sys.tables INNER JOIN syscolumns ON syscolumns.id = sys.tables.object_id
		  WHERE sys.tables.name LIKE 'NotificationTrigger' AND syscolumns.name LIKE 'BypassRights') 
	ALTER TABLE [NotificationTrigger] ADD [BypassRights] BIT NULL

-- Création dans DESC
IF NOT EXISTS (SELECT 1 FROM [DESC] WHERE [DESCID] = @nDescID)
	INSERT INTO [DESC] ([DESCID], [FILE], [FIELD], [FORMAT]) SELECT @nDescID, 'NotificationTrigger', 'BypassRights', 3
	
-- Création dans RES
IF NOT EXISTS (SELECT 1 FROM [RES] WHERE [RESID] = @nDescID)
	INSERT INTO [RES] (resid, lang_00, lang_01 ) select @nDescID, 'Pas de prise en compte des droits', ''

