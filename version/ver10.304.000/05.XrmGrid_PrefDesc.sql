
set nocount on;

DECLARE @nTab INT = 115000 
begin
    
	DELETE [res] where resid >= @nTab AND resid < @nTab + 100
	DELETE [FileDataParam] where DESCid >=@nTab AND DESCid < @nTab + 100
	DELETE [UserValue] where DESCid >=@nTab AND DESCid < @nTab + 100
	DELETE [DESC] where DESCid >=@nTab AND DESCid < @nTab + 100	
	
	-- Table XrmGrid
	INSERT INTO [DESC] ([DescId],[File], [Field], [Format], [Length], [type], [ActiveTab], [BreakLine]) 
	SELECT @nTab,'XrmGrid', 'XrmGrid', 0, 0, 22, 0, 25	
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
	SELECT @nTab, 'Grille', 'Grid', 'Grid', 'Grid', 'Grid', 'Grid'
	
	-- Titre de la grille
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) 
	SELECT @nTab + 1, 'XrmGrid', 'Title', 1, 1000	
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
	SELECT @nTab + 1, 'Titre', 'Title', 'Title', 'Title', 'Title', 'Title'	  
	
	-- Infobulle de la grille
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) 
	SELECT @nTab + 2, 'XrmGrid', 'Tooltip', 1, 1000	
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
	SELECT @nTab + 2, 'Infobulle', 'Tooltip', 'Title', 'Tooltip', 'Tooltip', 'Tooltip'	 
		
	-- Ordre d'affichage
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) 
	SELECT @nTab + 3, 'XrmGrid', 'DisplayOrder', 10, 0	
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
	SELECT @nTab + 3, 'Ordre d''affichage', 'Display Order', 'Display Order', 'Display Order', 'Display Order', 'Display Order'
	
	-- Afficher les titres des widgets
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) 
	SELECT @nTab + 4, 'XrmGrid', 'ShowWidgetTitle', 3, 1	
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
	SELECT @nTab + 4, 'Afficher les titres des widgets', 'Show Widget Title', 'Show Widget Title', 'Show Widget Title', 'Show Widget Title', 'Show Widget Title'

	--Table parente
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) 
	select @nTab + 5, 'XrmGrid', 'ParentTab', 10, 0
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
	select @nTab + 5, 'Table parente', 'Parent table', 'Parent table', 'Parent table', 'Parent table', 'Parent table'
	
	--Fiche parente
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) 
	select @nTab + 6, 'XrmGrid', 'ParentFileId', 10, 0
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
	select @nTab + 6, 'Fiche parente', 'Parent File', 'Parent File', 'Parent File', 'Parent File', 'Parent File'
	
	-- Permission de visualisation / mise a jour
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) 
	SELECT @nTab + 7, 'XrmGrid', 'ViewPermId', 10, 1000	
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
	SELECT @nTab + 7, 'Permission de visualisation', 'View Permission', 'View Permission', 'View Permission', 'View Permission', 'View Permission'
	
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) 
	SELECT @nTab + 8, 'XrmGrid', 'UpdatePermId', 10, 1000	
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
	SELECT @nTab + 8, 'Permission de mise a jour', 'Update Permission', 'Update Permission', 'Update Permission', 'Update Permission', 'Update Permission'
		
	
	-- Champ system
	--84
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) 
	SELECT @nTab + 84, 'XrmGrid', 'XrmGrid84', 3, 0
	
	--84_res
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05)
	SELECT @nTab + 84, 'Confidentielle', 'Confidential', 'Confidential', 'Confidential', 'Confidential', 'Confidential'
	
	-- 88
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) 
	select @nTab + 88, 'XrmGrid', 'XrmGrid88', 8, 0
	
	-- 88_res
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
	select @nTab + 88, 'Grille de', 'Grid of', 'Grid of', 'Grid of', 'Grid of', 'Grid of'
	
	-- 92
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) 
	select @nTab + 92, 'XrmGrid', 'XrmGrid92', 8, 0
	
	-- 92_res
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
	select @nTab + 92, 'Grid de', 'Grid of', 'Grid of', 'Grid of', 'Grid of', 'Grid of'

	
	-- 95
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) 
	SELECT @nTab + 95, 'XrmGrid', 'XrmGrid95', 2, 0
	
	-- 95_res
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
	SELECT @nTab + 95, 'Créé le', 'Created on', 'Created on', 'Created on', 'Created on', 'Created on'
	
	-- 96
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) 
	SELECT @nTab + 96, 'XrmGrid', 'XrmGrid96', 2, 0
	
	-- 96_res
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
	SELECT @nTab + 96, 'Modifié le', 'Modified on', 'Modified on', 'Modified on', 'Modified on', 'Modified on'	
	
	-- 97
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) 
	SELECT @nTab + 97, 'XrmGrid', 'XrmGrid97', 8, 0
	
	-- 97_res
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
	SELECT @nTab + 97, 'Créé par', 'Created by', 'Created by', 'Created by', 'Created by', 'Created by'
	
	-- 98
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) 
	SELECT @nTab + 98, 'XrmGrid', 'XrmGrid98', 8, 0
	
	-- 98_res
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
	SELECT @nTab + 98, 'Modifié par', 'Modified by', 'Modified by', 'Modified by', 'Modified by', 'Modified by'
	
    INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) 
	SELECT @nTab + 99, 'XrmGrid', 'XrmGrid99', 8, 0
	
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
	SELECT @nTab + 99, 'Appartient à', 'Belongs to', 'Belongs to', 'Belongs to', 'Belongs to', 'Belongs to'
	
	
	--Pref/selections
	delete from selections where tab=@nTab
	delete from pref where tab=@nTab
	
	
	insert into pref (tab,userid, listcol) select @nTab, 0, '115001;115095;115096;115099'
	insert into pref (tab,userid, listcol) select  @nTab, userid, '115001;115095;115096;115099' from [user]
   
	insert into selections (tab,label,listcol, listcolwidth,userid) 
	select @nTab, 'Vue par défaut', '115001;115095;115096;115099', '0;0', 0
	insert into selections (tab,label,listcol, listcolwidth,userid) 
	select @nTab, 'Vue par défaut', '115001;115095;115096;115099', '0;0', userid from [user]
  
	update pref set selectid = selections.selectid from [pref]  
	inner join selections on selections.userid = [pref].userid 
	and  [selections].[tab] = [pref].[tab] where [pref].[tab] = @nTab	
	 
	update [desc] set [disporder] = 1 where descid = @nTab + 1
	update [desc] set [disporder] = 2 where descid = @nTab + 2

END
