-- Meta Titre
IF NOT EXISTS ( SELECT 1
		  from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		  where sys.tables.name like 'FORMULARXRM' and syscolumns.name = 'MetaTitle')
BEGIN            
	ALTER TABLE [FORMULARXRM] ADD [MetaTitle] NVARCHAR(1000) DEFAULT NULL
END

-- Meta Description
IF NOT EXISTS ( SELECT 1
		  from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		  where sys.tables.name like 'FORMULARXRM' and syscolumns.name = 'MetaDescription')
BEGIN            
	ALTER TABLE [FORMULARXRM] ADD [MetaDescription] NVARCHAR(MAX) DEFAULT NULL
END
 
-- Meta Image URL
IF NOT EXISTS ( SELECT 1
		  from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		  where sys.tables.name like 'FORMULARXRM' and syscolumns.name = 'MetaImgURL')
BEGIN            
	ALTER TABLE [FORMULARXRM] ADD [MetaImgURL] NVARCHAR(2000) DEFAULT NULL
END


-- Création dans DESC et RES

INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) SELECT 113019, 'FORMULARXRM', 'MetaTitle', 1, 1000 
WHERE NOT EXISTS (SELECT DescId FROM [DESC] WHERE DescId = 113019)

INSERT INTO [RES] (ResId, LANG_00, LANG_01, LANG_02, LANG_03, LANG_04) SELECT 113019, 'Meta Titre','Meta Title','Meta Title','Meta Title','Meta Title'
WHERE NOT EXISTS (SELECT ResId FROM [RES] WHERE ResId = 113019)


INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) SELECT 113020, 'FORMULARXRM', 'MetaDescription', 9, 8000 
WHERE NOT EXISTS (SELECT DescId FROM [DESC] WHERE DescId = 113020)

INSERT INTO [RES] (ResId, LANG_00, LANG_01, LANG_02, LANG_03, LANG_04) SELECT 113020, 'Meta Description','Meta Description','Meta Description','Meta Description','Meta Description'
WHERE NOT EXISTS (SELECT ResId FROM [RES] WHERE ResId = 113020)


INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) SELECT 113021, 'FORMULARXRM', 'MetaImgURL', 1, 2000 
WHERE NOT EXISTS (SELECT DescId FROM [DESC] WHERE DescId = 113021)

INSERT INTO [RES] (ResId, LANG_00, LANG_01, LANG_02, LANG_03, LANG_04) SELECT 113021, 'Meta Image','Meta Image','Meta Image','Meta Image','Meta Image'
WHERE NOT EXISTS (SELECT ResId FROM [RES] WHERE ResId = 113021)