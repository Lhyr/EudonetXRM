-- SHA : Création du champ Texte d'aperçu
DECLARE @bIsUnicode BIT = 0

IF EXISTS (
		SELECT *
		FROM [CONFIGADV]
		WHERE [Parameter] = 'FULL_UNICODE'
			AND [Value] = '1'
		)
BEGIN
	SET @bIsUnicode = 1
END

SET NOCOUNT ON;

DECLARE @nTab INT
	,@tabName AS VARCHAR(100)

DECLARE curs CURSOR
FOR
SELECT [DescId]
	,[FILE]
FROM [DESC]
WHERE [TYPE] = 3

OPEN curs

FETCH NEXT
FROM curs
INTO @nTab
	,@tabName

WHILE @@FETCH_STATUS = 0
BEGIN
	--28	Texte d'aperçu (Preheader)
	IF NOT EXISTS (
			SELECT 1
			FROM sys.tables
			INNER JOIN syscolumns ON syscolumns.id = sys.tables.object_id
			WHERE sys.tables.name LIKE @tabName
				AND syscolumns.name LIKE 'TPL28'
			)
	BEGIN
	IF @bIsUnicode = 1
		BEGIN
			EXEC (
				'BEGIN
					ALTER TABLE [' + @tabName + '] ADD [TPL28] NVARCHAR(140) NULL;
				END');
		END
	ELSE
		BEGIN
			EXEC (
				'BEGIN
					ALTER TABLE [' + @tabName + '] ADD [TPL28] VARCHAR(140) NULL;
				END');
		END
	END

	IF (
			(
				SELECT COUNT(*)
				FROM [DESC]
				WHERE DescId = @nTab + 28
				) <= 0
			)
	BEGIN
		INSERT INTO [desc] (
			[DescId]
			,[File]
			,[Field]
			,[Format]
			,[Length]
			,[disporder]
			,[readonly]
			)
		SELECT @nTab + 28
			,@tabName
			,'TPL28'
			,1
			,140
			,28
			,0
	END

	IF (
			(
				SELECT COUNT(*)
				FROM [Res]
				WHERE ResId = @nTab + 28
				) <= 0
			)
	BEGIN
		INSERT INTO [res] (
			resid
			,lang_00
			,lang_01
			,lang_02
			,lang_03
			,lang_04
			)
		SELECT @nTab + 28
			,'Texte d''aperçu'
			,'Preheader'
			,'Preheader'
			,'Preheader'
			,'Preheader'
	END

	FETCH NEXT
	FROM curs
	INTO @nTab
		,@tabName
END

CLOSE curs

DEALLOCATE curs
