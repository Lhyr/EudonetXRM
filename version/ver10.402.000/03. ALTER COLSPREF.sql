IF NOT EXISTS (SELECT t.name, c.name FROM sys.tables t INNER JOIN sys.columns c ON t.object_id = c.object_id WHERE t.name = 'COLSPREF' and c.name LIKE 'XrmWidgetId')
BEGIN
	-- Création de la nouvelle colonne COLSPREF.XrmWidgetId
	ALTER TABLE [COLSPREF] ADD [XrmWidgetId] NUMERIC(18,0) NULL
	
	-- Ajout de la contrainte de clé étrangère, avec suppression en cascade
	ALTER TABLE [dbo].[COLSPREF]  WITH CHECK ADD CONSTRAINT [FK_COLSPREF_XrmWidget] FOREIGN KEY([xrmwidgetid])
	REFERENCES [dbo].[XrmWidget] ([XrmWidgetId])
	ON DELETE CASCADE
	
	-- On re-crée l'index pour ajouter la colonne XrmWidgetId
	IF EXISTS(SELECT object_id FROM sys.indexes WHERE name='IX_COLSPREF_KEY' AND object_id = OBJECT_ID('dbo.COLSPREF'))
		DROP INDEX [IX_COLSPREF_KEY] ON [dbo].[COLSPREF]
	CREATE UNIQUE NONCLUSTERED INDEX [IX_COLSPREF_KEY] ON [dbo].[COLSPREF]
	(
		[userid] ASC,
		[tab] ASC,
		[bkmtab] ASC,
		[colspreftype] ASC,
		[XrmWidgetId] ASC
	) WITH (
		PAD_INDEX = ON, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, 
		ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90
	) ON [PRIMARY]

END

