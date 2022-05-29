IF NOT EXISTS (
 -- AAB:  #Tâche #3 363, Ajout d'un champ "StartDate"
 
 SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[FORMULARXRM]') 
         AND name = 'STARTDATE' 
)
BEGIN
    ALTER TABLE [FORMULARXRM] Add [STARTDATE] datetime DEFAULT ''
END 

 

 

IF NOT EXISTS ( SELECT 1 FROM [DESC] WHERE [DESCID] = 113026)
    INSERT INTO[DESC] ([DESCID], [FILE], [FIELD], [FORMAT], [LENGTH], [ReadOnly],[popup]) SELECT 113026, 'FORMULARXRM', 'STARTDATE', 2, 0,0,0

 

IF NOT EXISTS ( SELECT 1 FROM [RES] WHERE [RESID] = 113026)
    INSERT INTO [RES] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select 113026, 'Date de début', 'START DATE', '', '', ''
    
    
GO