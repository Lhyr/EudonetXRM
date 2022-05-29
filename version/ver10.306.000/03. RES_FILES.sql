-- Création de la table RES_FILES si elle n'existe pas
-- Cette table permet de stocker les traductions des valeurs de fiches
IF NOT EXISTS (SELECT 1
		from  sys.tables
		where sys.tables.name = 'RES_FILES')
BEGIN
	CREATE TABLE [dbo].[RES_FILES](

		[ID] [int] IDENTITY(1,1) NOT NULL,
		[TAB] [int] NOT NULL,
		[DESCID] [int] NOT NULL,
		[FILEID] [int] NOT NULL,
		[ID_LANG] [int] NOT NULL,
		[LANG] [nvarchar](max) NULL
	)  
END
