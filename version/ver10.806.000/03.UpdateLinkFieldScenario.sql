SET NOCOUNT ON

DECLARE @RQ as VARCHAR(MAX)


DECLARE CURS CURSOR FOR
	SELECT
	'UPDATE [WORKFLOWSCENARIO] SET [' + Field + '] =  [WFTEVENTFILEID] WHERE  [' + Field + '] IS NULL AND  [WFTEVENTDESCID] = ' + CAST( RelationFileDescId as varchar(500)) 

	FROM [cfc_getLiaison](119200)


OPEN CURS
FETCH NEXT FROM CURS INTO @RQ

WHILE @@FETCH_STATUS = 0
BEGIN

	EXECUTE( @RQ)
	FETCH NEXT FROM CURS INTO @RQ

END

CLOSE CURS
DEALLOCATE CURS


 