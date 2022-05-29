
/**
 *  Ajout des droits de traitement sur les formulaires 
 *  Demande cf.32814
 */

DECLARE @FORM_ID INT = 55
DECLARE @FORM_ADD_ID INT = 56
DECLARE @FORM_MODIFY_ID INT = 57
DECLARE @FORM_DELETE_ID INT = 58

-- EUDORES..TRAIT
IF NOT EXISTS (SELECT * FROM EUDORES..TRAIT WHERE TraitId IN ( @FORM_ID, @FORM_ADD_ID, @FORM_MODIFY_ID, @FORM_DELETE_ID ) )
BEGIN	
	-- Ajout des ressources dans la table EUDORES..TRAIT
	INSERT INTO EUDORES..TRAIT (TraitId, LANG_00, LANG_01, LANG_02, LANG_03, LANG_04, LANG_05)
	VALUES (@FORM_ID, 'Formulaire', 'Formular', 'Formular', 'Formular', 'Formular', 'Formular')

	INSERT INTO EUDORES..TRAIT (TraitId, LANG_00, LANG_01, LANG_02, LANG_03, LANG_04, LANG_05)
	VALUES (@FORM_ADD_ID, 'Ajouter un formulaire', 'Add new formular', 'Add new formular', 'Add new formular', 'Add new formular', 'Add new formular')

	INSERT INTO EUDORES..TRAIT (TraitId, LANG_00, LANG_01, LANG_02, LANG_03, LANG_04, LANG_05)
	VALUES (@FORM_MODIFY_ID, 'Modifier un formulaire', 'Modify a formular', 'Modify a formular', 'Modify a formular', 'Modify a formular', 'Modify a formular')

	INSERT INTO EUDORES..TRAIT (TraitId, LANG_00, LANG_01, LANG_02, LANG_03, LANG_04, LANG_05)
	VALUES (@FORM_DELETE_ID, 'Supprimer un formulaire', 'Delete a formular', 'Delete a formular', 'Delete a formular', 'Delete a formular', 'Delete a formular')
END

--TRAIT
IF NOT EXISTS (SELECT * FROM TRAIT WHERE TraitId IN ( @FORM_ID, @FORM_ADD_ID, @FORM_MODIFY_ID, @FORM_DELETE_ID ) )
BEGIN	

-- Ajout des droits de traitement par defaut dans la table TRAIT de la base en cours
	INSERT INTO [TRAIT] ([TraitId], [UserLevel], [TraitLevel]) VALUES(@FORM_ID, 0, 0)
	INSERT INTO [TRAIT] ([TraitId], [UserLevel], [TraitLevel]) VALUES(@FORM_ADD_ID, 0, 0)
	INSERT INTO [TRAIT] ([TraitId], [UserLevel], [TraitLevel]) VALUES(@FORM_MODIFY_ID, 0, 0)
	INSERT INTO [TRAIT] ([TraitId], [UserLevel], [TraitLevel]) VALUES(@FORM_DELETE_ID, 0, 0)
END