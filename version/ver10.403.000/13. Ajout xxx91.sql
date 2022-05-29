

INSERT INTO [DESC] (
	DescId
	,[File]
	,Field
	,Format
	)
SELECT 107091
	,'MAILTEMPLATE'
	,'MAILTEMPLATE91'
	,30
WHERE NOT EXISTS (
		SELECT DescID
		FROM [DESC]
		WHERE DescId = 107091
		)

INSERT INTO RES (
	ResId
	,LANG_00
	,LANG_01
	,LANG_02
	,LANG_03
	,LANG_04
	,LANG_05
	,LANG_06
	,LANG_07
	,LANG_08
	,LANG_09
	)
SELECT 107091
	,'Annexes'
	,'Attachments'
	,'Anlagen'
	,'Bijlagen'
	,'Anexos'
	,'Allegati'
	,'Attachments'
	,'Attachments'
	,'Attachments'
	,'Attachments'
WHERE NOT EXISTS (
		SELECT ResId
		FROM RES
		WHERE ResId = 107091
		)

INSERT INTO [DESC] (
	DescId
	,[File]
	,Field
	,Format
	)
SELECT 491
	,'ADDRESS'
	,'ADR91'
	,30
WHERE NOT EXISTS (
		SELECT DescID
		FROM [DESC]
		WHERE DescId = 491
		)

INSERT INTO RES (
	ResId
	,LANG_00
	,LANG_01
	,LANG_02
	,LANG_03
	,LANG_04
	,LANG_05
	,LANG_06
	,LANG_07
	,LANG_08
	,LANG_09
	)
SELECT 491
	,'Annexes'
	,'Attachments'
	,'Anlagen'
	,'Bijlagen'
	,'Anexos'
	,'Allegati'
	,'Attachments'
	,'Attachments'
	,'Attachments'
	,'Attachments'
WHERE NOT EXISTS (
		SELECT ResId
		FROM RES
		WHERE ResId = 491
		)

	
DECLARE @sRequest AS NVARCHAR(max)

SET @sRequest = ''
--La clause not null n'a pas été activée car elle aurait pu être source de problème sur des bases contenant un trop grand nombre d'enregistrement.
SELECT @sRequest += 'ALTER TABLE ' + [File] + ' ADD ' + [Field] + '91 Int CONSTRAINT __DF__'+ Cast(([DESC].Descid + 91) as varchar(100)) + ' DEFAULT 0;' 

FROM [DESC]
WHERE DescId % 100 = 0
	AND EXISTS (
		SELECT 1
		FROM [DESC] PJ
		WHERE [DESC].DescId + '91' = PJ.DescId
		)
	AND NOT EXISTS (
		SELECT 1
		FROM sys.columns sc
		INNER JOIN sys.tables st ON st.object_id = sc.object_id
			AND st.NAME collate french_ci_ai = [DESC].[File] collate french_ci_ai
		WHERE sc.NAME collate french_ci_ai = [DESC].Field + '91' collate french_ci_ai
	)

exec sp_executesql @sRequest


UPDATE [DESC] SET Format = 30 WHERE [DESC].DescId % 100 = 91