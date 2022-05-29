-- OPTIM 2
IF NOT EXISTS (SELECT 1 FROM SYS.indexes where name='OPTIM_IDX_GROUP_USER' AND  OBJECT_NAME(object_id) = 'USER')
	AND  [dbo].[efc_IsIndexExists]('USER','GroupId','UserId') = 0
BEGIN 
	CREATE NONCLUSTERED INDEX [OPTIM_IDX_GROUP_USER] ON [dbo].[USER]
		([GroupId] ASC)
		INCLUDE ([UserId])
END

-- OPTIM 3
IF NOT EXISTS (SELECT 1 FROM SYS.indexes where name='OPTIM_IDX_POPUP' AND  OBJECT_NAME(object_id) = 'DESC')
	AND  [dbo].[efc_IsIndexExists]('DESC','Popup;DescId','File;Field;PopupDescId;Relation') = 0
BEGIN 
	CREATE NONCLUSTERED INDEX [OPTIM_IDX_POPUP] ON [dbo].[DESC]
		([Popup], [DescId])
		INCLUDE ([File],[Field],[PopupDescId],[Relation]) 
END

 -- OPTIM 5
IF NOT EXISTS (SELECT 1 FROM SYS.indexes where name='OPTIM_IDX_USERVALUE' AND  OBJECT_NAME(object_id) = 'USERVALUE')
	AND  [dbo].[efc_IsIndexExists]('USERVALUE','Tab;Type;UserId','Value;Label;Index') = 0
BEGIN 
	CREATE NONCLUSTERED INDEX [OPTIM_IDX_USERVALUE] ON [dbo].[USERVALUE]
		([Tab] ASC,	[Type] ASC,	[UserId] ASC)
		INCLUDE ([Value],[Label],[Index]) 
END

-- OPTIM 7
IF NOT EXISTS (SELECT 1 FROM SYS.indexes where name='OPTIM_IDX_TRAIT' AND  OBJECT_NAME(object_id) = 'TRAIT')
	AND  [dbo].[efc_IsIndexExists]('TRAIT','TraitId','RulesId') = 0
BEGIN 
	CREATE NONCLUSTERED INDEX [OPTIM_IDX_TRAIT] ON [dbo].[TRAIT]
		([TraitId] ASC)
		INCLUDE ([RulesId]) 
END

-- OPTIM 8
IF NOT EXISTS (SELECT 1 FROM SYS.indexes where name='OPTIM_IDX_USER' AND  OBJECT_NAME(object_id) = 'USER')
	AND  [dbo].[efc_IsIndexExists]('USER','UserId','GroupId;UserLevel;UserLogin') = 0
BEGIN 
	CREATE NONCLUSTERED INDEX [OPTIM_IDX_USER] ON [dbo].[USER]
		([UserId] ASC)
		INCLUDE ([GroupId],[UserLevel],[UserLogin]) 
END

-- AJOUT DE CLE PRIMAIRE MANQUANTES
IF NOT EXISTS (SELECT 1 FROM SYS.indexes where name='PK_SELECTIONS' AND  OBJECT_NAME(object_id) = 'SELECTIONS')
	AND [dbo].[efc_IsIndexExists]('SELECTIONS','SelectId','') = 0
BEGIN 
	ALTER TABLE [dbo].[SELECTIONS] ADD CONSTRAINT [PK_SELECTIONS] PRIMARY KEY CLUSTERED (SelectId) ON [PRIMARY]
END

IF NOT EXISTS (SELECT 1 FROM SYS.indexes where name='PK_MARKEDFILE' AND  OBJECT_NAME(object_id) = 'MARKEDFILE')
	AND [dbo].[efc_IsIndexExists]('MARKEDFILE','MarkedFileId','') = 0
BEGIN 
	ALTER TABLE [dbo].[MARKEDFILE] ADD CONSTRAINT [PK_MARKEDFILE] PRIMARY KEY CLUSTERED (MarkedFileId) ON [PRIMARY]
END

