DECLARE @AdrPerso_00 AS VARCHAR(128)
	,@AdrPerso_01 AS VARCHAR(128)
	,@AdrPerso_02 AS VARCHAR(128)
	,@AdrPerso_03 AS VARCHAR(128)
	,@AdrPerso_04 AS VARCHAR(128)
	,@AdrPerso_05 AS VARCHAR(128)
	,@AdrPro_00 AS VARCHAR(128)
	,@AdrPro_01 AS VARCHAR(128)
	,@AdrPro_02 AS VARCHAR(128)
	,@AdrPro_03 AS VARCHAR(128)
	,@AdrPro_04 AS VARCHAR(128)
	,@AdrPro_05 AS VARCHAR(128)
	,@ADR01_LENGTH AS NUMERIC
	,@ADR01_SQLLENGTH AS NUMERIC

SELECT @AdrPerso_00 = 'Adresse Personnelle'
	,@AdrPerso_01 = 'Personal address'
	,@AdrPerso_02 = 'Privatadresse'
	,@AdrPerso_03 = 'Persoonlijk adres'
	,@AdrPerso_04 = 'Dirección personal'
	,@AdrPerso_05 = 'Indirizzo personale'
	,@AdrPro_00 = 'Adresse professionnelle'
	,@AdrPro_01 = 'Professional address'
	,@AdrPro_02 = 'Berufliche Adresse'
	,@AdrPro_03 = 'Professioneel adres'
	,@AdrPro_04 = 'Dirección profesional'
	,@AdrPro_05 = 'Indirizzo professionale'
	

SELECT @ADR01_SQLLENGTH = character_maximum_length , @ADR01_LENGTH =D.Length  
FROM information_schema.columns
INNER JOIN [DESC] D on D.Field  =  column_name  
WHERE column_name = 'ADR01' and table_name = 'ADDRESS'

IF(@ADR01_SQLLENGTH != @ADR01_LENGTH and @ADR01_SQLLENGTH >0)
BEGIN
UPDATE [DESC] SET Length = @ADR01_SQLLENGTH WHERE DESCID = 401;
END

UPDATE ADDRESS
SET ADR01 = [dbo].[getPpName](PP01, PP02, PP03, 0) + ISNULL(' - ' + CASE 
			WHEN ADR92 = 1
				THEN CASE [USER].Lang
						WHEN 'LANG_01'
							THEN @AdrPerso_01
						WHEN 'LANG_02'
							THEN @AdrPerso_02
						WHEN 'LANG_03'
							THEN @AdrPerso_03
						WHEN 'LANG_04'
							THEN @AdrPerso_04
						WHEN 'LANG_04'
							THEN @AdrPerso_05
						WHEN 'LANG_05'
							THEN @AdrPerso_05
						ELSE @AdrPerso_00
						END
			ELSE PM01
			END, ''),
ADR01_AUTOBUILD = 1
FROM ADDRESS
LEFT JOIN [USER] ON UserId = Address.ADR97
LEFT JOIN PP ON PP.PPID = ADDRESS.PPID
LEFT JOIN PM ON PM.PMID = ADDRESS.PMID
WHERE LEN([dbo].[getPpName](PP01, PP02, PP03, 0) + ISNULL(' - ' + CASE 
			WHEN ADR92 = 1
				THEN CASE [USER].Lang
						WHEN 'LANG_01'
							THEN @AdrPerso_01
						WHEN 'LANG_02'
							THEN @AdrPerso_02
						WHEN 'LANG_03'
							THEN @AdrPerso_03
						WHEN 'LANG_04'
							THEN @AdrPerso_04
						WHEN 'LANG_04'
							THEN @AdrPerso_05
						WHEN 'LANG_05'
							THEN @AdrPerso_05
						ELSE @AdrPerso_00
						END
			ELSE PM01
			END, '')
		) <= @ADR01_SQLLENGTH
