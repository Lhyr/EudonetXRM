IF(SELECT COUNT(1) FROM [DESCADV] WHERE [PARAMETER] = 8 AND [DESCID] = 101100 ) = 1
	UPDATE [DESCADV] SET [VALUE] = 1 WHERE [PARAMETER] = 8 AND [DESCID] = 101100  
ELSE
	INSERT INTO [DESCADV] (Parameter,Value,Category,descid)
	SELECT 8,1,2,101100


 