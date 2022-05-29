-- Re-création de la sélection des colonnes pour la table FILTER s'il n'y a plus de sélection pour l'utilisateur 0
DECLARE @tabFilter INT = 104000

IF (SELECT COUNT(SelectId) FROM [PREF] WHERE Tab = @tabFilter AND UserId = 0 AND SelectId IS NULL) = 0
BEGIN
	DECLARE @selectID INT = 0

	-- Affichage de "Modifié le" et "Appartient à"
	INSERT INTO [SELECTIONS] (Tab, UserId, Label, ListCol) VALUES (@tabFilter, 0, 'Vue par défaut', '104006;104007')
	
	SELECT @selectID = SCOPE_IDENTITY()
	
	IF @selectID <> 0
		UPDATE [PREF] SET SelectId = @selectID WHERE Tab = @tabFilter AND UserId = 0
END