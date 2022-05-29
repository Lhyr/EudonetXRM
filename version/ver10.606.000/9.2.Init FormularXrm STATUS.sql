-- Initialisation de la colonne status dans formularxrm pour les formulaires avancés
IF EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[FORMULARXRM]') 
         AND name = 'STATUS' 
)
BEGIN
	UPDATE [FORMULARXRM] SET [STATUS] =
	CASE 
		WHEN  ISNULL([STATUS],'') = '' then '0'
	END
WHERE [STATUS] IS NULL AND [TYPE] = 1
END 

	