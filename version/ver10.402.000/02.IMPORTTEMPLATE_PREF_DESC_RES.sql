
set nocount on;

DECLARE @nTab INT = 119000 
begin
    
	DELETE [res] where resid like '1190__'
	DELETE [FileDataParam] where DESCid like '1190__'
	DELETE [UserValue] where DESCid like '1190__'
	DELETE [DESC] where DESCid like '1190__'
			
	-- Table IMPORTTEMPLATE
	INSERT INTO [DESC] ([DescId],[File], [Field], [Format], [Length], [type], [ActiveTab], [BreakLine]) 
	SELECT @nTab, 'IMPORTTEMPLATE', 'IMPORTTEMPLATE', 0, 0, 26, 0, 25	
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
	SELECT @nTab, 'IMPORTTEMPLATE', 'IMPORTTEMPLATE', 'IMPORTTEMPLATE', 'IMPORTTEMPLATE', 'IMPORTTEMPLATE', 'IMPORTTEMPLATE'
	
	-- Libellé
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) 
	SELECT @nTab + 1, 'IMPORTTEMPLATE', 'Libelle', 1, 255	
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
	SELECT @nTab + 1, 'Libelle', 'Libelle', 'Libelle', 'Libelle', 'Libelle', 'Libelle'	

    -- Paramètres en format JSON
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) 
	SELECT @nTab + 2, 'IMPORTTEMPLATE', 'Param', 9, 0	
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
	SELECT @nTab + 2, 'Param', 'Param', 'Param', 'Param', 'Param', 'Param'	
		
	-- Table
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) 
	select @nTab + 3, 'IMPORTTEMPLATE', 'Tab', 10, 0	
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
	select @nTab + 3, 'Table', 'Tab', 'Tab', 'Tab', 'Tab', 'Tab'
	
	-- Date de la dèrniere modification
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) 
	select @nTab + 4, 'IMPORTTEMPLATE', 'DateLastModified', 2, 0	
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
	select @nTab + 4, 'Modifié le', 'Last Time Modified', 'Last Time Modified', 'Last Time Modified', 'Last Time Modified', 'Last Time Modified'
	
	
	 -- Utilisateur propriétaire du modèle
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) 
	SELECT @nTab + 5, 'IMPORTTEMPLATE', 'UserId', 8, 0	
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
	SELECT @nTab + 5, 'Appartient à', 'Owner', 'Owner', 'Owner', 'Owner', 'Owner'
	
	-- Droits de visu
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) 
	SELECT @nTab + 6, 'IMPORTTEMPLATE', 'ViewPermId', 10, 0
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05)
	SELECT @nTab + 6, 'ViewPermId', 'ViewPermId', 'ViewPermId', 'ViewPermId', 'ViewPermId', 'ViewPermId'

	-- Droits de modification
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) 
	SELECT @nTab + 7, 'IMPORTTEMPLATE', 'UpdatePermId', 10, 0
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05)
	SELECT @nTab + 7, 'UpdatePermId', 'UpdatePermId', 'UpdatePermId', 'UpdatePermId', 'UpdatePermId', 'UpdatePermId'
	
	     
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) 
	select @nTab + 8, 'IMPORTTEMPLATE', 'ViewRulesId', 1, 500	
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
	select @nTab + 8, 'ViewRulesId', 'ViewRulesId', 'ViewRulesId', 'ViewRulesId', 'ViewRulesId', 'ViewRulesId'
	
	
	
	
	-- Id du modéle
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) 
	select @nTab + 9, 'IMPORTTEMPLATE', 'ImportTemplateId', 10, 0	
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
	select @nTab + 9, 'ImportTemplateId', 'Id', 'Id', 'Id', 'Id', 'Id'
	
	
	--Pref/selections
	delete from selections where tab=@nTab
	delete from pref where tab=@nTab
	
	
	insert into pref (tab,userid, listcol) select @nTab, 0, '119005;119004'
	insert into pref (tab,userid, listcol) select  @nTab, userid, '119005;119004' from [user]

	
	insert into selections (tab,label,listcol, listcolwidth,userid) 
	select @nTab, 'Vue par défaut', '119005;119004', '0;0', 0
	insert into selections (tab,label,listcol, listcolwidth,userid) 
	select @nTab, 'Vue par défaut', '119005;119004', '0;0', userid from [user]
 
	update pref set selectid = selections.selectid from [pref]  
	inner join selections on selections.userid = [pref].userid 
	and  [selections].[tab] = [pref].[tab] where [pref].[tab] = @nTab	
	 
	update [desc] set [disporder] = 1 where descid = @nTab + 1
	update [desc] set [disporder] = 2 where descid = @nTab + 2
	
END
 set nocount off;
