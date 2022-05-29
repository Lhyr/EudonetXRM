-- MAB - Backlog #1041 - Volumétrie E-mail - Passage de la colonne LastSynchronized en nullable

IF EXISTS(SELECT * FROM SYSOBJECTS where id = object_id(N'[dbo].[EDNMSGCMPT]') and OBJECTPROPERTY(id, N'IsUserTable') = 1 )
BEGIN
	ALTER TABLE [dbo].[EDNMSGCMPT]
	ALTER COLUMN [LastSynchronized] [DATETIME] NULL
END