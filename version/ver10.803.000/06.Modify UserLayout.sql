
/*
SPH - 15/05/2020
 -> Ré agencement de la fiche user en mode coordonnées
ALISTER - 06/02/2022
 -> Ré agencement de la fiche user en ajoutant "user profile"
*/
declare @Y as numeric
declare @X as numeric

declare @Line as numeric

set @Line = 0
set @Y = 0
set @X = 0


UPDATE [DESC] SET Columns = '140,A,25,140,A,25' WHERE [DescId] = 101000

UPDATE [desc] SET DispOrder = 0 where  [FILE]='USER' AND [DispOrder]>0

/* Partie haute - Photo et infos de base*/
UPDATE [DESC] SET [X] = 0, [Y] = @Line, [ROWSPAN] = 9, [COLSPAN] = 1 , DispOrder = 1 WHERE [DESCID] = 101075
 

	UPDATE [DESC] SET [X] = 1, [Y] = @Line, [ROWSPAN] = 1, [COLSPAN] = 1, [DispOrder] = (select MAX(disporder) + 1 from [desc]  where  [FILE]='USER' )  WHERE [DESCID] = 101001 -- Login
	set @Line = @Line +1

	UPDATE [DESC] SET [X] = 1, [Y] = @Line, [ROWSPAN] = 1, [COLSPAN] = 1, [DispOrder] = (select MAX(disporder) + 1 from [desc]  where  [FILE]='USER' )  WHERE [DESCID] = 101002 -- Nom complet 
	set @Line = @Line +1

	UPDATE [DESC] SET [X] = 1, [Y] = @Line, [ROWSPAN] = 1, [COLSPAN] = 1, [DispOrder] = (select MAX(disporder) + 1 from [desc]  where  [FILE]='USER' )  WHERE [DESCID] = 101010 -- Fonction
	set @Line = @Line +1

	UPDATE [DESC] SET [X] = 1, [Y] = @Line, [ROWSPAN] = 1, [COLSPAN] = 1, [DispOrder] = (select MAX(disporder) + 1 from [desc]  where  [FILE]='USER' )  WHERE [DESCID] = 101007 -- tél
	set @Line = @Line +1

	UPDATE [DESC] SET [X] = 1, [Y] = @Line, [ROWSPAN] = 1, [COLSPAN] = 1, [DispOrder] = (select MAX(disporder) + 1 from [desc]  where  [FILE]='USER' )  WHERE [DESCID] = 101009 -- portable
	set @Line = @Line +1

	UPDATE [DESC] SET [X] = 1, [Y] = @Line, [ROWSPAN] = 1, [COLSPAN] = 1, [DispOrder] = (select MAX(disporder) + 1 from [desc]  where  [FILE]='USER' )  WHERE [DESCID] = 101008 -- fax
	set @Line = @Line +1

	UPDATE [DESC] SET [X] = 1, [Y] = @Line, [ROWSPAN] = 1, [COLSPAN] = 1, [DispOrder] = (select MAX(disporder) + 1 from [desc]  where  [FILE]='USER' )  WHERE [DESCID] = 101004 -- email
	set @Line = @Line +1
	
	UPDATE [DESC] SET [X] = 1, [Y] = @Line, [ROWSPAN] = 1, [COLSPAN] = 1, [DispOrder] = (select MAX(disporder) + 1 from [desc]  where  [FILE]='USER' )  WHERE [DESCID] = 101018 -- email autre
	set @Line = @Line +1
	
	UPDATE [DESC] SET [X] = 1, [Y] = @Line, [ROWSPAN] = 1, [COLSPAN] = 1, [DispOrder] = (select MAX(disporder) + 1 from [desc]  where  [FILE]='USER' )  WHERE [DESCID] = 101023 -- Langue
	set @Line = @Line +1

/*Coordonnées*/
UPDATE [DESC] SET [X] = 0, [Y] = @Line, [ROWSPAN] = 1, [COLSPAN] = 2, [DispOrder] = (select MAX(disporder) + 1 from [desc]  where  [FILE]='USER' )  WHERE [DESCID] = 101024 -- TITRE SEPARATEUR
set @Line = @Line+1

UPDATE [DESC] SET [X] = 0, [Y] = @Line, [ROWSPAN] = 3, [COLSPAN] = 2, [DispOrder] = (select MAX(disporder) + 1 from [desc]  where  [FILE]='USER' )  WHERE [DESCID] = 101006 -- Addresse
set @Line = @Line+3

UPDATE [DESC] SET [X] = 0, [Y] = @Line, [ROWSPAN] = 1, [COLSPAN] = 1, [DispOrder] = (select MAX(disporder) + 1 from [desc]  where  [FILE]='USER' )  WHERE [DESCID] = 101013 -- CP
	UPDATE [DESC] SET [X] = 1, [Y] = @Line, [ROWSPAN] = 1, [COLSPAN] = 1, [DispOrder] = (select MAX(disporder) + 1 from [desc]  where  [FILE]='USER' )  WHERE [DESCID] = 101014 -- Ville
set @Line = @Line+1

/* Extensions */
UPDATE [DESC] SET [X] = 0, [Y] = @Line, [ROWSPAN] = 1, [COLSPAN] = 2, [DispOrder] = (select MAX(disporder) + 1 from [desc]  where  [FILE]='USER' )  WHERE [DESCID] = 101031  -- TITRE SEPARATEUR
set @Line = @Line+1

UPDATE [DESC] SET [X] = 0, [Y] = @Line, [ROWSPAN] = 1, [COLSPAN] = 1, [DispOrder] = (select MAX(disporder) + 1 from [desc]  where  [FILE]='USER' )  WHERE [DESCID] = 101020  -- Exchange
set @Line = @Line+1


/*Habilitation & Apparatenance*/
UPDATE [DESC] SET [X] = 0, [Y] = @Line, [ROWSPAN] = 1, [COLSPAN] = 2, [DispOrder] = (select MAX(disporder) + 1 from [desc]  where  [FILE]='USER' )  WHERE [DESCID] = 101025
set @Line = @Line+1

UPDATE [DESC] SET [X] = 0, [Y] = @Line, [ROWSPAN] = 1, [COLSPAN] = 1, [DispOrder] = (select MAX(disporder) + 1 from [desc]  where  [FILE]='USER' )  WHERE [DESCID] = 101011 -- Désactivé
	UPDATE [DESC] SET [X] = 1, [Y] = @Line, [ROWSPAN] = 1, [COLSPAN] = 1, [DispOrder] = (select MAX(disporder) + 1 from [desc]  where  [FILE]='USER' )  WHERE [DESCID] = 101012 -- Masqué
set @Line = @Line+1

UPDATE [DESC] SET [X] = 0, [Y] = @Line, [ROWSPAN] = 1, [COLSPAN] = 1, [DispOrder] = (select MAX(disporder) + 1 from [desc]  where  [FILE]='USER' )  WHERE [DESCID] = 101027 -- Groupe

UPDATE [DESC] SET [X] = 1, [Y] = @Line, [ROWSPAN] = 1, [COLSPAN] = 1, [DispOrder] = (select MAX(disporder) + 1 from [desc]  where  [FILE]='USER' )  WHERE [DESCID] = 101017 -- Niveau
set @Line = @Line+1	

UPDATE [DESC] SET [X] = 1, [Y] = @Line, [ROWSPAN] = 1, [COLSPAN] = 1, [DispOrder] = (select MAX(disporder) + 1 from [desc]  where  [FILE]='USER' )  WHERE [DESCID] = 101036 -- Profil utilisateur
set @Line = @Line+1	

UPDATE [DESC] SET [X] = 0, [Y] = @Line, [ROWSPAN] = 1, [COLSPAN] = 1, [DispOrder] = (select MAX(disporder) + 1 from [desc]  where  [FILE]='USER' )  WHERE [DESCID] = 101028 -- Mdp not expire
	UPDATE [DESC] SET [X] = 1, [Y] = @Line, [ROWSPAN] = 1, [COLSPAN] = 1 , [DispOrder] = (select MAX(disporder) + 1 from [desc]  where  [FILE]='USER' )  WHERE [DESCID] = 101032 -- Profil
set @Line = @Line+1

UPDATE [DESC] SET [X] = 1, [Y] = @Line, [ROWSPAN] = 1, [COLSPAN] = 1, [DispOrder] = (select MAX(disporder) + 1 from [desc]  where  [FILE]='USER' )  WHERE [DESCID] = 101034 -- Product

UPDATE [DESC] SET [X] = 0, [Y] = @Line, [ROWSPAN] = 1, [COLSPAN] = 1, [DispOrder] = (select MAX(disporder) + 1 from [desc]  where  [FILE]='USER' )  WHERE [DESCID] = 101035 -- Mdp not expire
set @Line = @Line+1

/*Informations complémentaires*/
UPDATE [DESC] SET [X] = 0, [Y] = @Line, [ROWSPAN] = 1, [COLSPAN] = 2, [DispOrder] = (select MAX(disporder) + 1 from [desc]  where  [FILE]='USER' )  WHERE [DESCID] = 101026 --
set @Line = @Line+1

UPDATE [DESC] SET [X] = 0, [Y] = @Line, [ROWSPAN] = 5, [COLSPAN] = 2, [DispOrder] = (select MAX(disporder) + 1 from [desc]  where  [FILE]='USER' )  WHERE [DESCID] = 101003 --Description