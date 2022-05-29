-- Création de la colonne MetaTags dans FORMULARXRM

IF NOT EXISTS (
		SELECT 1
		FROM sys.columns sc
		INNER JOIN sys.tables st ON st.object_id = sc.object_id
			AND st.NAME collate french_ci_ai = 'FORMULARXRM' collate french_ci_ai
		WHERE sc.NAME collate french_ci_ai = 'MetaTags' collate french_ci_ai
		)
BEGIN
	ALTER TABLE FORMULARXRM ADD MetaTags NVARCHAR(MAX)
	
	-- placé ici pour ne pas que cette activation soit relancé sur les descentes/remontés de version
	update [desc] set ActiveTab=1 where descid = 102000
	
END

-- Création dans DESC et RES

INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) SELECT 113014, 'FORMULARXRM', 'MetaTags', 9, 8000 
WHERE NOT EXISTS (SELECT DescId FROM [DESC] WHERE DescId = 113014)

INSERT INTO [RES] (ResId, LANG_00, LANG_01, LANG_02, LANG_03, LANG_04) SELECT 113014, 'Balises meta','Meta tags','Meta tags','Meta tags','Meta tags'
WHERE NOT EXISTS (SELECT ResId FROM [RES] WHERE ResId = 113014)
