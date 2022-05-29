DECLARE @nTab INT = 119200 
BEGIN

    
	    
 
	
	
	-- Onglet Parent
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length],[popup]  ) SELECT @nTab + 4, 'WorkflowScenario', 'WFTEVENTDESCID', 1,0,5
	WHERE NOT EXISTS (SELECT 1 FROM [DESC] WHERE DESCID = @nTab + 4)
	
	INSERT INTO [res] (resid, lang_00) SELECT @nTab + 4 , 'Table parente'
	WHERE NOT EXISTS (SELECT 1 FROM [res] WHERE resid = @nTab + 4)
	
	-- Table Cible
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length],[popup]   ) SELECT @nTab + 5, 'WorkflowScenario', 'WFTTARGETDESCID', 1,0,5
	WHERE NOT EXISTS (SELECT 1 FROM [DESC] WHERE DESCID = @nTab + 5)
	
	INSERT INTO [res] (resid, lang_00) SELECT @nTab + 5 , 'Table cible'
	WHERE NOT EXISTS (SELECT 1 FROM [res] WHERE resid = @nTab + 5)
	
	
	update [DESC] set [DispOrder] = 2, [Rowspan] = 1, [Colspan] = 3 where [DescId] = @nTab + 4
	update [DESC] set [DispOrder] = 3, [Rowspan] = 1, [Colspan] = 3 where [DescId] = @nTab + 5
	
END