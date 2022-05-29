-- MAB - 2017-04-25 - #54 942 - Activer par d�faut l'extension E-mailing

 IF (SELECT COUNT(1) FROM [EXTENSION] WHERE [EXTENSIONCODE] = 'EMAILING') = 1
	UPDATE [EXTENSION] SET [EXTENSIONSTATUS] = 'STATUS_READY' WHERE [EXTENSIONCODE] = 'EMAILING'
ELSE
	INSERT INTO [EXTENSION] (
		[EXTENSIONTYPE],
		[EXTENSIONCODE],
		[EXTENSIONSTATUS],
		[EXTENSIONPARAM],
		[EXTENSION96],
		[EXTENSION95]
	)
	SELECT
		'ADMIN_EXTENSIONS_FROMSTORE',
		'EMAILING',
		'STATUS_READY',
		'{}',
		GETDATE(),
		GETDATE()