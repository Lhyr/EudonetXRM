
set nocount on;

DECLARE @nTab INT = 101100 
DECLARE @nDescid INT = 101100 
DECLARE @ColName Varchar(500)

begin
    
	DELETE [res] where resid >= @nTab AND resid < @nTab + 100
	DELETE [FileDataParam] where DESCid >=@nTab AND DESCid < @nTab +100
	DELETE [UserValue] where DESCid >=@nTab AND DESCid < @nTab +100
	DELETE [DESCADV] where DESCid >=@nTab AND DESCid < @nTab +100
	DELETE [DESC] where DESCid >=@nTab AND DESCid < @nTab +100
			
	-- TABLE  
	INSERT INTO [DESC] ([DescId],[File], [Field], [Format], [Length], [type], [ActiveTab], [BreakLine]) SELECT @nTab, 'EXTENSION', 'EXTENSION', 0, 0, 0, 0, 25	
	INSERT INTO [res] (resid, lang_00 ) SELECT @nTab, 'EXTENSION' 
	
	-- EXTENSIONID
 /*
	set @ColName = 'EXTENSIONID'
	set @nTab = @nTab + 1
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) SELECT @nTab, 'EXTENSION', @ColName, 8, 0	
	INSERT INTO [res] (resid, lang_00 ) SELECT @nTab, @ColName
	*/ 
	

	-- EXTENSIONTYPE
	set @ColName = 'EXTENSIONTYPE'
	set @nTab = @nTab + 1
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) SELECT @nTab, 'EXTENSION', @ColName, 1, 50	
	INSERT INTO [res] (resid, lang_00 ) SELECT @nTab, @ColName

	

	-- EXTENSIONCODE
	set @ColName = 'EXTENSIONCODE'
	set @nTab = @nTab + 1
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) SELECT @nTab, 'EXTENSION', @ColName, 1, 1000	
	INSERT INTO [res] (resid, lang_00 ) SELECT @nTab, @ColName


	-- EXTENSIONSTATUS
	set @ColName = 'EXTENSIONSTATUS'
	set @nTab = @nTab + 1
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) SELECT @nTab, 'EXTENSION', @ColName, 1, 1000	
	INSERT INTO [res] (resid, lang_00 ) SELECT @nTab, @ColName


	-- EXTENSIONPARAM
	set @ColName = 'EXTENSIONPARAM'
	set @nTab = @nTab + 1
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) SELECT @nTab, 'EXTENSION', @ColName, 9, 0	
	INSERT INTO [res] (resid, lang_00 )  SELECT @nTab, @ColName

	-- EXTENSIONUSERID
	set @ColName = 'EXTENSIONUSERID'
	set @nTab = @nTab + 1
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) SELECT @nTab, 'EXTENSION', @ColName, 8, 0	
	INSERT INTO [res] (resid, lang_00 ) SELECT @nTab, @ColName



	-- EXTENSIONDATEENABLED
	set @ColName = 'EXTENSIONDATEENABLED'
	set @nTab = @nTab + 1
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) SELECT @nTab, 'EXTENSION', @ColName, 2, 0	
	INSERT INTO [res] (resid, lang_00 ) SELECT @nTab, @ColName



	-- EXTENSIONDATEDISABLED
	set @ColName = 'EXTENSIONDATEDISABLED'
	set @nTab = @nTab + 1
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) SELECT @nTab, 'EXTENSION', @ColName, 2, 0	
	INSERT INTO [res] (resid, lang_00 ) SELECT @nTab, @ColName

	-- EVT79
	set @ColName = 'EXTENSION79'
	set @nTab = @nDescid + 79
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) SELECT @nTab, 'EXTENSION', @ColName, 0, 0	
	INSERT INTO [res] (resid, lang_00 ) SELECT @nTab, @ColName

	
	-- EVT79
	set @ColName = 'EXTENSION84'
	set @nTab = @nDescid + 84
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) SELECT @nTab, 'EXTENSION', @ColName, 3, 0	
	INSERT INTO [res] (resid, lang_00 ) SELECT @nTab, @ColName

	-- EVT87
	set @ColName = 'EXTENSION87'
	set @nTab = @nDescid + 87
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) SELECT @nTab, 'EXTENSION', @ColName, 0, 0	
	INSERT INTO [res] (resid, lang_00 ) SELECT @nTab, @ColName


	-- EVT88
	set @ColName = 'EXTENSION88'
	set @nTab = @nDescid + 88
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) SELECT @nTab, 'EXTENSION', @ColName, 8, 0	
	INSERT INTO [res] (resid, lang_00 ) SELECT @nTab, @ColName

	-- EVT89
	set @ColName = 'EXTENSION89'
	set @nTab = @nDescid + 89
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) SELECT @nTab, 'EXTENSION', @ColName, 9, 0	
	INSERT INTO [res] (resid, lang_00 ) SELECT @nTab, @ColName

	-- EVT91
	set @ColName = 'EXTENSION91'
	set @nTab = @nDescid + 91
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) SELECT @nTab, 'EXTENSION', @ColName, 0, 0	
	INSERT INTO [res] (resid, lang_00 ) SELECT @nTab, @ColName


	-- EVT93
	set @ColName = 'EXTENSION93'
	set @nTab = @nDescid + 93
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) SELECT @nTab, 'EXTENSION', @ColName, 9, 0	
	INSERT INTO [res] (resid, lang_00 ) SELECT @nTab, @ColName

	-- EVT94
	set @ColName = 'EXTENSION94'
	set @nTab = @nDescid + 94
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) SELECT @nTab, 'EXTENSION', @ColName, 9, 0	
	INSERT INTO [res] (resid, lang_00 ) SELECT @nTab, @ColName

	-- 95
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) SELECT @nDescid + 95, 'EXTENSION', 'EXTENSION95', 2, 0
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) SELECT @nDescid + 95, 'Créé le', 'Created on', 'Created on', 'Created on', 'Created on', 'Created on'
	
	-- 96
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) SELECT @nDescid + 96, 'EXTENSION', 'EXTENSION96', 2, 0
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) SELECT @nDescid + 96, 'Modifié le', 'Modified on', 'Modified on', 'Modified on', 'Modified on', 'Modified on'	
	
	-- 97
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) SELECT @nDescid + 97, 'EXTENSION', 'EXTENSION97', 8, 0	
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) SELECT @nDescid + 97, 'Créé par', 'Created by', 'Created by', 'Created by', 'Created by', 'Created by'
	
	-- 98
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) SELECT @nDescid + 98, 'EXTENSION', 'EXTENSION98', 8, 0
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) SELECT @nDescid + 98, 'Modifié par', 'Modified by', 'Modified by', 'Modified by', 'Modified by', 'Modified by'
	
	-- 99
    INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) SELECT @nDescid + 99, 'EXTENSION', 'EXTENSION99', 8, 0	
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) SELECT @nDescid + 99, 'Appartient à', 'Belongs to', 'Belongs to', 'Belongs to', 'Belongs to', 'Belongs to'
	
	
	--Pref/selections
	delete from selections where tab=@nDescid
	delete from pref where tab=@nDescid
	
	
	insert into pref (tab,userid, listcol) select @nDescid, 0, '101101'
	insert into pref (tab,userid, listcol) select  @nDescid, userid, '101101' from [user]

	
	insert into selections (tab,label,listcol, listcolwidth,userid)  select @nDescid, 'Vue par défaut', '101101;101102', '0;0', 0
	insert into selections (tab,label,listcol, listcolwidth,userid)  select @nDescid, 'Vue par défaut', '1011001;101102', '0;0', userid from [user]

 
	update pref set selectid = selections.selectid from [pref]  
	inner join selections on selections.userid = [pref].userid 
	and  [selections].[tab] = [pref].[tab] where [pref].[tab] = @nDescid	
	 
	update [desc] set [disporder] = 1 where descid = @nDescid
	update [desc] set [disporder] = 2 where descid = @nDescid + 2
	
END
