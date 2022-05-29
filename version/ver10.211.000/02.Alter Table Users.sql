
-- Modifié le 
IF NOT EXISTS ( SELECT 1
		  from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		  where sys.tables.name like 'USER' and syscolumns.name like 'UserDisplayName')
BEGIN            
	ALTER TABLE [USER] ADD [UserDisplayName] as IsNull(UserName, UserLogin)
END

IF NOT EXISTS ( SELECT 1 FROM [DESC] WHERE [DESCID] = 101030)
	INSERT INTO[DESC] ([DESCID], [FILE], [FIELD], [FORMAT], [LENGTH], [DispOrder]) SELECT 101030, 'USER', 'UserDisplayName', 1, 800,0

 
 
 declare @IsComputed as bit
 SELECT  @IsComputed= iscomputed
		  from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		  where sys.tables.name like 'USER' and syscolumns.name like 'UserDisplayName'
		  
IF @IsComputed = 1
BEGIN

	UPDATE [DESC] SET [ReadOnly] = 1 where descid=101030
END


IF NOT EXISTS ( SELECT 1 FROM [RES] WHERE [RESID] = 101030)
	INSERT INTO [RES] (resid, lang_00, lang_01 ) select 101030, 'Nom Complet', ''
 
 

 
-- Langue
update [DESC] set Obligat=1 where DescId=101023


 
	
IF NOT EXISTS ( SELECT 1 FROM [RES] WHERE [RESID] = 103002)
	INSERT INTO [RES] (resid, lang_00, lang_01 ) select 103002, 'GroupLevel', ''
	
 IF NOT EXISTS ( SELECT 1 FROM [DESC] WHERE [DESCID] = 103002)
	INSERT INTO[DESC] ([DESCID], [FILE], [FIELD], [FORMAT], [LENGTH], [DispOrder]) SELECT 103002, 'GROUP', 'GroupLevel', 1, 800,0
	
	
	IF NOT EXISTS ( SELECT 1 FROM [RES] WHERE [RESID] = 103003)
	INSERT INTO [RES] (resid, lang_00, lang_01 ) select 103003, 'Groupe Public', ''
	
 IF NOT EXISTS ( SELECT 1 FROM [DESC] WHERE [DESCID] = 103003)
	INSERT INTO[DESC] ([DESCID], [FILE], [FIELD], [FORMAT], [LENGTH], [DispOrder]) SELECT 103003, 'GROUP', 'GroupPublic', 3, 1,0