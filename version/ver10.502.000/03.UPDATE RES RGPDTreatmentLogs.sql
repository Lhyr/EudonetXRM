DECLARE @nTab INT = 117000

UPDATE [RES] SET
[LANG_00] = 'Journal des traitements'
,[LANG_01] = 'Treatments logs'
,[LANG_02] = 'Treatments logs'
,[LANG_03] = 'Treatments logs'
,[LANG_04] = 'Treatments logs'
,[LANG_05] = 'Treatments logs'
WHERE [ResId] = @nTab
