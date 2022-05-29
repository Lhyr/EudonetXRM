--------------- CREATION DE LA GRILLE DE REPORTING RGPD DANS TABLEAU DE BORD EN ADMINISTRATION ---------------

DECLARE @parentSystemTab INT = 0 -- XrmGrid
DECLARE @parentFileId INT = 1
DECLARE @gridId INT = 0
DECLARE @widgetId INT = 0
DECLARE @XrmWidgetDescId INT = 115100
DECLARE @XrmWidgetTitleDescId INT = @XrmWidgetDescId + 1
DECLARE @XrmWidgetSubtitleDescId INT = @XrmWidgetDescId + 2

-- Suppression de la grille et de ses dépendances si elle existe déjà
SELECT @gridId = XrmGridId FROM [XrmGrid] WHERE ParentTab = @parentSystemTab AND ParentFileId = @parentFileId
IF @gridId <> 0
BEGIN
	-- Premier nettoyage des widgets non rattachés (à cause de la contrainte)
	DELETE FROM [XrmWidget] WHERE XrmWidgetId NOT IN (SELECT XrmWidgetId FROM [XrmGridWidget])
	
	DELETE FROM [XrmGridWidget] WHERE XrmGridId = @gridId
	-- Deuxième nettoyage pour la relation venant d'être supprimée
	DELETE FROM [XrmWidget] WHERE XrmWidgetId NOT IN (SELECT XrmWidgetId FROM [XrmGridWidget])
	DELETE FROM [XrmWidgetParam] WHERE XrmWidgetId NOT IN (SELECT XrmWidgetId FROM [XrmWidget])
	DELETE FROM [RES_FILES] WHERE [TAB] = @XrmWidgetDescId AND [DESCID] IN (@XrmWidgetTitleDescId, @XrmWidgetSubtitleDescId) AND [FILEID] NOT IN (SELECT XrmWidgetId FROM [XrmWidget])
	
	DELETE FROM [XrmGrid] WHERE XrmGridId = @gridId
END

-- Création de la grille
INSERT INTO [XrmGrid] (ParentTab, ParentFileId, Title, XrmGrid95) VALUES (@parentSystemTab, @parentFileId, 'Reporting RPGD', GETDATE())
SELECT @gridId = SCOPE_IDENTITY()

-- Création des widgets

-- Widget indicateur
INSERT INTO [XrmWidget] (Title, SubTitle, [Type], DefaultPosX, DefaultPosY, DefaultWidth, DefaultHeight, ShowHeader, ShowWidgetTitle, XrmWidget95) VALUES
('Nombre de champs', 'Champs de type RGPD', 14, 0, 0, 3, 3, 1, 1, GETDATE())
SELECT @widgetId = SCOPE_IDENTITY()
INSERT INTO [XrmGridWidget] (XrmWidgetId, XrmGridId) VALUES (@widgetId, @gridId)
INSERT INTO [XrmWidgetParam] (XrmWidgetId, ParamName, ParamValue) VALUES (@widgetId, 'countQuery', 'SELECT COUNT(DISTINCT Descid) FROM [DESCADV] WHERE Parameter = 21 and Value = ''1''')
INSERT INTO [XrmWidgetParam] (XrmWidgetId, ParamName, ParamValue) VALUES (@widgetId, 'libelle', 'Champs de type RGPD')
INSERT INTO [XrmWidgetParam] (XrmWidgetId, ParamName, ParamValue) VALUES (@widgetId, 'noAdmin', '1')
INSERT INTO [RES_FILES] ([TAB], [DESCID], [FILEID], [ID_LANG], [LANG]) VALUES (@XrmWidgetDescId, @XrmWidgetTitleDescId, @widgetId, 0, 'Nombre de champs')
INSERT INTO [RES_FILES] ([TAB], [DESCID], [FILEID], [ID_LANG], [LANG]) VALUES (@XrmWidgetDescId, @XrmWidgetTitleDescId, @widgetId, 1, 'N° of fields')
INSERT INTO [RES_FILES] ([TAB], [DESCID], [FILEID], [ID_LANG], [LANG]) VALUES (@XrmWidgetDescId, @XrmWidgetSubtitleDescId, @widgetId, 0, 'Champs de type RGPD')
INSERT INTO [RES_FILES] ([TAB], [DESCID], [FILEID], [ID_LANG], [LANG]) VALUES (@XrmWidgetDescId, @XrmWidgetSubtitleDescId, @widgetId, 1, 'GDPR-type fields')

-- Widget liste "Journal des logs"
INSERT INTO [XrmWidget] (Title, SubTitle, [Type], ContentSource, DefaultPosX, DefaultPosY, DefaultWidth, DefaultHeight, ShowHeader, ShowWidgetTitle, XrmWidget95) VALUES
('Journal des traitements archivés et supprimés', '', 4, '117000', 3, 0, 9, 3, 1, 1, GETDATE())
SELECT @widgetId = SCOPE_IDENTITY()
INSERT INTO [XrmGridWidget] (XrmWidgetId, XrmGridId) VALUES (@widgetId, @gridId)
INSERT INTO [XrmWidgetParam] (XrmWidgetId, ParamName, ParamValue) VALUES (@widgetId, 'showHeader', '0')
INSERT INTO [XrmWidgetParam] (XrmWidgetId, ParamName, ParamValue) VALUES (@widgetId, 'nbrows', '20')
INSERT INTO [XrmWidgetParam] (XrmWidgetId, ParamName, ParamValue) VALUES (@widgetId, 'listcol', '117001;117002;117003;117004;117005;117006')
INSERT INTO [XrmWidgetParam] (XrmWidgetId, ParamName, ParamValue) VALUES (@widgetId, 'noAdmin', '1')
INSERT INTO [RES_FILES] ([TAB], [DESCID], [FILEID], [ID_LANG], [LANG]) VALUES (@XrmWidgetDescId, @XrmWidgetTitleDescId, @widgetId, 0, 'Journal des traitements archivés et supprimés')
INSERT INTO [RES_FILES] ([TAB], [DESCID], [FILEID], [ID_LANG], [LANG]) VALUES (@XrmWidgetDescId, @XrmWidgetTitleDescId, @widgetId, 1, 'Deleted and archives process log')

-- Widgets params

-- Traductions