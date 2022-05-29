DECLARE @campaignMediaType int = 106046

DECLARE @nEmailingCatValue numeric(18)
DECLARE @nSMSCatValue numeric(18)

--Récupération des valeurs des catalogues parent
SELECT TOP 1 @nEmailingCatValue = [DataId]
FROM [FILEDATA]
WHERE [DescId] = @campaignMediaType
AND [Data] = 'email'

SELECT TOP 1 @nSMSCatValue = [DataId]
FROM [FILEDATA]
WHERE [DescId] = @campaignMediaType
AND [Data] = 'sms'

--Mise à jour type de media sur CAMPAIGN
IF @nEmailingCatValue IS NOT NULL
BEGIN
	UPDATE [CAMPAIGN] SET
	[MediaType] = @nEmailingCatValue
	WHERE [SendType] IN (0, 1)
END

IF @nSMSCatValue IS NOT NULL
BEGIN
	UPDATE [CAMPAIGN] SET
	[MediaType] = @nSMSCatValue
	WHERE [SendType] IN (2)
END

--Mise à jour type de media sur INTERACTION
UPDATE [INTERACTION] SET [TPL17] = [FILEDATA].[ParentDataId]
FROM [INTERACTION]
INNER JOIN [FILEDATA] ON [FILEDATA].[DataId] = [INTERACTION].[TPL09]
WHERE [FILEDATA].[ParentDataId] IS NOT NULL 
