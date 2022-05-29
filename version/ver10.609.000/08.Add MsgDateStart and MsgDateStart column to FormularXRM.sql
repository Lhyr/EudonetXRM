IF NOT EXISTS (
 -- AAB:  #Tâche #3 418, Ajout d'un champ "MsgDateStart" et "MsgDateEnd"
 
 SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[FORMULARXRM]') 
         AND name = 'MsgDateStart' OR name = 'MsgDateEnd'
)
BEGIN
    ALTER TABLE [FORMULARXRM] ADD [MsgDateStart] varbinary(max) DEFAULT NULL;
    ALTER TABLE [FORMULARXRM] ADD [MsgDateEnd] varbinary(max) DEFAULT NULL;
END 

IF NOT EXISTS (SELECT 1 FROM [DESC] WHERE [DESCID] = 113027)
    INSERT INTO [DESC] ([DESCID], [FILE], [FIELD], [FORMAT], [LENGTH], [ReadOnly],[popup]) SELECT 113027, 'FORMULARXRM', 'MsgDateStart', 21, -1,0,0

	IF NOT EXISTS (SELECT 1 FROM [DESC] WHERE [DESCID] = 113028)
    INSERT INTO [DESC] ([DESCID], [FILE], [FIELD], [FORMAT], [LENGTH], [ReadOnly],[popup]) SELECT 113028, 'FORMULARXRM', 'MsgDateEnd', 21, -1,0,0 

IF NOT EXISTS (SELECT 1 FROM [RES] WHERE [RESID] = 113027)
    INSERT INTO [RES] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) SELECT 113027, 'Message Date de début', 'Start Date Message', '', '', ''

IF NOT EXISTS (SELECT 1 FROM [RES] WHERE [RESID] = 113028)
    INSERT INTO [RES] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) SELECT 113028, 'Message Date de fin', 'End Date Message', '', '', ''
    
    
GO