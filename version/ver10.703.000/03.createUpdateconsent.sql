IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_NAME = 'esp_archiveConsent')
	EXEC ('CREATE PROC dbo.[esp_archiveConsent] AS SELECT ''stub version, to be replaced''')
	
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_NAME = 'esp_archiveConsentAll')
	EXEC ('CREATE PROC dbo.[esp_archiveConsentAll] AS SELECT ''stub version, to be replaced''')