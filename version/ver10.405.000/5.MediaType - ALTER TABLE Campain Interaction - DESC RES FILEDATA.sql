DECLARE @campaignMediaType int = 106046
DECLARE @interactionMediaType int = 118017

--Creation du champs sur la table Campagne
IF NOT EXISTS (SELECT 1
		FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		WHERE sys.tables.name like 'CAMPAIGN' and syscolumns.name like 'MediaType' )
BEGIN
	ALTER TABLE [CAMPAIGN] ADD MediaType VARCHAR(100) NULL
END

--Creation du champs sur la table Interaction
IF NOT EXISTS (SELECT 1
		FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		WHERE sys.tables.name like 'INTERACTION' and syscolumns.name like 'TPL17' )
BEGIN
	ALTER TABLE [INTERACTION] ADD [TPL17] VARCHAR(100) NULL
END


--Creation des desc
IF NOT EXISTS (SELECT 1 FROM [DESC] WHERE [DescId] = @campaignMediaType)
BEGIN
	INSERT INTO [DESC] ([DescId], [File], [Field])
	VALUES (@campaignMediaType, 'CAMPAIGN', 'MediaType')
END

IF NOT EXISTS (SELECT 1 FROM [DESC] WHERE [DescId] = @interactionMediaType)
BEGIN
	INSERT INTO [DESC] ([DescId], [File], [Field])
	VALUES (@interactionMediaType, 'INTERACTION', 'TPL17')
END

UPDATE [DESC] SET
[Format] = 1
,[Length] = 100
,[Popup] = 3
,[PopupDescId] = @campaignMediaType
,[Multiple] = 0

,[Case] = 0
,[Historic] = 0
,[Obligat] = 0
,[ActiveTab] = 0
,[ActiveBkmPP] = 1
,[ActiveBkmPM] = 1
,[ActiveBkmEvent] = 1
,[GetLevel] = 1
,[ViewLevel] = 1
,[UserLevel] = 0
,[InterPP] = 0
,[InterPM] = 0
,[InterEvent] = 0
,[TabIndex] = 0
,[Bold] = 0
,[Italic] = 0
,[Underline] = 0
,[Flat] = 0
,[Disabled] = 0
,[Unicode] = 0
,[NbrBkmInline] = 0
,[TreatmentMaxRows] = 0
,[TreeViewUserList] = 0
WHERE [DescId] IN (@campaignMediaType, @interactionMediaType)

UPDATE [DESC] SET
[BreakLine] = ( SELECT [BreakLine] FROM [DESC] AS [DESC_TAB] WHERE [DESC_TAB].[DescId] = ([DESC].[DescId] - ([DESC].[DescId] % 100)) )
WHERE [DescId] IN (@campaignMediaType, @interactionMediaType)

UPDATE [DESC] SET
[FullUserList] = 0
WHERE [DescId] IN (@campaignMediaType)

UPDATE [DESC] SET
[FullUserList] = 1
WHERE [DescId] IN (@interactionMediaType)

--Creation des res
IF NOT EXISTS (SELECT 1 FROM [RES] WHERE [ResId] = @campaignMediaType)
BEGIN
	INSERT INTO [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05]) 
	VALUES (@campaignMediaType, 'Type de média', 'Media Type', 'Media Type', 'Media Type', 'Media Type', 'Media Type')
END

IF NOT EXISTS (SELECT 1 FROM [RES] WHERE [ResId] = @interactionMediaType)
BEGIN
	INSERT INTO [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05]) 
	VALUES (@interactionMediaType, 'Type de média', 'Media Type', 'Media Type', 'Media Type', 'Media Type', 'Media Type')
END

--Creation des FileDataParam
IF NOT EXISTS (SELECT 1 FROM [FileDataParam] WHERE [DescId] = @campaignMediaType)
BEGIN
	INSERT INTO [FileDataParam] ([DescId], [DataEnabled], [SortEnabled], [LangUsed], [DisplayMask], [SortBy], [TreeView], [DataAutoEnabled], [DataAutoStart], [NoAutoLoad], [SearchLimit], [TreeViewOnlyLastChildren]) 
	VALUES (@campaignMediaType, 1, 0, '0;1;2;3;4;5', '[TEXT]', '[TEXT]', 0, 0, 0, 0, 0, 0)
END

IF NOT EXISTS (SELECT 1 FROM [FileDataParam] WHERE [DescId] = @interactionMediaType)
BEGIN
	INSERT INTO [FileDataParam] ([DescId], [DataEnabled], [SortEnabled], [LangUsed], [DisplayMask], [SortBy], [TreeView], [DataAutoEnabled], [DataAutoStart], [NoAutoLoad], [SearchLimit], [TreeViewOnlyLastChildren]) 
	VALUES (@interactionMediaType, 1, 0, '0;1;2;3;4;5', '[TEXT]', '[TEXT]', 0, 0, 0, 0, 0, 0)
END

--Creation des permissions
DECLARE @nAddPermissionId numeric(18)
DECLARE @nUpdatePermissionId numeric(18)
DECLARE @nDeletePermissionId numeric(18)
	
IF NOT EXISTS (SELECT 1 FROM [FileDataParam] WHERE [DescId] = @campaignMediaType AND [AddPermission] IS NOT NULL AND [UpdatePermission] IS NOT NULL AND [DeletePermission] IS NOT NULL)
BEGIN
	select @nAddPermissionId = null, @nUpdatePermissionId = null, @nDeletePermissionId = null
	insert into [PERMISSION] ([Mode], [Level], [User]) values (1, 0, '')
	select @nAddPermissionId = scope_identity()
	insert into [PERMISSION] ([Mode], [Level], [User]) values (1, 0, '')
	select @nUpdatePermissionId = scope_identity()
	insert into [PERMISSION] ([Mode], [Level], [User]) values (1, 0, '')
	select @nDeletePermissionId = scope_identity()

	update [FILEDATAPARAM] set 
	[AddPermission] = @nAddPermissionId
	,[UpdatePermission] = @nUpdatePermissionId
	,[DeletePermission] = @nDeletePermissionId
	where [DescId] = @campaignMediaType
END

IF NOT EXISTS (SELECT 1 FROM [FileDataParam] WHERE [DescId] = @interactionMediaType AND [AddPermission] IS NOT NULL AND [UpdatePermission] IS NOT NULL AND [DeletePermission] IS NOT NULL)
BEGIN
	select @nAddPermissionId = null, @nUpdatePermissionId = null, @nDeletePermissionId = null
	insert into [PERMISSION] ([Mode], [Level], [User]) values (1, 0, '')
	select @nAddPermissionId = scope_identity()
	insert into [PERMISSION] ([Mode], [Level], [User]) values (1, 0, '')
	select @nUpdatePermissionId = scope_identity()
	insert into [PERMISSION] ([Mode], [Level], [User]) values (1, 0, '')
	select @nDeletePermissionId = scope_identity()

	update [FILEDATAPARAM] set 
	[AddPermission] = @nAddPermissionId
	,[UpdatePermission] = @nUpdatePermissionId
	,[DeletePermission] = @nDeletePermissionId
	where [DescId] = @interactionMediaType
END

--Creation des valeurs du catalogue
DECLARE @nEmailingCatValue numeric(18)

IF NOT EXISTS (SELECT 1 FROM [FILEDATA] WHERE [DescId] = @campaignMediaType)
BEGIN
	INSERT INTO [FILEDATA] ([DescId], [Data], [Lang_00], [Lang_01], [Lang_02], [Lang_03], [Lang_04], [Lang_05])
	VALUES (@campaignMediaType, 'email', 'Emailing', 'Emailing', 'Emailing', 'Emailing', 'Emailing', 'Emailing')
	select @nEmailingCatValue = scope_identity()
	
	INSERT INTO [FILEDATA] ([DescId], [Data], [Lang_00], [Lang_01], [Lang_02], [Lang_03], [Lang_04], [Lang_05])
	VALUES (@campaignMediaType, 'sms', 'SMS', 'SMS', 'SMS', 'SMS', 'SMS', 'SMS')
	INSERT INTO [FILEDATA] ([DescId], [Data], [Lang_00], [Lang_01], [Lang_02], [Lang_03], [Lang_04], [Lang_05])
	VALUES (@campaignMediaType, 'phone', 'Téléphone', 'Phone', 'Phone', 'Phone', 'Phone', 'Phone')
	INSERT INTO [FILEDATA] ([DescId], [Data], [Lang_00], [Lang_01], [Lang_02], [Lang_03], [Lang_04], [Lang_05])
	VALUES (@campaignMediaType, 'mail', 'Courrier', 'Mail', 'Mail', 'Mail', 'Mail', 'Mail')

	UPDATE [FILEDATA] SET 
	[Tip_Lang_00_Format] = 0
	,[Tip_Lang_01_Format] = 0
	,[Tip_Lang_02_Format] = 0
	,[Tip_Lang_03_Format] = 0
	,[Tip_Lang_04_Format] = 0
	,[Tip_Lang_05_Format] = 0
	,[Tip_Lang_06_Format] = 0
	,[Tip_Lang_07_Format] = 0
	,[Tip_Lang_08_Format] = 0
	,[Tip_Lang_09_Format] = 0
	,[Tip_Lang_10_Format] = 0
	WHERE [DescId] = @campaignMediaType	
END


--Modification des catalogues "Type de campagne" pour les lier
DECLARE @campaignCategory int = 106008
DECLARE @interactionCampaignType int = 118009

UPDATE [DESC] SET
[BoundDescId] = @campaignMediaType
WHERE [DescId] = @campaignCategory

UPDATE [DESC] SET
[BoundDescId] = @interactionMediaType
WHERE [DescId] = @interactionCampaignType

IF @nEmailingCatValue IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM [FILEDATA] WHERE [DescId] = @campaignCategory AND [ParentDataId] IS NOT NULL)
BEGIN
	UPDATE [FILEDATA] SET
	[ParentDataId] = @nEmailingCatValue
	WHERE [DescId] = @campaignCategory
END


--Mise à jour Mise en page
UPDATE [DESC] SET [DispOrder] = 8, x = 0, y = 4 WHERE [DescId] = @interactionMediaType
UPDATE [DESC] SET [DispOrder] = 9, x = 1, y = 4 WHERE [DescId] = 118009
UPDATE [DESC] SET [DispOrder] = 10, x = 0, y = 5 WHERE [DescId] = 118008
UPDATE [DESC] SET [DispOrder] = 11, x = 1, y = 5 WHERE [DescId] = 118010
UPDATE [DESC] SET [DispOrder] = 12, x = 0, y = 6 WHERE [DescId] = 118011
UPDATE [DESC] SET [DispOrder] = 13, x = 1, y = 6 WHERE [DescId] = 118012
UPDATE [DESC] SET [DispOrder] = 14, x = 0, y = 7 WHERE [DescId] = 118013
UPDATE [DESC] SET [DispOrder] = 15, x = 1, y = 7 WHERE [DescId] = 118014
UPDATE [DESC] SET [DispOrder] = 16, x = 0, y = 8 WHERE [DescId] = 118015
UPDATE [DESC] SET [DispOrder] = 18, x = 0, y = 12 WHERE [DescId] = 118016
UPDATE [DESC] SET [DispOrder] = 20 WHERE [DescId] = 118094