--TRAIT
IF NOT EXISTS (SELECT * FROM TRAIT WHERE TraitId IN (  39 ) )
BEGIN	

-- Ajout des droits de traitement par defaut dans la table TRAIT de la base en cours
	INSERT INTO [TRAIT] ([TraitId], [UserLevel], [TraitLevel]) VALUES(39, 0, 0)	
END