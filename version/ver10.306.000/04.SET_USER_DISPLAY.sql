
/*
SPH - 21/06/2017
 -> Ré agencement de la fiche user en mode coordonnées

*/
declare @Y as numeric
declare @X as numeric


set @Y = 0
set @X = 0

UPDATE [desc] SET DispOrder = 0 where  [FILE]='USER' AND [DispOrder]>0

/* Partie haute - Photo et infos de base*/
UPDATE [DESC] SET [X] = 0, [Y] = 0, [ROWSPAN] = 9, [COLSPAN] = 1 , DispOrder = 1 WHERE [DESCID] = 101075

	UPDATE [DESC] SET [X] = 1, [Y] = 0, [ROWSPAN] = 1, [COLSPAN] = 1, [DispOrder] = (select MAX(disporder) + 1 from [desc]  where  [FILE]='USER' )  WHERE [DESCID] = 101001 -- Login
	UPDATE [DESC] SET [X] = 1, [Y] = 1, [ROWSPAN] = 1, [COLSPAN] = 1, [DispOrder] = (select MAX(disporder) + 1 from [desc]  where  [FILE]='USER' )  WHERE [DESCID] = 101002 -- Nom complet 
	UPDATE [DESC] SET [X] = 1, [Y] = 2, [ROWSPAN] = 1, [COLSPAN] = 1, [DispOrder] = (select MAX(disporder) + 1 from [desc]  where  [FILE]='USER' )  WHERE [DESCID] = 101010 -- Fonction
	UPDATE [DESC] SET [X] = 1, [Y] = 3, [ROWSPAN] = 1, [COLSPAN] = 1, [DispOrder] = (select MAX(disporder) + 1 from [desc]  where  [FILE]='USER' )  WHERE [DESCID] = 101007 -- tél
	UPDATE [DESC] SET [X] = 1, [Y] = 4, [ROWSPAN] = 1, [COLSPAN] = 1, [DispOrder] = (select MAX(disporder) + 1 from [desc]  where  [FILE]='USER' )  WHERE [DESCID] = 101009 -- portable
	UPDATE [DESC] SET [X] = 1, [Y] = 5, [ROWSPAN] = 1, [COLSPAN] = 1, [DispOrder] = (select MAX(disporder) + 1 from [desc]  where  [FILE]='USER' )  WHERE [DESCID] = 101008 -- fax
	UPDATE [DESC] SET [X] = 1, [Y] = 6, [ROWSPAN] = 1, [COLSPAN] = 1, [DispOrder] = (select MAX(disporder) + 1 from [desc]  where  [FILE]='USER' )  WHERE [DESCID] = 101004 -- email
	UPDATE [DESC] SET [X] = 1, [Y] = 7, [ROWSPAN] = 1, [COLSPAN] = 1, [DispOrder] = (select MAX(disporder) + 1 from [desc]  where  [FILE]='USER' )  WHERE [DESCID] = 101018 -- email autre
	UPDATE [DESC] SET [X] = 1, [Y] = 8, [ROWSPAN] = 1, [COLSPAN] = 1, [DispOrder] = (select MAX(disporder) + 1 from [desc]  where  [FILE]='USER' )  WHERE [DESCID] = 101023 -- Langue

/*Coordonnées*/
UPDATE [DESC] SET [X] = 0, [Y] = 9, [ROWSPAN] = 1, [COLSPAN] = 2, [DispOrder] = (select MAX(disporder) + 1 from [desc]  where  [FILE]='USER' )  WHERE [DESCID] = 101024 -- TITRE SEPARATEUR
UPDATE [DESC] SET [X] = 0, [Y] = 10, [ROWSPAN] = 3, [COLSPAN] = 2, [DispOrder] = (select MAX(disporder) + 1 from [desc]  where  [FILE]='USER' )  WHERE [DESCID] = 101006 -- Addresse

UPDATE [DESC] SET [X] = 0, [Y] = 13, [ROWSPAN] = 1, [COLSPAN] = 1, [DispOrder] = (select MAX(disporder) + 1 from [desc]  where  [FILE]='USER' )  WHERE [DESCID] = 101013 -- CP
	UPDATE [DESC] SET [X] = 1, [Y] = 13, [ROWSPAN] = 1, [COLSPAN] = 1, [DispOrder] = (select MAX(disporder) + 1 from [desc]  where  [FILE]='USER' )  WHERE [DESCID] = 101014 -- Ville


/* Extensions */
UPDATE [DESC] SET [X] = 0, [Y] = 14, [ROWSPAN] = 1, [COLSPAN] = 2, [DispOrder] = (select MAX(disporder) + 1 from [desc]  where  [FILE]='USER' )  WHERE [DESCID] = 101031  -- TITRE SEPARATEUR
UPDATE [DESC] SET [X] = 0, [Y] = 15, [ROWSPAN] = 1, [COLSPAN] = 1, [DispOrder] = (select MAX(disporder) + 1 from [desc]  where  [FILE]='USER' )  WHERE [DESCID] = 101020  -- Exchange

/*Habilitation & Apparatenance*/
UPDATE [DESC] SET [X] = 0, [Y] = 16, [ROWSPAN] = 1, [COLSPAN] = 2, [DispOrder] = (select MAX(disporder) + 1 from [desc]  where  [FILE]='USER' )  WHERE [DESCID] = 101025

UPDATE [DESC] SET [X] = 0, [Y] = 17, [ROWSPAN] = 1, [COLSPAN] = 1, [DispOrder] = (select MAX(disporder) + 1 from [desc]  where  [FILE]='USER' )  WHERE [DESCID] = 101011 -- Désactivé
	UPDATE [DESC] SET [X] = 1, [Y] = 17, [ROWSPAN] = 1, [COLSPAN] = 1, [DispOrder] = (select MAX(disporder) + 1 from [desc]  where  [FILE]='USER' )  WHERE [DESCID] = 101012 -- Masqué

UPDATE [DESC] SET [X] = 0, [Y] = 18, [ROWSPAN] = 1, [COLSPAN] = 1, [DispOrder] = (select MAX(disporder) + 1 from [desc]  where  [FILE]='USER' )  WHERE [DESCID] = 101027 -- Groupe
	UPDATE [DESC] SET [X] = 1, [Y] = 18, [ROWSPAN] = 1, [COLSPAN] = 1, [DispOrder] = (select MAX(disporder) + 1 from [desc]  where  [FILE]='USER' )  WHERE [DESCID] = 101017 -- Niveau
UPDATE [DESC] SET [X] = 0, [Y] = 19, [ROWSPAN] = 1, [COLSPAN] = 1, [DispOrder] = (select MAX(disporder) + 1 from [desc]  where  [FILE]='USER' )  WHERE [DESCID] = 101028 -- Mdp not expire
	UPDATE [DESC] SET [X] = 1, [Y] = 19, [ROWSPAN] = 1, [COLSPAN] = 1  WHERE [DESCID] = 101032 -- Profil

/*Informations complémentaires*/
UPDATE [DESC] SET [X] = 0, [Y] = 20, [ROWSPAN] = 1, [COLSPAN] = 2, [DispOrder] = (select MAX(disporder) + 1 from [desc]  where  [FILE]='USER' )  WHERE [DESCID] = 101026 --
UPDATE [DESC] SET [X] = 0, [Y] = 21, [ROWSPAN] = 5, [COLSPAN] = 2, [DispOrder] = (select MAX(disporder) + 1 from [desc]  where  [FILE]='USER' )  WHERE [DESCID] = 101003 --Description