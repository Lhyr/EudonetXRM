IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[FORMULARXRM]') 
         AND name = 'EXTENDEDPARAM'
)
BEGIN
	Alter Table [FORMULARXRM] Add EXTENDEDPARAM NVarchar(max)
end


IF NOT EXISTS ( SELECT 1 FROM [DESC] WHERE [DESCID] = 113023)
INSERT INTO[DESC] ([DESCID], [FILE], [FIELD], [FORMAT], [LENGTH], [Unicode]) SELECT 113023, 'FORMULARXRM', 'EXTENDEDPARAM', 1, -1,1

IF NOT EXISTS ( SELECT 1 FROM [RES] WHERE [RESID] = 113023)
	INSERT INTO [RES] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select 113023, 'Paramètres avancés', '', '', '', ''