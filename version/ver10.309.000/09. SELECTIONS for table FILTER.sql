-- Re-cr�ation de la s�lection des colonnes pour la table FILTER s'il n'y a plus de s�lection pour l'utilisateur 0
DECLARE @tabFilter INT = 104000

IF (SELECT COUNT(SelectId) FROM [PREF] WHERE Tab = @tabFilter AND UserId = 0 AND SelectId IS NULL) = 0
BEGIN
	DECLARE @selectID INT = 0

	-- Affichage de "Modifi� le" et "Appartient �"
	INSERT INTO [SELECTIONS] (Tab, UserId, Label, ListCol) VALUES (@tabFilter, 0, 'Vue par d�faut', '104006;104007')
	
	SELECT @selectID = SCOPE_IDENTITY()
	
	IF @selectID <> 0
		UPDATE [PREF] SET SelectId = @selectID WHERE Tab = @tabFilter AND UserId = 0
END