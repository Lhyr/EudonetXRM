/* ID Groupe */
declare @nFieldIdGroup INT = 101027
if exists(select * from [RES] where [ResId] = @nFieldIdGroup )
begin
	UPDATE [RES] SET
	[LANG_00] = 'ID Groupe'
	,[LANG_01] = 'Group ID'
	,[LANG_02] = 'Group ID'
	,[LANG_03] = 'Group ID'
	,[LANG_04] = 'Group ID'
	,[LANG_05] = 'Group ID'
	WHERE [ResId] = @nFieldIdGroup
end
else
begin
	INSERT INTO [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05])
	VALUES (@nFieldIdGroup, 'ID Groupe', 'Group ID', 'Group ID', 'Group ID', 'Group ID', 'Group ID')
end


/* Nom affiché */
declare @nFieldDisplayName INT = 101030
if exists(select * from [RES] where [ResId] = @nFieldDisplayName )
begin
	UPDATE [RES] SET
	[LANG_00] = 'Nom affiché'
	,[LANG_01] = 'Display name'
	,[LANG_02] = 'Display name'
	,[LANG_03] = 'Display name'
	,[LANG_04] = 'Display name'
	,[LANG_05] = 'Display name'
	WHERE [ResId] = @nFieldDisplayName
end
else
begin
	INSERT INTO [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05])
	VALUES (@nFieldDisplayName, 'Nom affiché', 'Display name', 'Display name', 'Display name', 'Display name', 'Display name')
end