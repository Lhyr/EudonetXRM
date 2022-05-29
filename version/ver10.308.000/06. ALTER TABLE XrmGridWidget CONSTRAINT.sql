-- Suppression de la clé étrangère existante 
--IF (OBJECT_ID('FK_XrmWidgetPref', 'F') IS NOT NULL)
	--ALTER TABLE [dbo].[XrmGridWidget] DROP CONSTRAINT FK_XrmWidgetPref

-- Nettoyage des relations Widget/Grid pour des widgets ou grids qui n'existent plus
DELETE FROM XRMGRIDWIDGET WHERE XrmGridId NOT IN (SELECT XrmGridId FROM XrmGrid)
DELETE FROM XRMGRIDWIDGET WHERE XrmWidgetId NOT IN (SELECT XrmWidgetId FROM XrmWidget)

-- Création de la clé étrangère sur XrmGridId
IF (OBJECT_ID('FK_XrmGridWidget_XrmGrid', 'F') IS NULL)
BEGIN
	ALTER TABLE [dbo].[XrmGridWidget]  WITH CHECK ADD  CONSTRAINT [FK_XrmGridWidget_XrmGrid] FOREIGN KEY([XrmGridId]) REFERENCES [dbo].[XrmGrid] ([XrmGridId]) 
	ALTER TABLE [dbo].[XrmGridWidget] CHECK CONSTRAINT [FK_XrmGridWidget_XrmGrid]
END

IF (OBJECT_ID('FK_XrmGridWidget_XrmWidget', 'F') IS NULL)
BEGIN
	-- Création de la clé étrangère sur XrmWidgetId
	ALTER TABLE [dbo].[XrmGridWidget]  WITH CHECK ADD  CONSTRAINT [FK_XrmGridWidget_XrmWidget] FOREIGN KEY([XrmWidgetId]) REFERENCES [dbo].[XrmWidget] ([XrmWidgetId]) 
	ALTER TABLE [dbo].[XrmGridWidget] CHECK CONSTRAINT [FK_XrmGridWidget_XrmWidget]
END
