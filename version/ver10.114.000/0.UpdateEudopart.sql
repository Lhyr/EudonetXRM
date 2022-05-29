/*
 LMO/NLA : 
 Maj des eudopart pour migration

 */
DECLARE @Cherche AS VARCHAR(max)
DECLARE @Pos AS NUMERIC
DECLARE @Cont AS VARCHAR(max)
DECLARE @Eudoid AS NUMERIC
DECLARE @Content AS VARCHAR(max)
DECLARE @Compt AS NUMERIC
DECLARE @Newv AS VARCHAR(max) = 'xrm.eudonet.com/XRM/'

DECLARE Curs CURSOR
FOR
SELECT eudopartid
	,eudopartcontent
FROM [eudopart]
WHERE EudoPartContent LIKE '%ww4.eudonet%'
	OR EudoPartContent LIKE '%ww5.eudonet%'
	OR EudoPartContent LIKE '%www.eudonet%'
	OR EudoPartContent LIKE '%ww2.eudonet%'

OPEN Curs

FETCH Curs
INTO @Eudoid
	,@Content

SET @COMPT = 0

WHILE @@Fetch_status = 0
BEGIN
	SET @Cherche = @Content
	SET @Pos = charindex('V7/datas', @Cherche, 0)

	IF (
			@Pos <> 0
			OR @Pos <> NULL
			)
	BEGIN
		SET @Pos = - 1

		WHILE ((charindex('V7/datas', @Cherche, @Pos + 1)) <> 0)
		BEGIN
			SET @Pos = charindex('V7/datas', @Cherche, @Pos + 1)
			SET @Cherche = SUBSTRING(@Cherche, 0, @Pos - 16) + @Newv + SUBSTRING(@Cherche, @Pos + 3, LEN(@Cherche))

			update [EUDOPART] set EudoPartContent = @Cherche where eudopartid = @Eudoid
			

			SET @COMPT = 1 + @COMPT
			SET @Pos = @pos + Len(@Newv) + 1
		END

		FETCH Curs
		INTO @Eudoid
			,@Content
	END
	ELSE
	BEGIN
		FETCH Curs
		INTO @Eudoid
			,@Content
	END
END

SELECT @Compt

CLOSE Curs

DEALLOCATE Curs
