IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[FORMULARXRM]') 
         AND name = 'LANG'
)
BEGIN
	Alter Table [FORMULARXRM] Add LANG Varchar(7)
end


-- Langue du formulaire
IF NOT EXISTS ( SELECT 1 FROM [DESC] WHERE [DESCID] = 113024)
	INSERT INTO[DESC] ([DESCID], [FILE], [FIELD], [FORMAT], [LENGTH]) SELECT 113024, 'FORMULARXRM', 'LANG', 1, 7

IF NOT EXISTS ( SELECT 1 FROM [RES] WHERE [RESID] = 113024)
	INSERT INTO [RES] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select 113024, 'Langue du formulaire', '', '', '', ''