set nocount on;
 
DECLARE @nTab INT = 119200 
begin
    	
	-- LA TABLE Workflow Trigger est pour les admins
	-- On recupere la permId si on l'a deja creee	
    DECLARE @VisuPermId INT = 0   
    SELECT @VisuPermId = IsNull([ViewPermId], 0) FROM [DESC] WHERE DescId = @nTab 
	if(@VisuPermId = 0)
	BEGIN
     -- 0:Niveau seulement, 1:Utilisateur seulement, 2:Utilisateur ou Niveau, 3:Utilisateur et Niveau 
     -- que pour les admins  
     -- Id des users séparés par ";"  ex: 19;89;265;185;184	
	 INSERT INTO [Permission] ([Mode], [Level], [User]) values (2, 99, NULL)
	 SELECT @VisuPermId = SCOPE_IDENTITY()
	END

	-- Raz
	DELETE [res] where (resid >= @nTab  AND ResId <  @nTab + 40 ) OR (ResId > @nTab + 70 AND resid < @nTab + 100 )
	DELETE [FileDataParam] where (descid >= @nTab  AND descid <  @nTab + 40 ) OR (descid > @nTab + 70 AND descid < @nTab + 100 )
	DELETE [UserValue] where (descid >= @nTab  AND descid <  @nTab + 40 ) OR (descid > @nTab + 70 AND descid < @nTab + 100 )
	DELETE [DESC] where (descid >= @nTab  AND descid <  @nTab + 40 ) OR (descid > @nTab + 70 AND descid < @nTab + 100 )
	
	
	-- Workflow Trigger
	INSERT INTO [DESC] ([DescId],[ViewPermId], [File], [Field], [Format], [Length], [type], [ActiveTab], [BreakLine]) SELECT @nTab, @VisuPermId, 'WorkflowScenario', 'WFSC', 0, 0, 0, 1, 25
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) SELECT @nTab, 'Scénario', '', '', '', ''

	-- Libellé
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) SELECT @nTab + 1, 'WorkflowScenario', 'WFSCLabel', 1, 1000
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) SELECT @nTab + 1, 'Libellé', 'Label', 'Label', 'Label', 'Label'
	   

    -- Description
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length],[html] ) SELECT @nTab + 2, 'WorkflowScenario', 'WFSCDescription', 9, 0 ,1
	INSERT INTO [res] (resid, lang_00) SELECT @nTab + 2 , 'Description'
 
    -- Activé
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]  ) SELECT @nTab + 3, 'WorkflowScenario', 'WFSCEnabled', 3,0
	INSERT INTO [res] (resid, lang_00) SELECT @nTab + 3 , 'Activé'
	
	-- Champ system
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) SELECT @nTab + 84, 'WorkflowScenario', 'WFSC84', 3, 0
	INSERT INTO [RES] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) SELECT @nTab + 84, 'Confidentielle', 'Confidential', 'Confidential', 'Confidential', 'Confidential', 'Confidential'
	
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) select @nTab + 88, 'WorkflowScenario', 'WFSC88', 8, 0
	INSERT INTO [RES] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) select @nTab + 88, 'Notification de', 'Notification of', 'Notification of', 'Notification of', 'Notification of', 'Notification of'
	
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) SELECT @nTab + 91, 'WorkflowScenario', 'WFSC91', 2, 0
	INSERT INTO [res] (resid, lang_00, lang_01) SELECT @nTab + 91, 'Annexes', 'Annexes'	

	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) SELECT @nTab + 95, 'WorkflowScenario', 'WFSC95', 2, 0
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) SELECT @nTab + 95, 'Créé le', 'Created on', 'Created on', 'Created on', 'Created on'	
	
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) SELECT @nTab + 96, 'WorkflowScenario', 'WFSC96', 2, 0
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) SELECT @nTab + 96, 'Modifié le', 'Modified on', 'Modified on', 'Modified on', 'Modified on'	
	
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) SELECT @nTab + 97, 'WorkflowScenario', 'WFSC97', 8, 0
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) SELECT @nTab + 97, 'Créé par', 'Created by', 'Created by', 'Created by', 'Created by'
	
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) SELECT @nTab + 98, 'WorkflowScenario', 'WFSC98', 8, 0
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) SELECT @nTab + 98, 'Modifié par', 'Modified by', 'Modified by', 'Modified by', 'Modified by'
	
    INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) SELECT @nTab + 99, 'WorkflowScenario', 'WFSC99', 8, 0
	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05) SELECT @nTab + 99, 'Appartient à', 'Belongs to', 'Belongs to', 'Belongs to', 'Belongs to', 'Belongs to'
	
	
	--Pref/selections
	
	delete from selections where tab=@nTab
	delete from pref where tab=@nTab
	insert into pref (tab,userid, listcol) select @nTab, 0, '119201'
	insert into pref (tab,userid, listcol) select  @nTab, userid, '119201' from [user]

	insert into selections (tab,label,listcol, listcolwidth,userid) select @nTab, 'Vue par défaut', '119201', '0;0', 0
	insert into selections (tab,label,listcol, listcolwidth,userid) select @nTab, 'Vue par défaut', '119201', '0;0', userid from [user]

	update pref set selectid = selections.selectid from [pref]  
	inner join selections on selections.userid = [pref].userid 
	and  [selections].[tab] = [pref].[tab] where [pref].[tab] = @nTab
	
	
	--Mise en page Table WorkflowScenario
	update [DESC] set [columns] = '200,500,25' where [DescId] = @nTab  
	update [DESC] set [DispOrder] = 1, [Rowspan] = 1, [Colspan] = 3 where [DescId] = @nTab + 1 	
	update [DESC] set [DispOrder] = 2, [Rowspan] = 1, [Colspan] = 3 where [DescId] = @nTab + 2
	update [DESC] set [DispOrder] = 3, [Rowspan] = 1, [Colspan] = 3 where [DescId] = @nTab + 3

	
 
    

END