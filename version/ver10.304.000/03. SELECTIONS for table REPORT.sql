-- #54337 : Re-cr�ation de la s�lection des colonnes pour la table REPORT s'il n'y a plus de s�lection pour l'utilisateur 0
IF (SELECT COUNT(SelectId) FROM [PREF] WHERE Tab = 105000 AND UserId = 0 AND SelectId IS NULL) = 0
BEGIN
	DECLARE @selectID INT = 0

	INSERT INTO [SELECTIONS] (Tab, UserId, Label, ListCol) VALUES (105000, 0, 'Vue par d�faut', '105008;105004')
	
	SELECT @selectID = SCOPE_IDENTITY()
	
	IF @selectID <> 0
		UPDATE [PREF] SET SelectId = @selectID WHERE Tab = 105000 AND UserId = 0
END