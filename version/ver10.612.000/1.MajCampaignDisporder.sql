/*
AUTHORS : SPHAM/ASTAN
DATE : 28/10/2020
US : #2576
Tache : #3699

Affichage des champs suivant en mode fiche via attribution d'un disporder

		- Date d’expiration (descid 106010 )
		- Date de purge (descid 106025 )
		- Historiser (descid  106011)
		- Catégorie (descid 106008)
*/

SET NOCOUNT ON

DECLARE @DescId AS INT
DECLARE MajDisporder CURSOR FOR

SELECT DescId  FROM [DESC] dd
WHERE 
		[file]= 'CAMPAIGN' 
	AND DescId IN ( 106010, 106025, 106011, 106008,106017)
	AND IsNull(disporder, 0) = 0
	ORDER BY DispOrder ASC


OPEN MajDisporder

FETCH MajDisporder INTO @DescId

WHILE @@FETCH_STATUS = 0
BEGIN

	-- Maj du disporder pour le champ @DescId
	 UPDATE [DESC] SET DispOrder = 
		(
			SELECT TOP 1 N FROM cfc_getIDs() 
			WHERE 
					-- Disporder entre 1 et 69
					N >= 1  AND N <= 69
					
				AND 
					-- Et pas déjà utilisé
					N NOT IN (
					SELECT  DispOrder FROM [DESC]  
					WHERE [FILE]= 'CAMPAIGN' AND DispOrder > 0
				)
				ORDER by N  
		) 
	WHERE descid = @DescId

	FETCH MajDisporder INTO @DescId

END

CLOSE MajDisporder
DEALLOCATE MajDisporder

IF EXISTS ( SELECT 1 FROM [DESC] WHERE Columns='A,A,25,A,A,25' AND [DESCID] = 106000)
	UPDATE [DESC] SET Columns='300,A,25,300,A,25'  WHERE Columns='A,A,25,A,A,25' AND [DESCID] = 106000


SET NOCOUNT OFF