-- trigger
IF (SELECT COUNT(1) FROM [DESCADV] where Parameter = 56 and DescId = 119100 ) = 0
BEGIN
	INSERT INTO [DESCADV] (DescId, Parameter, Value, Category) VALUES (119100, 56, 1, 0)
END
	
-- step
IF (SELECT COUNT(1) FROM [DESCADV] where Parameter = 56 and DescId = 119300 ) = 0
BEGIN
	INSERT INTO [DESCADV] (DescId, Parameter, Value, Category) VALUES (119300, 56, 1, 0)
END
	
-- WorkflowActivities
IF (SELECT COUNT(1) FROM [DESCADV] where Parameter = 56 and DescId = 119400 ) = 0
BEGIN
	INSERT INTO [DESCADV] (DescId, Parameter, Value, Category) VALUES (119400, 56, 1, 0)
END
	
	
-- WorkflowScenario
IF (SELECT COUNT(1) FROM [DESCADV] where Parameter = 56 and DescId = 119200 ) = 0
BEGIN
	INSERT INTO [DESCADV] (DescId, Parameter, Value, Category) VALUES (119200, 56, 1, 0)
END

-- WorkflowAction
IF (SELECT COUNT(1) FROM [DESCADV] where Parameter = 56 and DescId = 119600 ) = 0
BEGIN
	INSERT INTO [DESCADV] (DescId, Parameter, Value, Category) VALUES (119600, 56, 1, 0)
END