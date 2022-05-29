/*
*	Création des préférences d'un nouveau utilisteur
*	Table CONFIG, PREF, SELECTIONS
*	HLA - Creation le 04/11/09
*	HLA - Modif le 14/09/11
*   SPH - Modif le 04/11/2016 pour colspref
*	exec [dbo].[esp_creaPref] 223, 113
*/

ALTER PROCEDURE [dbo].[esp_creaPref]
	@nUserId numeric,				-- UserId du nouveau utilisateur
	@nProfilUserId numeric			-- UserId de l'utilisateur dont il faut reprendre le profil
AS 

declare @sUserId as varchar(50)
declare @sGroupId as varchar(50)
declare @sProfilUserId as varchar(50)

declare @sField as varchar(max)
declare @sInsertField as varchar(max)
declare @sSelectField as varchar(max)
declare @sFrom as varchar(max)
declare @sDefaultOwnerField as varchar(max)
declare @sWhere as varchar(max)
declare @sLeftOn as varchar(max)

set @sUserId = cast(@nUserId as varchar(50))
set @sProfilUserId = cast(@nProfilUserId as varchar(50))
select @sGroupId = cast([groupid] as varchar(50)) from [user] where userid = @nUserId


/**********/
/* CONFIG */
/**********/
set @sField = ''
set @sFrom = ''
set @sWhere = ''
IF IsNull(@nProfilUserId,0) <> 0
BEGIN
	select @sField = ',' + dbo.[getConfigField](null)
	set @sFrom = ' FROM [CONFIG]'
	set @sWhere = ' WHERE [UserId] = ' + @sProfilUserId
END

exec('IF NOT EXISTS (select 1 from [CONFIG] where userid = ' + @sUserId + ')'
	+ ' INSERT INTO [Config] ([UserId]' + @sField + ') SELECT ' + @sUserId + @sField + @sFrom + @sWhere)


/********/
/* PREF */
/********/
set @sField = ''
set @sFrom = ''
set @sWhere = ''
IF IsNull(@nProfilUserId,0) <> 0
BEGIN
	select @sField = ',' + dbo.[getPrefField](null)
	set @sWhere = ' AND [UserId] = ' + @sProfilUserId
END

set @sDefaultOwnerField = '(SELECT CASE WHEN MAX(CAST(IsNull(case when [DefaultOwner] NOT LIKE ''%[^0-9]%'' then [DefaultOwner] else 0 end,0) AS NUMERIC)) > 0 THEN ' + @sUserId + ' ELSE 0 END FROM [Pref] SubPref WHERE SubPref.[Tab] = [Pref].[Tab])'
exec('INSERT INTO [Pref] ([Tab],[UserId],[DefaultOwner],[CalendarTodayOnLogin]' + @sField + ')'
	+ ' SELECT DISTINCT [Tab],' + @sUserId + ',' + @sDefaultOwnerField + ',1' + @sField
	+ ' FROM [Pref]'
	+ ' WHERE not exists (select prefid from [Pref] as VERIF where VERIF.tab = [Pref].tab and VERIF.userid = ' + @sUserId + ')' + @sWhere)



/*******************/
/* COLSPREF		   */
/*******************/
INSERT INTO COLSPREF ( [USERID], [TAB], [BKMTAB], [colspreftype],  [col], [colwidth])
SELECT DISTINCT @sUserId, CP.[TAB], CP.[BKMTAB], CP.[COLSPREFTYPE], CP.[COL], CP.[COLWIDTH]
FROM [COLSPREF] CP  WHERE CP.[userid] = ISNULL(@nProfilUserId,0)
AND NOT EXISTS (
	SELECT 1 FROM [COLSPREF] CPU 
	WHERE CPU.userid = @sUserId 
		AND CP.tab = CPU.tab 
		AND CP.bkmtab = CPU.bkmtab
		AND CP.colspreftype = CPU.colspreftype
		)





/***********/
/* BKMPREF */
/***********/
set @sLeftOn = ''
set @sInsertField = '[bkmcol], [bkmcolwidth], [bkmsort], [bkmorder], [bkmhisto], [bkmfilterCol], [bkmfilterOp], [bkmfilterValue], [AddedBkmWhere]'
set @sSelectField = '[bkmpref].[bkmcol], [bkmpref].[bkmcolwidth], [bkmpref].[bkmsort], [bkmpref].[bkmorder], [bkmpref].[bkmhisto], [bkmpref].[bkmfilterCol], [bkmpref].[bkmfilterOp], [bkmpref].[bkmfilterValue], [bkmpref].[AddedBkmWhere]'
IF IsNull(@nProfilUserId,0) <> 0
BEGIN
	set @sLeftOn = '[bkmpref].[userid] = ' + @sProfilUserId
END
ELSE
BEGIN
	set @sLeftOn = '[bkmpref].[userid] = -1'
END

-- Uniquement les tables rattaché à PP ou lié depuis une rubrique avec liaison
exec('INSERT INTO BKMPREF ([userid], [bkm], [tab], ' + @sInsertField + ' )
select distinct [pref].[userid], [pref].[tab], 200, ' + @sSelectField + '
from [pref] inner join [desc] MAIN on MAIN.[DescId] = [pref].[tab]
	left join [bkmpref] on [bkmpref].[bkm] = [pref].[tab] and [bkmpref].[tab] = 200 and ' + @sLeftOn + '
where [pref].[tab] not in (2, 200, 500, 600) and [pref].[tab] < 100000 and (isnull(MAIN.[interpp], 0) <> 0 or [pref].[tab] = 400 or EXISTS (select descid from [DESC] where [file] = MAIN.[file] and [popup] = 2 and [PopupDescId] = 201 and [Relation] = 1))
	and not exists (select [bkmid] from [BKMPREF] where [BKMPREF].[userid] = [PREF].[userid] and [bkm] = [pref].[tab] and [tab] = 200)
	and [pref].[userid] = ' + @sUserId)

-- Uniquement les tables rattaché à PM ou lié depuis une rubrique avec liaison
exec('INSERT INTO BKMPREF ([userid], [bkm], [tab], ' + @sInsertField + ')
select distinct [pref].[userid], [pref].[tab], 300, ' + @sSelectField + '
from [pref] inner join [desc] MAIN on MAIN.[DescId] = [pref].[tab]
	left join [bkmpref] on [bkmpref].[bkm] = [pref].[tab] and [bkmpref].[tab] = 300 and ' + @sLeftOn + '
where [pref].[tab] not in (2, 300, 500, 600) and [pref].[tab] < 100000 and (isnull(MAIN.[interpm], 0) <> 0 or [pref].[tab] = 400 or EXISTS (select descid from [DESC] where [file] = MAIN.[file] and [popup] = 2 and [PopupDescId] = 301 and [Relation] = 1))
	and not exists (select [bkmid] from [BKMPREF] where [BKMPREF].[userid] = [PREF].[userid] and [bkm] = [pref].[tab] and [tab] = 300)
	and [pref].[userid] = ' + @sUserId)

-- DOUBLONS - Uniquement les FILE_MAIN (doublons diponible uniquement sur les tables de type FILE_MAIN)
exec('INSERT INTO BKMPREF ([userid], [bkm], [tab], ' + @sInsertField + ')
select distinct [pref].[userid], 2, [pref].[tab], ' + @sSelectField + '
from [pref] inner join [desc] MAIN on MAIN.[DescId] = [pref].[tab]
	left join [bkmpref] on [bkmpref].[bkm] = 2 and [bkmpref].[tab] = [pref].[tab] and ' + @sLeftOn + '
where [pref].[tab] not in (2, 500, 600) and [pref].[tab] < 100000 and [type] = 0
	and not exists (select [bkmid] from [BKMPREF] where [BKMPREF].[userid] = [PREF].[userid] and [bkm] = 2 and [tab] = [pref].[tab])
	and [pref].[userid] = ' + @sUserId)

-- HISTORIQUE PP - Uniquement les FILE_MAIN (historique diponible uniquement sur les tables de type FILE_MAIN)
exec('INSERT INTO BKMPREF ([userid], [bkm], [tab], ' + @sInsertField + ')
select distinct [pref].[userid], 100000, 200, ' + @sSelectField + '
from [pref] inner join [desc] MAIN on MAIN.[DescId] = [pref].[tab]
	left join [bkmpref] on [bkmpref].[bkm] = 100000 and [bkmpref].[tab] = 200 and ' + @sLeftOn + '
where [pref].[tab] = 100000
	and not exists (select [bkmid] from [BKMPREF] where [BKMPREF].[userid] = [PREF].[userid] and [bkm] = 100000 and [tab] = 200)
	and [pref].[userid] = ' + @sUserId)

-- HISTORIQUE PM - Uniquement les FILE_MAIN (historique diponible uniquement sur les tables de type FILE_MAIN)
exec('INSERT INTO BKMPREF ([userid], [bkm], [tab], ' + @sInsertField + ')
select distinct [pref].[userid], 100000, 300, ' + @sSelectField + '
from [pref] inner join [desc] MAIN on MAIN.[DescId] = [pref].[tab]
	left join [bkmpref] on [bkmpref].[bkm] = 100000 and [bkmpref].[tab] = 300 and ' + @sLeftOn + '
where [pref].[tab] = 100000
	and not exists (select [bkmid] from [BKMPREF] where [BKMPREF].[userid] = [PREF].[userid] and [bkm] = 100000 and [tab] = 300)
	and [pref].[userid] = ' + @sUserId)

-- HISTORIQUE OTHER - Uniquement les FILE_MAIN (historique diponible uniquement sur les tables de type FILE_MAIN)
exec('INSERT INTO BKMPREF ([userid], [bkm], [tab], ' + @sInsertField + ')
select distinct [pref].[userid], 100000, [pref].[tab], ' + @sSelectField + '
from [pref] inner join [desc] MAIN on MAIN.[DescId] = [pref].[tab]
	left join [bkmpref] on [bkmpref].[bkm] = 100000 and [bkmpref].[tab] = [pref].[tab] and ' + @sLeftOn + '
where [pref].[tab] not in (2, 200, 300, 500, 600) and [pref].[tab] < 100000 and [type] = 0
	and not exists (select [bkmid] from [BKMPREF] where [BKMPREF].[userid] = [PREF].[userid] and [bkm] = 100000 and [tab] = [pref].[tab])
	and [pref].[userid] = ' + @sUserId)

-- PJ - Uniquement les FILE_MAIN (pj diponible uniquement sur les tables de type FILE_MAIN)
exec('INSERT INTO BKMPREF ([userid], [bkm], [tab], ' + @sInsertField + ')
select distinct [pref].[userid], 102000, [pref].[tab], ' + @sSelectField + '
from [pref] inner join [desc] MAIN on MAIN.[DescId] = [pref].[tab]
	left join [bkmpref] on [bkmpref].[bkm] = 102000 and [bkmpref].[tab] = [pref].[tab] and ' + @sLeftOn + '
where [pref].[tab] not in (2, 500, 600) and [pref].[tab] < 100000
	and not exists (select [bkmid] from [BKMPREF] where [BKMPREF].[userid] = [PREF].[userid] and [bkm] = 102000 and [tab] = [pref].[tab])
	and [pref].[userid] = ' + @sUserId)

-- DESCID_BKM_PM_EVENT - Signet des affaires de la pm de l'affaire en cours
exec('INSERT INTO BKMPREF ([userid], [bkm], [tab], ' + @sInsertField + ')
select distinct [pref].[userid], [pref].[tab] + 87, [pref].[tab], ' + @sSelectField + '
from [pref] inner join [desc] MAIN on MAIN.[DescId] = [pref].[tab]
	left join [bkmpref] on [bkmpref].[bkm] = [pref].[tab] + 87 and [bkmpref].[tab] = [pref].[tab] and ' + @sLeftOn + '
where ([pref].[tab] = 100 or ([pref].[tab] >= 1000 and [pref].[tab] < 100000)) and [type] = 0
	and not exists (select [bkmid] from [BKMPREF] where [BKMPREF].[userid] = [PREF].[userid] and [bkm] = [pref].[tab] + 87 and [tab] = [pref].[tab])
	and [pref].[userid] = ' + @sUserId)

-- Signets LIAISON DIRECT
exec('INSERT INTO BKMPREF ([userid], [bkm], [tab], ' + @sInsertField + ')
select distinct [pref].[userid], BKM.[DescId], [PARENT].[descid], ' + @sSelectField + '
from [PREF] inner join [DESC] as BKM on [PREF].[tab] = BKM.[DescId]
	inner join [DESC] as PARENT on PARENT.DescId = (case when isnull(BKM.[InterEventNum],0) <> 0 then (BKM.[InterEventNum] + 10) * 100 else 100 end)
	left join [bkmpref] on [bkmpref].[bkm] = BKM.[DescId] and [bkmpref].[tab] = [PARENT].[descid] and ' + @sLeftOn + '
where BKM.[DescId] like ''%00'' and (BKM.[DescId] > 1000 or BKM.[DescId] = 100) and BKM.[InterEvent] = 1
	and not exists (select [bkmid] from [BKMPREF] where [BKMPREF].[userid] = [PREF].[userid] and [bkm] = BKM.[DescId] and [BKMPREF].[tab] = [PARENT].[descid])
	and [pref].[userid] = ' + @sUserId)

-- Signet LIAISON INDIRECT depuis "lié depuis"
exec('INSERT INTO BKMPREF ([userid], [bkm], [tab], ' + @sInsertField + ')
select distinct [pref].[userid], BKM.[DescId], [PARENT].[descid], ' + @sSelectField + '
from [PREF] inner join [DESC] as BKM on [PREF].[tab] = BKM.[DescId]
	inner join [DESC] as RUB on BKM.[DescId] = RUB.[DescId] - RUB.[DescId] % 100
	inner join [DESC] as PARENT on PARENT.[DescId] = RUB.[PopupDescId] - 1
	left join [bkmpref] on [bkmpref].[bkm] = BKM.[DescId] and [bkmpref].[tab] = [PARENT].[descid] and ' + @sLeftOn + '
where RUB.[Popup] = 2 and RUB.[Relation] = 1  and (PARENT.[DescId] > 1000 or PARENT.[DescId] = 100)
	and not exists (select [bkmid] from [BKMPREF] where [BKMPREF].[userid] = [PREF].[userid] and [bkm] = BKM.[DescId] and [BKMPREF].[tab] = [PARENT].[descid])
	and [pref].[userid] = ' + @sUserId)	


/**************/
/* SELECTIONS */
/**************/
set @sField = ''
set @sFrom = ''
set @sWhere = ''
IF IsNull(@nProfilUserId,0) <> 0
BEGIN
	select @sField = ',' + dbo.[getSelectionField]()
	set @sWhere = 'WHERE IsNull([UserList],'''') = '''' and [UserId] = ' + @sProfilUserId

	-- Temporairement on stocke le selectid du userprofil dans la colonne UserList
	-- pour infos, la colonne UserList est exclus du processus de recopie du profil utilisateur
	exec ('INSERT INTO [SELECTIONS] ([UserId],[UserList]' + @sField + ')'
		+ ' SELECT ' + @sUserId + ',[selectid]' + @sField
		+ ' FROM [SELECTIONS] ' + @sWhere)
END

set @sField = ''
set @sWhere = ''
IF IsNull(@sGroupId,'') <> ''
BEGIN
	/* SELECTIONS DE TYPE ONGLET DEFINIES POUR LE GROUP */
	INSERT INTO [SELECTIONS] ([TAB], [DEFAULTSELECTID], [USERID], [LABEL], [TABORDER]) 
		SELECT [TAB], [SELECTID], @nUserId, [LABEL], [TABORDER]
		FROM [SELECTIONS] 
		WHERE ';' + [UserList] + ';' like ';G' + @sGroupId + ';' AND [TAB] = 0
			AND NOT EXISTS (
				SELECT SEL.[SELECTID] FROM [SELECTIONS] SEL 
				WHERE SEL.[USERID] = @nUserId AND SEL.[TAB] = 0 AND SEL.[DEFAULTSELECTID] = [SELECTIONS].SELECTID
			)

	/* SELECTIONS DE TYPE LIST DEFINIES POUR LE GROUP */
	INSERT INTO [SELECTIONS] ([TAB], [DEFAULTSELECTID], [USERID], [LABEL], [LISTCOL], [LISTCOLWIDTH])
		SELECT [TAB], [SELECTID], @nUserId, [LABEL], [LISTCOL], [LISTCOLWIDTH]
		FROM [SELECTIONS]
		WHERE ';' + [UserList] + ';' like ';G' + @sGroupId + ';' AND [TAB] <> 0
			AND NOT EXISTS ( 
				SELECT SEL.[SELECTID] FROM [SELECTIONS] SEL
				WHERE SEL.[USERID] = @nUserId AND SEL.[TAB] = [SELECTIONS].[TAB] AND SEL.[DEFAULTSELECTID] = [SELECTIONS].SELECTID
			)
END

/* POINTE LES PREF DE L'UTILISATEUR SUR LES NOUVELLES SELECTIONS CRER A PARTIR DE L'UTILISATEUR PROFIL */
IF IsNull(@nProfilUserId,0) <> 0
BEGIN
	UPDATE [CONFIG]
		SET [CONFIG].[TABORDERID] = sChild.[SELECTID]
		FROM [CONFIG] cParent
				inner join [SELECTIONS] sParent on sParent.[SELECTID] = cParent.[TABORDERID]
				inner join [SELECTIONS] sChild on sChild.[UserList] = sParent.[SELECTID] 
				inner join [CONFIG] on [CONFIG].[USERID] = sChild.[USERID]
		WHERE cParent.userid = @nProfilUserId and [CONFIG].[USERID] = @nUserId and IsNull(cParent.[TABORDERID],0) <> 0

	UPDATE [PREF]
		SET [PREF].[SELECTID] = sChild.[SELECTID]
		FROM [PREF] pParent
				inner join [SELECTIONS] sParent on sParent.[SELECTID] = pParent.[SELECTID]
				inner join [SELECTIONS] sChild on sChild.[UserList] = sParent.[SELECTID] 
				inner join [PREF] on [PREF].[USERID] = sChild.[USERID]
		WHERE pParent.userid = @nProfilUserId and [PREF].[USERID] = @nUserId
				and pParent.[TAB] <> 0 and pParent.[TAB] = [PREF].[TAB] and IsNull(pParent.[SELECTID],0) <> 0

	UPDATE [SELECTIONS] SET [UserList] = null WHERE IsNull([UserList],'') <> '' and [USERID] = @nUserId
END

/* POINTE LES PREF DE L'UTILISATEUR SUR LA DERNIERE SELECTION IMPORT POUR SON GROUPE */
IF IsNull(@sGroupId,'') <> ''
BEGIN
	UPDATE [CONFIG]
		SET [TABORDERID] = [SELECTID]
		FROM [CONFIG] inner join [SELECTIONS] on [CONFIG].[USERID] = [SELECTIONS].[USERID]
		WHERE [CONFIG].[USERID] = @nUserId and [SELECTIONS].[TAB] = 0 and IsNull([DEFAULTSELECTID],0) <> 0 

	UPDATE [PREF]
		SET [SELECTID] = tab.id
		FROM [PREF] inner join (
				SELECT max([SELECTIONS].[SELECTID]) as id, tab, userid
				FROM [SELECTIONS]
				WHERE [SELECTIONS].[TAB] <> 0 and IsNull([DEFAULTSELECTID],0) <> 0 and [SELECTIONS].[USERID] = @nUserId
				GROUP BY [TAB],[USERID]
		) tab on tab.[USERID] = [PREF].[USERID] and tab.[TAB] = [PREF].[TAB]
END


/***************************************/
/* Recopie des droits d'un autre profil*/
/***************************************/
IF IsNull(@nProfilUserId,0) <> 0
BEGIN
	UPDATE [PERMISSION]
		SET [User] = Substring(Replace(';'+ [User] +';', ';'+ @sProfilUserId +';', ';'+ @sProfilUserId +';'+ @sUserId +';'), 2, Len(Replace(';'+ [User] +';', ';'+ @sProfilUserId +';', ';'+ @sProfilUserId +';'+ @sUserId +';')) - 2)
	WHERE ';'+[User]+';' LIKE '%;'+ @sProfilUserId +';%' AND ';'+[User]+';' NOT LIKE '%;'+ @sUserId +';%'
END
