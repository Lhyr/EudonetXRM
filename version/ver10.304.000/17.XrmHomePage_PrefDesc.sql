
set nocount on;

DECLARE @nTab INT = 115200 
begin
    
	DELETE [res] where resid >= @nTab AND resid < @nTab + 100
	DELETE [FileDataParam] where DESCid >=@nTab AND DESCid < @nTab + 100
	DELETE [UserValue] where DESCid >=@nTab AND DESCid < @nTab + 100
	DELETE [DESC] where DESCid >=@nTab AND DESCid < @nTab + 100	
	
	-- Table des pages d'accueil
	INSERT INTO [DESC] ([DescId],[File], [Field], [Format], [Length], [type], [ActiveTab], [BreakLine]) 
	SELECT @nTab,'XrmHomePage', 'XrmHomePage', 0, 0, 22, 0, 25	
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
	SELECT @nTab, 'Pages d''accueil XRM', 'Xrm homepages', 'Xrm homepages', 'Xrm homepages', 'Xrm homepages', 'Xrm homepages'
	
	-- Titre de la page d'accueil
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) 
	SELECT @nTab + 1, 'XrmHomePage', 'Title', 1, 1000	
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
	SELECT @nTab + 1, 'Titre', 'Title', 'Title', 'Title', 'Title', 'Title'	  
	
	-- Infobulle de la page d'accueil
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length], [default]) 
	SELECT @nTab + 2, 'XrmHomePage', 'Tooltip', 1, 1000, ''	
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
	SELECT @nTab + 2, 'Infobulle', 'Tooltip', 'Tooltip', 'Tooltip', 'Tooltip', 'Tooltip'	  
	
	
	-- Affectation utilisateurs
	insert into [desc] ([DescId], [File], [Field], [Format], [Length], [Multiple]) 
	select @nTab + 3, 'XrmHomePage', 'UserAssign', 8, 0, 1
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
	select @nTab + 3, 'Affectée à (Utilisateurs)', 'Assigned to(Users)', 'Assigned to(Users)', 'Assigned to(Users)', 'Assigned to(Users)', 'Assigned to(Users)'

	-- Affectation groupes
	insert into [desc] ([DescId], [File], [Field], [Format], [Length], [Multiple]) 
	select @nTab + 4, 'XrmHomePage', 'GroupAssign', 8, 0, 1
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
	select @nTab + 4, 'Affectée à (Groupes)', 'Assigned to(Groups)', 'Assigned to(Groups)', 'Assigned to(Groups)', 'Assigned to(Groups)', 'Assigned to(Groups)'
	
	-- Champ system
	
	--84 et 84_res
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) 
	SELECT @nTab + 84, 'XrmHomePage', 'XrmHomePage84', 3, 0	
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05)
	SELECT @nTab + 84, 'Confidentielle', 'Confidential', 'Confidential', 'Confidential', 'Confidential', 'Confidential'
	
	
	-- 92 et 92_res
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) 
	select @nTab + 92, 'XrmHomePage', 'XrmHomePage92', 8, 0	
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
	select @nTab + 92, 'Page de', 'Home page of', 'Home page of', 'Home page of', 'Home page of', 'Home page of'
	
	-- 95 et 95_res
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) 
	SELECT @nTab + 95, 'XrmHomePage', 'XrmHomePage95', 2, 0
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
	SELECT @nTab + 95, 'Créé le', 'Created on', 'Created on', 'Created on', 'Created on', 'Created on'
	
	-- 96 et 96_res
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) 
	SELECT @nTab + 96, 'XrmHomePage', 'XrmHomePage96', 2, 0
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
	SELECT @nTab + 96, 'Modifié le', 'Modified on', 'Modified on', 'Modified on', 'Modified on', 'Modified on'	
	
	-- 97 et 97_res
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) 
	SELECT @nTab + 97, 'XrmHomePage', 'XrmHomePage97', 8, 0
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
	SELECT @nTab + 97, 'Créé par', 'Created by', 'Created by', 'Created by', 'Created by', 'Created by'
	
	-- 98 et 98_res
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) 
	SELECT @nTab + 98, 'XrmHomePage', 'XrmHomePage98', 8, 0
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
	SELECT @nTab + 98, 'Modifié par', 'Modified by', 'Modified by', 'Modified by', 'Modified by', 'Modified by'
	
    INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) 
	SELECT @nTab + 99, 'XrmHomePage', 'XrmHomePage99', 8, 0
	
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
	SELECT @nTab + 99, 'Appartient à', 'Belongs to', 'Belongs to', 'Belongs to', 'Belongs to', 'Belongs to'
	
	
	--Pref/selections
	delete from selections where tab=@nTab
	delete from pref where tab=@nTab
	
	
	insert into pref (tab,userid, listcol) select @nTab, 0, '115201;115202;115203;115204'
	insert into pref (tab,userid, listcol) select @nTab, userid, '115201;115202;115203;115204' from [user]
   
	insert into selections (tab,label,listcol, listcolwidth,userid) 
	select @nTab, 'Vue par défaut', '115201;115202;115203;115204', '0;0', 0
	insert into selections (tab,label,listcol, listcolwidth,userid) 
	select @nTab, 'Vue par défaut', '115201;115202;115203;115204', '0;0', userid from [user]
  
	update pref set selectid = selections.selectid from [pref]  
	inner join selections on selections.userid = [pref].userid 
	and  [selections].[tab] = [pref].[tab] where [pref].[tab] = @nTab	
	 
	update [desc] set [disporder] = 1 where descid = @nTab + 1
	update [desc] set [disporder] = 2 where descid = @nTab + 2

END
