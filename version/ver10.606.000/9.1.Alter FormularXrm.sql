-- KJE: #US #1 984 #Tâche #2 865, Ajout d'un champ "status"

IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[FORMULARXRM]') 
         AND name = 'STATUS' 
)
BEGIN
	ALTER TABLE [FORMULARXRM] Add [STATUS] Varchar(50) DEFAULT ''
END 



IF NOT EXISTS ( SELECT 1 FROM [DESC] WHERE [DESCID] = 113025)
	INSERT INTO[DESC] ([DESCID], [FILE], [FIELD], [FORMAT], [LENGTH], [ReadOnly],[popup]) SELECT 113025, 'FORMULARXRM', 'STATUS', 1, 100,1,6

IF NOT EXISTS ( SELECT 1 FROM [RES] WHERE [RESID] = 113025)
	INSERT INTO [RES] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select 113025, 'Statut', 'Status', '', '', ''
	
	
GO


	