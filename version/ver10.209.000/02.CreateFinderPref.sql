IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE sys.tables.name = 'FINDERPREF')
BEGIN
	CREATE TABLE [dbo].[FINDERPREF]
	(
		[finderid] [int] IDENTITY(1,1) NOT NULL,
		[userid] [numeric](18, 0) NOT NULL DEFAULT ((0)),
		[tab] [numeric](18, 0) NOT NULL DEFAULT ((0)),
		[findercol] [varchar](1000) NULL,
		[findercolwidth] [varchar](1000) NULL,
		[findersort] [varchar](20) NULL,
		[finderorder] [varchar](10) NULL,
		[finderfilterCol] [varchar](250) NULL,
		[finderfilterOp] [varchar](250) NULL,
		[finderfilterValue] [nvarchar](500) NULL
		CONSTRAINT [PK_FINDERPREF] PRIMARY KEY CLUSTERED ([finderid] ASC)
		WITH (PAD_INDEX = ON, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
	) ON [PRIMARY]

	CREATE UNIQUE NONCLUSTERED INDEX [CleEudo] ON [dbo].[FINDERPREF]
	(
		[userid] ASC,
		[tab] ASC
	) WITH (
		PAD_INDEX = ON, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, 
		ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90
	) ON [PRIMARY]
END

-- Ajout d'une colonne regoupant les informations de taille et de filtre express
IF NOT EXISTS (SELECT 1 FROM sys.columns sc INNER JOIN sys.tables st ON st.object_id = sc.object_id AND st.NAME = 'FINDERPREF' WHERE sc.NAME = 'findercoloptions')
BEGIN
	ALTER TABLE dbo.FINDERPREF ADD findercoloptions varchar(MAX) NULL
END

-- Suppresion des anciennes colonnes non utile
IF EXISTS (SELECT 1 FROM sys.columns sc INNER JOIN sys.tables st ON st.object_id = sc.object_id AND st.NAME = 'FINDERPREF' WHERE sc.NAME = 'finderfilterCol')
BEGIN
	ALTER TABLE dbo.FINDERPREF DROP COLUMN finderfilterCol 
END

IF EXISTS (SELECT 1 FROM sys.columns sc INNER JOIN sys.tables st ON st.object_id = sc.object_id AND st.NAME = 'FINDERPREF' WHERE sc.NAME = 'finderfilterOp')
BEGIN
	ALTER TABLE dbo.FINDERPREF DROP COLUMN finderfilterOp 
END

IF EXISTS (SELECT 1 FROM sys.columns sc INNER JOIN sys.tables st ON st.object_id = sc.object_id AND st.NAME = 'FINDERPREF' WHERE sc.NAME = 'finderfilterValue')
BEGIN
	ALTER TABLE dbo.FINDERPREF DROP COLUMN finderfilterValue 
END

-- Finalement on sépare les informations de taille et de filtre express
IF EXISTS (SELECT 1 FROM sys.columns sc INNER JOIN sys.tables st ON st.object_id = sc.object_id AND st.NAME = 'FINDERPREF' WHERE sc.NAME = 'findercoloptions')
BEGIN
	ALTER TABLE dbo.FINDERPREF DROP COLUMN findercoloptions 
END

IF EXISTS (SELECT 1 FROM sys.columns sc INNER JOIN sys.tables st ON st.object_id = sc.object_id AND st.NAME = 'FINDERPREF' WHERE sc.NAME = 'findercolwidth')
BEGIN
	ALTER TABLE dbo.FINDERPREF ALTER COLUMN findercolwidth varchar(MAX) NULL
END

IF NOT EXISTS (SELECT 1 FROM sys.columns sc INNER JOIN sys.tables st ON st.object_id = sc.object_id AND st.NAME = 'FINDERPREF' WHERE sc.NAME = 'finderfilteroptions')
BEGIN
	ALTER TABLE dbo.FINDERPREF ADD finderfilteroptions varchar(MAX) NULL
END
