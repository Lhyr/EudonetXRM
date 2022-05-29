/* 	GCH
	Passage des champs BODY de formulaire de varchar/text à varbinary
	05/08/2014
*/
set nocount on;

declare @TypeName as varchar(50)
declare @nTab int, @nShortDescId int
set @nTab = 113000

--BODY****************************************************************************************
-->DESC
set @nShortDescId = 7
update [desc] set [format]=21,[length]=-1 where descid = @nTab+@nShortDescId;

-->SQL DROP
set @TypeName = ''
SELECT @TypeName = ST.name FROM SYSOBJECTS OBJ
	inner join sys.columns COL on COL.object_id = OBJ.id
	inner join sys.types ST on COL.system_type_id = ST.system_type_id
where OBJECTPROPERTY(OBJ.id, N'IsUserTable') = 1
	and OBJ.id = object_id(N'[dbo].[FORMULARXRM]')
	and COL.name = 'Body'
IF (@TypeName != 'varbinary' and isnull(@TypeName,'')!='')
BEGIN
	Alter Table [dbo].[FORMULARXRM] DROP Column [Body];
END
-->SQL ADD
IF NOT EXISTS(
	SELECT OBJ.* FROM SYSOBJECTS OBJ
		inner join sys.columns COL on COL.object_id = OBJ.id
	where OBJECTPROPERTY(OBJ.id, N'IsUserTable') = 1
		and OBJ.id = object_id(N'[dbo].[FORMULARXRM]')
		and COL.name = 'Body'
		)
begin
	ALTER TABLE [dbo].[FORMULARXRM] ADD [Body] varbinary(max) NULL;
end


--SUBMISSION_BODY*****************************************************************************
-->DESC
set @nShortDescId = 9
update [desc] set [format]=21,[length]=-1 where descid = @nTab+@nShortDescId;

-->SQL DROP
set @TypeName = ''
SELECT @TypeName = ST.name FROM SYSOBJECTS OBJ
	inner join sys.columns COL on COL.object_id = OBJ.id
	inner join sys.types ST on COL.system_type_id = ST.system_type_id
where OBJECTPROPERTY(OBJ.id, N'IsUserTable') = 1
	and OBJ.id = object_id(N'[dbo].[FORMULARXRM]')
	and COL.name = 'SubmissionBody'
IF (@TypeName != 'varbinary' and isnull(@TypeName,'')!='')
BEGIN
	Alter Table [dbo].[FORMULARXRM] DROP Column [SubmissionBody];
END
-->SQL ADD
IF NOT EXISTS(
	SELECT OBJ.* FROM SYSOBJECTS OBJ
		inner join sys.columns COL on COL.object_id = OBJ.id
	where OBJECTPROPERTY(OBJ.id, N'IsUserTable') = 1
		and OBJ.id = object_id(N'[dbo].[FORMULARXRM]')
		and COL.name = 'SubmissionBody'
		)
begin
	ALTER TABLE [dbo].[FORMULARXRM] ADD [SubmissionBody] varbinary(max) NULL;
end
