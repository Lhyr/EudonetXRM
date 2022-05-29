
set nocount on;

DECLARE @nTab INT = 115100 
begin
    
	DELETE [res] where resid >= @nTab AND resid < @nTab + 100
	DELETE [FileDataParam] where DESCid >=@nTab AND DESCid < @nTab + 100
	DELETE [UserValue] where DESCid >=@nTab AND DESCid < @nTab + 100
	DELETE [DESC] where DESCid >=@nTab AND DESCid < @nTab + 100
			
	-- Table XrmHomePage
	INSERT INTO [DESC] ([DescId],[File], [Field], [Format], [Length], [type], [ActiveTab], [BreakLine]) 
	SELECT @nTab, 'XrmWidget', 'XrmWidget', 0, 0, 23, 0, 25	
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
	SELECT @nTab, 'Widget', 'Widget', 'Widget', 'Widget', 'Widget', 'Widget'
	
	-- Titre du widget
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) 
	SELECT @nTab + 1, 'XrmWidget', 'Title', 1, 1000	
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
	SELECT @nTab + 1, 'Titre', 'Title', 'Title', 'Title', 'Title', 'Title'	

    -- Sous-Titre du widget
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) 
	SELECT @nTab + 2, 'XrmWidget', 'SubTitle', 1, 1000	
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
	SELECT @nTab + 2, 'Sous-titre', 'Subtitle', 'Subtitle', 'Subtitle', 'Subtitle', 'Subtitle'	
	
	 -- Infobulle du widget
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) 
	SELECT @nTab + 3, 'XrmWidget', 'Tooltip', 1, 1000	
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
	SELECT @nTab + 3, 'Infobulle', 'Tooltip', 'Tooltip', 'Tooltip', 'Tooltip', 'Tooltip'	
	
	-- Type de widget
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) 
	select @nTab + 4, 'XrmWidget', 'Type', 10, 0	
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
	select @nTab + 4, 'Type de widget', 'Widget type', 'Widget type', 'Widget type', 'Widget type', 'Widget type'
	
	
	 -- Pictogramme du widget
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) 
	SELECT @nTab + 5, 'XrmWidget', 'PictoIcon', 1, 50	
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
	SELECT @nTab + 5, 'Pictogramme', 'Pictogramme', 'Pictogramme', 'Pictogramme', 'Pictogramme', 'Pictogramme'
	
	-- Déplaçable
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) 
	SELECT @nTab + 6, 'XrmWidget', 'Move', 3, 0
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05)
	SELECT @nTab + 6, 'Déplaçable', 'Can be moved', 'Can be moved', 'Can be moved', 'Can be moved', 'Can be moved'

	-- Redémensionnable
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) 
	SELECT @nTab + 7, 'XrmWidget', 'Resize', 3, 0
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05)
	SELECT @nTab + 7, 'Redémensionnable', 'Can be resized', 'Can be resized', 'Can be resized', 'Can be resized', 'Can be resized'
	
	 -- Options d'affichage : Toujours affiché :0, Affiché par défaut : 1, Masqué par défaut : 2    
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) 
	select @nTab + 8, 'XrmWidget', 'DisplayOption', 10, 1	
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
	select @nTab + 8, 'Option d''affichage', 'Display option', 'Display option', 'Display option', 'Display option', 'Display option'
	
	
	-- DefaultPosX
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) 
	select @nTab + 9, 'XrmWidget', 'DefaultPosX', 10, 0	
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
	select @nTab + 9, 'Position x', 'X position', 'X position', 'X position', 'X position', 'X position'
	
	-- DefaultPosY
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) 
	select @nTab + 10, 'XrmWidget', 'DefaultPosY', 10, 0	
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
	select @nTab + 10, 'Position y', 'Y position', 'Y position', 'Y position', 'Y position', 'Y position'
	
	-- DefaultWidth
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) 
	select @nTab + 11, 'XrmWidget', 'DefaultWidth', 10, 0	
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
	select @nTab + 11, 'Largeur par défaut', 'Default width', 'Default width', 'Default width', 'Default width', 'Default width'
	
	-- DefaultHeight
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) 
	select @nTab + 12, 'XrmWidget', 'DefaultHeight', 10, 0	
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
	select @nTab + 12, 'Hauteur par défaut', 'Default height', 'Default height', 'Default height', 'Default height', 'Default height'
	
	
	-- ContentSource
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) 
	select @nTab + 13, 'XrmWidget', 'ContentSource', 1, 0
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
	select @nTab + 13, 'Source du contenu', 'Content source', 'Content source', 'Content source', 'Content source', 'Content source'
	
	
	-- ContentParam
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) 
	select @nTab + 14, 'XrmWidget', 'ContentParam', 1, 0
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
	select @nTab + 14, 'Paramètres du contenu', 'Content Param', 'Content Param', 'Content Param', 'Content Param', 'Content Param'
	
	-- ContentType
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) 
	select @nTab + 15, 'XrmWidget', 'ContentType', 10, 0
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
	select @nTab + 15, 'Type du contenu', 'Content type', 'Content type', 'Content type', 'Content type', 'Content type'
	
	
    -- Permission visualtion et modification
    insert into [desc] ([DescId], [File], [Field], [Format], [Length]) 
	select @nTab + 16, 'XrmWidget', 'ViewPermId', 10, 0	
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
	select @nTab + 16, 'Permission de visualisation', 'View permission', 'View permission', 'View permission', 'View permission', 'View permission'
	
	 -- Pictogramme du widget
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) 
	SELECT @nTab + 17, 'XrmWidget', 'PictoColor', 1, 50	
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
	SELECT @nTab + 17, 'Coleur Pictogramme', 'Pictogramme color', 'Pictogramme color', 'Pictogramme color', 'Pictogramme color', 'Pictogramme color'
	
	-- Actualisation manuel
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) 
	SELECT @nTab + 18, 'XrmWidget', 'ManualRefresh', 3, 0
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05)
	SELECT @nTab + 18, 'Actualisation', 'Refreshing', 'Refreshing', 'Refreshing', 'Refreshing', 'Refreshing'

	-- Afficher les titres des widgets
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) 
	SELECT @nTab + 19, 'XrmWidget', 'ShowHeader', 3, 1	
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
	SELECT @nTab + 19, 'Afficher l''entête du widgets', 'Show Widget header', 'Show Widget header', 'Show Widget header', 'Show Widget header', 'Show Widget header'

	
	-- Champ system
	--84
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) 
	SELECT @nTab + 84, 'XrmWidget', 'XrmWidget84', 3, 0
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05)
	SELECT @nTab + 84, 'Confidentielle', 'Confidential', 'Confidential', 'Confidential', 'Confidential', 'Confidential'
	
	-- 88
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) 
	select @nTab + 88, 'XrmWidget', 'XrmWidget88', 8, 0
	
	-- 88_res
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
	select @nTab + 88, 'Widget de', 'Widget of', 'Widget of', 'Widget of', 'Widget of', 'Widget of'
	
	-- 92
	insert into [desc] ([DescId], [File], [Field], [Format], [Length]) 
	select @nTab + 92, 'XrmWidget', 'XrmWidget92', 8, 0
	
	-- 92_res
	insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
	select @nTab + 92, 'Widget  de', 'Widget of', 'Widget of', 'Widget of', 'Widget of', 'Widget of'
	
	-- 95
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) 
	SELECT @nTab + 95, 'XrmWidget', 'XrmWidget95', 2, 0
	
	-- 95_res
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
	SELECT @nTab + 95, 'Créé le', 'Created on', 'Created on', 'Created on', 'Created on', 'Created on'
	
	-- 96
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) 
	SELECT @nTab + 96, 'XrmWidget', 'XrmWidget96', 2, 0
	
	-- 96_res
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
	SELECT @nTab + 96, 'Modifié le', 'Modified on', 'Modified on', 'Modified on', 'Modified on', 'Modified on'	
	
	-- 97
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) 
	SELECT @nTab + 97, 'XrmWidget', 'XrmWidget97', 8, 0
	
	-- 97_res
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
	SELECT @nTab + 97, 'Créé par', 'Created by', 'Created by', 'Created by', 'Created by', 'Created by'
	
	-- 98
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) 
	SELECT @nTab + 98, 'XrmWidget', 'XrmWidget98', 8, 0
	
	-- 98_res
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
	SELECT @nTab + 98, 'Modifié par', 'Modified by', 'Modified by', 'Modified by', 'Modified by', 'Modified by'
	
    INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) 
	SELECT @nTab + 99, 'XrmWidget', 'XrmWidget99', 8, 0
	
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) 
	SELECT @nTab + 99, 'Appartient à', 'Belongs to', 'Belongs to', 'Belongs to', 'Belongs to', 'Belongs to'
	
	
	--Pref/selections
	delete from selections where tab=@nTab
	delete from pref where tab=@nTab
	
	
	insert into pref (tab,userid, listcol) select @nTab, 0, '115101;115102;115107;115108;115195;115199'
	insert into pref (tab,userid, listcol) select  @nTab, userid, '115101;115102;115107;115108;115195;115199' from [user]

	
	insert into selections (tab,label,listcol, listcolwidth,userid) 
	select @nTab, 'Vue par défaut', '115101;115102;115107;115108;115195;115199', '0;0', 0
	insert into selections (tab,label,listcol, listcolwidth,userid) 
	select @nTab, 'Vue par défaut', '115101;115102;115107;115108;115195;115199', '0;0', userid from [user]
 
	update pref set selectid = selections.selectid from [pref]  
	inner join selections on selections.userid = [pref].userid 
	and  [selections].[tab] = [pref].[tab] where [pref].[tab] = @nTab	
	 
	update [desc] set [disporder] = 1 where descid = @nTab + 1
	update [desc] set [disporder] = 2 where descid = @nTab + 2
	
END
