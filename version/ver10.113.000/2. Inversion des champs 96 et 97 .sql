

IF EXISTS(select * from [desc] where descid = 113097 and field like 'ModifiedOn')
BEGIN

UPDATE [DESC] SET [field] ='ModifiedOn' WHERE [descid] =113096;
UPDATE [RES] SET [lang_00] =  'Modifié le', [lang_01] =  'Modified on', [lang_02] =  'Modified on', [lang_03] =  'Modified on', [lang_04] =  'Modified on'  WHERE resid = 113096;

UPDATE [DESC] SET [field] ='CreatedBy' WHERE [descid] =113097;
UPDATE [RES] SET [lang_00] =  'Créé par', [lang_01] =  'Created by', [lang_02] =  'Created by', [lang_03] =  'Created by', [lang_04] =  'Created by'  WHERE resid = 113097;

END


IF EXISTS(select * from [desc] where descid = 107097 and field like 'ModifiedOn')
BEGIN

UPDATE [DESC] SET [field] ='ModifiedOn' WHERE [descid] =107096;
UPDATE [RES] SET [lang_00] =  'Modifié le', [lang_01] =  'Modified on', [lang_02] =  'Modified on', [lang_03] =  'Modified on', [lang_04] =  'Modified on'  WHERE resid = 113096;

UPDATE [DESC] SET [field] ='CreatedBy' WHERE [descid] =107097;
UPDATE [RES] SET [lang_00] =  'Créé par', [lang_01] =  'Created by', [lang_02] =  'Created by', [lang_03] =  'Created by', [lang_04] =  'Created by'  WHERE resid = 113097;
END