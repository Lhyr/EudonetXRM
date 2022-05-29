ALTER TABLE FORMULARXRM ALTER COLUMN UserId NUMERIC NULL

-- Création des nouvelles colonnes dans FORMULARXRM

IF NOT EXISTS (
		SELECT 1
		FROM sys.columns sc
		INNER JOIN sys.tables st ON st.object_id = sc.object_id
			AND st.NAME collate french_ci_ai = 'FORMULARXRM' collate french_ci_ai
		WHERE sc.NAME collate french_ci_ai = 'FacebookShare' collate french_ci_ai
		)
BEGIN
	ALTER TABLE FORMULARXRM ADD FacebookShare BIT
END

IF NOT EXISTS (
		SELECT 1
		FROM sys.columns sc
		INNER JOIN sys.tables st ON st.object_id = sc.object_id
			AND st.NAME collate french_ci_ai = 'FORMULARXRM' collate french_ci_ai
		WHERE sc.NAME collate french_ci_ai = 'GoogleShare' collate french_ci_ai
		)
BEGIN
	ALTER TABLE FORMULARXRM ADD GoogleShare BIT
END

IF NOT EXISTS (
		SELECT 1
		FROM sys.columns sc
		INNER JOIN sys.tables st ON st.object_id = sc.object_id
			AND st.NAME collate french_ci_ai = 'FORMULARXRM' collate french_ci_ai
		WHERE sc.NAME collate french_ci_ai = 'TwitterShare' collate french_ci_ai
		)
BEGIN
	ALTER TABLE FORMULARXRM ADD TwitterShare BIT
END

IF NOT EXISTS (
		SELECT 1
		FROM sys.columns sc
		INNER JOIN sys.tables st ON st.object_id = sc.object_id
			AND st.NAME collate french_ci_ai = 'FORMULARXRM' collate french_ci_ai
		WHERE sc.NAME collate french_ci_ai = 'LinkedinShare' collate french_ci_ai
		)
BEGIN
	ALTER TABLE FORMULARXRM ADD LinkedinShare BIT
END

-- Création dans DESC et RES

INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) SELECT 113015, 'FORMULARXRM', 'FacebookShare', 3, 1 
WHERE NOT EXISTS (SELECT DescId FROM [DESC] WHERE DescId = 113015)

INSERT INTO [RES] (ResId, LANG_00, LANG_01, LANG_02, LANG_03, LANG_04) SELECT 113015, 'Partage Facebook','Facebook Share','Facebook Share','Facebook Share','Facebook Share'
WHERE NOT EXISTS (SELECT ResId FROM [RES] WHERE ResId = 113015)


INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) SELECT 113016, 'FORMULARXRM', 'GoogleShare', 3, 1 
WHERE NOT EXISTS (SELECT DescId FROM [DESC] WHERE DescId = 113016)

INSERT INTO [RES] (ResId, LANG_00, LANG_01, LANG_02, LANG_03, LANG_04) SELECT 113016, 'Partage Google','Google Share','Google Share','Google Share','Google Share'
WHERE NOT EXISTS (SELECT ResId FROM [RES] WHERE ResId = 113016)


INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) SELECT 113017, 'FORMULARXRM', 'TwitterShare', 3, 1 
WHERE NOT EXISTS (SELECT DescId FROM [DESC] WHERE DescId = 113017)

INSERT INTO [RES] (ResId, LANG_00, LANG_01, LANG_02, LANG_03, LANG_04) SELECT 113017, 'Partage Twitter','Twitter Share','Twitter Share','Twitter Share','Twitter Share'
WHERE NOT EXISTS (SELECT ResId FROM [RES] WHERE ResId = 113017)


INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) SELECT 113018, 'FORMULARXRM', 'LinkedinShare', 3, 1 
WHERE NOT EXISTS (SELECT DescId FROM [DESC] WHERE DescId = 113018)

INSERT INTO [RES] (ResId, LANG_00, LANG_01, LANG_02, LANG_03, LANG_04) SELECT 113018, 'Partage Linkedin','Linkedin Share','Linkedin Share','Linkedin Share','Linkedin Share'
WHERE NOT EXISTS (SELECT ResId FROM [RES] WHERE ResId = 113018)
