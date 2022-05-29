/*    HLA/SPH
      Création du champ autobuild pour indiquer si l'event à un nom autogénéré
      12/05/2017
*/
SET NOCOUNT ON;

DECLARE @tabName AS VARCHAR(100)

DECLARE curs CURSOR
FOR
SELECT [FILE] 
FROM [DESC]  d
left join sys.tables st on st.name = d.[file]
WHERE d.[TYPE] = 0 AND [FIELD] = 'EVT' and st.name is not null


OPEN curs

FETCH NEXT
FROM curs
INTO @tabName

WHILE @@FETCH_STATUS = 0
BEGIN
	IF NOT EXISTS (
			SELECT 1
			FROM sys.tables
			INNER JOIN syscolumns ON syscolumns.id = sys.tables.object_id
			WHERE sys.tables.NAME LIKE @tabName
				AND syscolumns.NAME LIKE 'EVT01_AUTOBUILD'
			)
	BEGIN
		--EXEC ('ALTER TABLE [' + @tabName + '] ADD [EVT01_AUTOBUILD] [BIT] CONSTRAINT [DF_' + @tabName + '_EVT01_AUTOBUILD] DEFAULT(1) NOT NULL ');
		EXEC ('ALTER TABLE [' + @tabName + '] ADD [EVT01_AUTOBUILD] [BIT] NULL');
	END

	FETCH NEXT
	FROM curs
	INTO @tabName
END

CLOSE curs

DEALLOCATE curs
