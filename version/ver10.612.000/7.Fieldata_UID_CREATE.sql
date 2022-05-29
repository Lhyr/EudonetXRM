SET NOCOUNT ON

declare @isUnicode bit
select @isUnicode = count(1) where exists (select 1 from CONFIGADV where Parameter = 'FULL_UNICODE' and isnull([Value], 0) = 1)

IF NOT EXISTS (select 1
		  from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		  where sys.tables.name like 'FILEDATA' and syscolumns.name like 'UID')
BEGIN
	IF @isUnicode = 1
	BEGIN
		ALTER TABLE [FILEDATA] ADD [UID] NVARCHAR(50) NULL
	END
	ELSE
	BEGIN
		ALTER TABLE [FILEDATA] ADD [UID] VARCHAR(50) NULL
	END
END

SET NOCOUNT OFF