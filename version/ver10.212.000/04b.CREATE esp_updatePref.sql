/*
*	CREATE PROCEDURE
*	BSE - 30/11/2016
*/
CREATE PROCEDURE [dbo].[esp_updatePref] @nUserId NUMERIC
	,-- UserId du nouveau utilisateur 
	@nProfilUserId NUMERIC -- UserId de l'utilisateur dont il faut reprendre le profil 
AS
DECLARE @sUserId AS VARCHAR(50)
DECLARE @sGroupId AS VARCHAR(50)
DECLARE @sProfilUserId AS VARCHAR(50)
DECLARE @sField AS VARCHAR(max)
DECLARE @sFrom AS VARCHAR(max)
DECLARE @sDefaultOwnerField AS VARCHAR(max)
DECLARE @sWhere AS VARCHAR(max)

SET @sUserId = cast(@nUserId AS VARCHAR(50))
SET @sProfilUserId = cast(@nProfilUserId AS VARCHAR(50))

SELECT @sGroupId = cast([groupid] AS VARCHAR(50))
FROM [user]
WHERE userid = @nUserId

/**********/
/* CONFIG */
/**********/
SET @sField = ''
SET @sFrom = ''
SET @sWhere = ''

IF IsNull(@nProfilUserId, 0) <> 0
BEGIN
	SELECT @sField = ',' + dbo.[getConfigField]('Ref')
END

EXEC ('UPDATE [CONFIG] set [USERID] = ' + @sUserId + @sField + ' FROM [CONFIG] INNER JOIN [CONFIG] REF ON REF.[USERID] = ' + @sProfilUserId + ' AND [CONFIG].[USERID] = ' + @sUserId)

/********/
/* PREF */
/********/
SET @sField = ''
SET @sFrom = ''
SET @sWhere = ''

IF IsNull(@nProfilUserId, 0) <> 0
BEGIN
	SELECT @sField = ',' + dbo.[getPrefField]('Ref')

	SET @sWhere = 'WHERE [UserId] = ' + @sProfilUserId
END

SET @sDefaultOwnerField = '(SELECT CASE WHEN MAX(CAST(IsNull(case when [DefaultOwner] NOT LIKE ''%[^0-9]%'' then [DefaultOwner] else 0 end,0) AS NUMERIC)) > 0 THEN ' + @sUserId + ' ELSE 0 END FROM [Pref] SubPref WHERE SubPref.[Tab] = [Pref].[Tab])'

EXEC ('UPDATE [Pref] SET [DefaultOwner] = ' + @sDefaultOwnerField + ',[CalendarTodayOnLogin] = 1' + @sField + ' FROM [Pref] INNER JOIN  [Pref] Ref on Pref.[Tab] = Ref.[Tab] AND Pref.[UserId] = ' + @sUserId + ' AND Ref.[Userid] = ' + @sProfilUserId)

PRINT ('UPDATE [Pref] SET [DefaultOwner] = ' + @sDefaultOwnerField + ',[CalendarTodayOnLogin] = 1' + @sField + ' FROM [Pref] INNER JOIN  [Pref] Ref on Pref.[Tab] = Ref.[Tab] AND Pref.[UserId] = ' + @sUserId + ' AND Ref.[Userid] = ' + @sProfilUserId)

/*******************/
/* COLSPREF		   */
/*******************/
-- Créatîon des pref manquante
INSERT INTO COLSPREF (
	[USERID]
	,[TAB]
	,[BKMTAB]
	,[colspreftype]
	,[col]
	,[colwidth]
	)
SELECT DISTINCT @sUserId
	,CP.[TAB]
	,CP.[BKMTAB]
	,CP.[COLSPREFTYPE]
	,CP.[COL]
	,CP.[COLWIDTH]
FROM [COLSPREF] CP
WHERE CP.[userid] = ISNULL(@nProfilUserId, 0)
	AND NOT EXISTS (
		SELECT 1
		FROM [COLSPREF] CPU
		WHERE CPU.userid = @sUserId
			AND CP.tab = CPU.tab
			AND CP.bkmtab = CPU.bkmtab
			AND CP.colspreftype = CPU.colspreftype
		)

UPDATE COLSPREF
SET COL = REF.col
	,colwidth = REF.colwidth
FROM COLSPREF
INNER JOIN COLSPREF REF ON COLSPREF.tab = REF.tab
	AND COLSPREF.bkmtab = REF.bkmtab
	AND COLSPREF.colspreftype = REF.colspreftype
	AND REF.userid = ISNULL(@nProfilUserId, 0)

/**************/
/* SELECTIONS */
/**************/
SET @sField = ''
SET @sFrom = ''
SET @sWhere = ''

IF IsNull(@nProfilUserId, 0) <> 0
BEGIN
	SELECT @sField = ',' + dbo.[getSelectionField]()

	SET @sWhere = 'WHERE IsNull([UserList],'''') = '''' and [UserId] = ' + @sProfilUserId

	-- Temporairement on stocke le selectid du userprofil dans la colonne UserList 
	-- pour infos, la colonne UserList est exclus du processus de recopie du profil utilisateur 
	EXEC ('INSERT INTO [SELECTIONS] ([UserId],[UserList]' + @sField + ')' + ' SELECT ' + @sUserId + ',[selectid]' + @sField + ' FROM [SELECTIONS] ' + @sWhere)
END

/* POINTE LES PREF DE L'UTILISATEUR SUR LES NOUVELLES SELECTIONS CRER A PARTIR DE L'UTILISATEUR PROFIL */
IF IsNull(@nProfilUserId, 0) <> 0
BEGIN
	UPDATE [CONFIG]
	SET [CONFIG].[TABORDERID] = sChild.[SELECTID]
	FROM [CONFIG] cParent
	INNER JOIN [SELECTIONS] sParent ON sParent.[SELECTID] = cParent.[TABORDERID]
	INNER JOIN [SELECTIONS] sChild ON sChild.[UserList] = sParent.[SELECTID]
	INNER JOIN [CONFIG] ON [CONFIG].[USERID] = sChild.[USERID]
	WHERE cParent.userid = @nProfilUserId
		AND [CONFIG].[USERID] = @nUserId
		AND IsNull(cParent.[TABORDERID], 0) <> 0

	UPDATE [PREF]
	SET [PREF].[SELECTID] = sChild.[SELECTID]
	FROM [PREF] pParent
	INNER JOIN [SELECTIONS] sParent ON sParent.[SELECTID] = pParent.[SELECTID]
	INNER JOIN [SELECTIONS] sChild ON sChild.[UserList] = sParent.[SELECTID]
	INNER JOIN [PREF] ON [PREF].[USERID] = sChild.[USERID]
	WHERE pParent.userid = @nProfilUserId
		AND [PREF].[USERID] = @nUserId
		AND pParent.[TAB] <> 0
		AND pParent.[TAB] = [PREF].[TAB]
		AND IsNull(pParent.[SELECTID], 0) <> 0

	UPDATE [SELECTIONS]
	SET [UserList] = NULL
	WHERE IsNull([UserList], '') <> ''
		AND [USERID] = @nUserId
END

/* POINTE LES PREF DE L'UTILISATEUR SUR LA DERNIERE SELECTION IMPORT POUR SON GROUPE */
IF IsNull(@sGroupId, '') <> ''
BEGIN
	UPDATE [CONFIG]
	SET [TABORDERID] = [SELECTID]
	FROM [CONFIG]
	INNER JOIN [SELECTIONS] ON [CONFIG].[USERID] = [SELECTIONS].[USERID]
	WHERE [CONFIG].[USERID] = @nUserId
		AND [SELECTIONS].[TAB] = 0
		AND IsNull([DEFAULTSELECTID], 0) <> 0

	UPDATE [PREF]
	SET [SELECTID] = tab.id
	FROM [PREF]
	INNER JOIN (
		SELECT max([SELECTIONS].[SELECTID]) AS id
			,tab
			,userid
		FROM [SELECTIONS]
		WHERE [SELECTIONS].[TAB] <> 0
			AND IsNull([DEFAULTSELECTID], 0) <> 0
			AND [SELECTIONS].[USERID] = @nUserId
		GROUP BY [TAB]
			,[USERID]
		) tab ON tab.[USERID] = [PREF].[USERID]
		AND tab.[TAB] = [PREF].[TAB]
END

/***************************************/
/* Recopie des droits d'un autre profil*/
/***************************************/
IF IsNull(@nProfilUserId, 0) <> 0
BEGIN
	UPDATE [PERMISSION]
	SET [User] = Substring(Replace(';' + [User] + ';', ';' + @sProfilUserId + ';', ';' + @sProfilUserId + ';' + @sUserId + ';'), 2, Len(Replace(';' + [User] + ';', ';' + @sProfilUserId + ';', ';' + @sProfilUserId + ';' + @sUserId + ';')) - 2)
	WHERE ';' + [User] + ';' LIKE '%;' + @sProfilUserId + ';%'
		AND ';' + [User] + ';' NOT LIKE '%;' + @sUserId + ';%'
END
