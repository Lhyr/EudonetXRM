IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[USER]') 
         AND name = 'PASSWORD_POLICIES_ALGO' 
)
BEGIN
	ALTER TABLE [USER] Add PASSWORD_POLICIES_ALGO Varchar(50) DEFAULT ''
END 



IF NOT EXISTS ( SELECT 1 FROM [DESC] WHERE [DESCID] = 101035)
	INSERT INTO[DESC] ([DESCID], [FILE], [FIELD], [FORMAT], [LENGTH], [ReadOnly],[popup]) SELECT 101035, 'USER', 'PASSWORD_POLICIES_ALGO', 1, 100,1,6
ELSE
	UPDATE [DESC] SET [FORMAT] = 1, [ReadOnly] = 1, [length]=100, [popup] = 6 WHERE [DESCID] = 101035

IF NOT EXISTS ( SELECT 1 FROM [RES] WHERE [RESID] = 101035)
	INSERT INTO [RES] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select 101035, 'Niveau de sécurité', 'Security level', '', '', ''
	