DECLARE @campaignField INT = 106008
DECLARE @interactionField INT = 118009

IF NOT EXISTS (
		SELECT *
		FROM [DESC]
		WHERE [DescId] = @interactionField
			AND [PopupDescId] = @campaignField
		)
BEGIN
	UPDATE [FILEDATA]
	SET [DescId] = @campaignField
	WHERE [DescId] = @interactionField
		AND [DataId] IN (
			SELECT [TPL09]
			FROM [INTERACTION]
			WHERE isnumeric([TPL09]) = 1
			)

	DELETE
	FROM [FILEDATA]
	WHERE [DescId] = @interactionField

	UPDATE [DESC]
	SET [PopupDescId] = @campaignField
	WHERE [DescId] = @interactionField
END
