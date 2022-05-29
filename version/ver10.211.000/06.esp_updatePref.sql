 

/* 
*	Création des préférences d'un nouvel utilisteur 
*	Table CONFIG, PREF, SELECTIONS 
*	HLA - 04/11/09 
*	exec [dbo].[esp_creaPref] 202, 113 
*	KHA - 23/03/2011 
*	dérivée en [esp_updatePref] pour le bug 15087 : 
*	l'option reprendre le profil de n'est pas prise en compte lors de la mise à jour d'un contact 
*/ 
 
ALTER PROCEDURE [dbo].[esp_updatePref] 
	@nUserId numeric,				-- UserId du nouveau utilisateur 
	@nProfilUserId numeric			-- UserId de l'utilisateur dont il faut reprendre le profil 
AS  
 
declare @sUserId as varchar(50) 
declare @sGroupId as varchar(50) 
declare @sProfilUserId as varchar(50) 
 
declare @sField as varchar(max) 
declare @sFrom as varchar(max) 
declare @sDefaultOwnerField as varchar(max) 
declare @sWhere as varchar(max) 
 
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
	select @sField = ',' + dbo.[getConfigField]('Ref')  
END 
 
exec('UPDATE [CONFIG] set [USERID] = '+ @sUserId + @sField +  
' FROM [CONFIG] INNER JOIN [CONFIG] REF ON REF.[USERID] = ' + @sProfilUserId + ' AND [CONFIG].[USERID] = '+ @sUserId) 
 
 
/********/ 
/* PREF */ 
/********/ 
set @sField = '' 
set @sFrom = '' 
set @sWhere = '' 
IF IsNull(@nProfilUserId,0) <> 0 
BEGIN 
	select @sField = ',' + dbo.[getPrefField]('Ref') 
	set @sWhere = 'WHERE [UserId] = ' + @sProfilUserId 
END 
 
set @sDefaultOwnerField = '(SELECT CASE WHEN MAX(CAST(IsNull(case when [DefaultOwner] NOT LIKE ''%[^0-9]%'' then 0 else [DefaultOwner] end,0) AS NUMERIC)) > 0 THEN ' + @sUserId + ' ELSE 0 END FROM [Pref] SubPref WHERE SubPref.[Tab] = [Pref].[Tab])' 
exec('UPDATE [Pref] SET [DefaultOwner] = ' + @sDefaultOwnerField + ',[CalendarTodayOnLogin] = 1' + @sField   
	+ ' FROM [Pref] INNER JOIN  [Pref] Ref on Pref.[Tab] = Ref.[Tab] AND Pref.[UserId] = ' + @sUserId + ' AND Ref.[Userid] = '+@sProfilUserId) 
 
 


 
/*******************/
/* COLSPREF		   */
/*******************/

 -- Créatîon des pref manquante
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



UPDATE COLSPREF 
	SET 
		COL = REF.col,
		colwidth = REF.colwidth 
		
FROM COLSPREF
INNER JOIN COLSPREF REF ON 
		COLSPREF.tab = REF.tab 
	AND COLSPREF.bkmtab = REF.bkmtab
	AND COLSPREF.colspreftype = REF.colspreftype
	AND REF.userid = ISNULL(@nProfilUserId , 0)



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
