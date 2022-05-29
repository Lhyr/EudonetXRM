/***********************************************************************************************************
CRU : Reprise d'un script de la 10.110 pour ré-injecter les modèles manquants
***********************************************************************************************************/

--ETAPE 1 - Mise à jour éventuelle de la colonne Type précédemment créée sur tous les modèles XRM existants en version antérieure
UPDATE [MAILTEMPLATE] SET [Type] = 0 WHERE [Type] IS NULL

--ETAPE 2 - Importation des modèles de mails unitaires v7
-- Ouverture du curseur et récupération de tous les templates liés à Contacts et Adresses
Declare @idtab As Numeric
Declare Curs2 Cursor For
	-- SOUS-ETAPE 1 - Récupération des 
	SELECT DescId FROM [DESC] WHERE  (TYPE = 3 OR [Type] = 2) AND Descid IN (select tab from [PREF] where AdrJoin = 1)
	Open Curs2
	Fetch Curs2 Into @idtab
	While @@Fetch_status = 0
		Begin
			DECLARE @VALEURTPL as NUMERIC = @idtab 

			------------------------------------------

			-- SOUS-ETAPE 2 : Récupération de l'id du premier utilisateur Administrateur de la base, et ressources de langue

			DECLARE @VALEUREVT as NUMERIC  = (select intereventnum+10 from [desc] where descid = @VALEURTPL)  
			DECLARE @IDLOG as Numeric = (select top 1 userid from [USER] where UserLogin like '%ADMIN%')
			if ISNUMERIC(@IDLOG)=0
			begin
				set @IDLOG = (select top 1 userid from [USER] where UserLevel = 99)
			end
			DECLARE @AdminLang AS VARCHAR(10) = (SELECT Lang from [USER] where UserId = @IDLOG)

			DECLARE @WarningReplaceLink AS VARCHAR(30) = 'LIEN A REMPLACER'
			DECLARE @WarningReplaceField AS VARCHAR(30) = 'CHAMP A REMPLACER'
			DECLARE @WarningCheckTemplate AS VARCHAR(30) = 'MODELE A VERIFIER'
			DECLARE @WarningConvertTemplate AS VARCHAR(30) = 'MODELE A CONVERTIR'
			IF @AdminLang <> 'LANG_00'
			BEGIN
				SET @WarningReplaceLink = 'LINK NEEDS REPLACEMENT'
				SET @WarningReplaceField = 'FIELD NEEDS REPLACEMENT'
				SET @WarningCheckTemplate = 'TEMPLATE NEEDS TO BE CHECKED'
				SET @WarningConvertTemplate = 'TEMPLATE AWAITING CONVERSION'
			END

			INSERT INTO [dbo].[MAILTEMPLATE]
					(
					[Type]
					,[Label]
					,[Subject]
					,[Body]
					,[Body_HTML]
					,[BodyCSS]
					,[Histo]
					,[Tab]
					,[CreatedOn]
					,[CreatedBy]
					)
				SELECT
				NULL -- Valeur temporaire indiquant que le modèle est issu de la v7 et doit être converti
				,[Value]
				,[Value]
				,[Memo]
				,[MemoFormat]
				,[MemoCss]
				,0
				,@VALEURTPL -- table de destination (LA TABLE DES DESTINATAIRES, là où on voit le modele)
				,getdate()
				,@IDLOG  --id du compte admin
			FROM [dbo].[CATALOG]
			WHERE DescId IN (SELECT left(descid,len(descid)-1)+'7' FROM [DESC] where [desc].type=3)
				-- Pas d'import de modèles v7 précédemment importés (comportant le même nom pour le même TabId) et non traités (NULL) ou traités (1)
			AND [Value] NOT IN (
				SELECT [Subject] FROM [MAILTEMPLATE]
				WHERE [Type] IS NULL OR [TYPE] = 1
				AND [Tab] = @VALEURTPL
			)
			-- REMARQUES :

			-- On récupère les modèles de mails sur les tables de type E-mail.
			-- Requête : select type,* from [desc] where [type]=3

			-- Le champ qui contient les modèles de mails unitaires en v7 est toujours le descid + 07 (ici, en exemple, le DescID de la table cible est 3000)
			-- select * from CATALOG where descid=3007  

			------------------------------------------

			-- SOUS-ETAPE 3 : Mise à jour du corps de mail

			DECLARE @BODY AS VARCHAR(MAX)
			DECLARE @DESCID AS VARCHAR(MAX)
			DECLARE @BLOCFIN AS VARCHAR(MAX)
			DECLARE @POSITION AS NUMERIC
			DECLARE @INITIALMERGEFIELDCOUNT AS NUMERIC
			DECLARE @CURRENTMERGEFIELDCOUNT AS NUMERIC
			DECLARE @GOODMERGEFIELD AS BIT
			DECLARE @trackstatpos AS NUMERIC
			DECLARE @position2 as NUMERIC
			DECLARE @POS3 as NUMERIC
			DECLARE @LEN AS NUMERIC
			DECLARE @LEN2 AS NUMERIC
			DECLARE @NFILEID AS NUMERIC
			DECLARE @COMPT AS NUMERIC
			DECLARE @EDNPOS AS NUMERIC = 0
			DECLARE @VAR1 as VARCHAR(MAX)
			DECLARE @REPLACE_TXT AS VARCHAR(MAX) = '<span style="color:red;">' + @WarningReplaceLink + ' -></span>'
			DECLARE @REPLACE_CHAMP AS VARCHAR(MAX) = '<span style="color:red;">' + @WarningReplaceField + ' -></span>'
			DECLARE @TrackingURL AS VARCHAR(50) = '.eudonet.com/v7/app/TrackStat'
			DECLARE @TrackingURLRoot AS VARCHAR(25) = '<a href="https://wwX'
			DECLARE @LAB as Varchar(MAX)
			DECLARE CURS CURSOR
				FOR
					SELECT MAILTEMPLATEid
					FROM MAILTEMPLATE WHERE [Type] IS NULL -- Type IS NULL : Sélection des modèles précédemment importés uniquement
						OPEN CURS
						FETCH CURS
						INTO @NFILEID
						SET @COMPT=0
						WHILE @@FETCH_STATUS = 0
							BEGIN
								SELECT @BODY=[BODY] FROM MAILTEMPLATE WHERE MAILTEMPLATEID=@NFILEID
								SET @INITIALMERGEFIELDCOUNT = (LEN(@body)-LEN(REPLACE(@body,'ednfielddescid="','')))/LEN('ednfielddescid="')
								SET @CURRENTMERGEFIELDCOUNT = 0
								WHILE (CHARINDEX('ednfielddescid="', @body)) <> 0 AND @CURRENTMERGEFIELDCOUNT < @INITIALMERGEFIELDCOUNT
									BEGIN
										set @CURRENTMERGEFIELDCOUNT = 1 + @CURRENTMERGEFIELDCOUNT
										--select cast(@CURRENTMERGEFIELDCOUNT as varchar) + ' / ' + cast(@INITIALMERGEFIELDCOUNT as varchar) + ' | ' + cast(@NFILEID as varchar)
										set @position = CHARINDEX('ednfielddescid="', @body)
										set @DescID = SUBSTRING(@body,@position+16,10)
										set @Len= CHARINDEX('"',@DescID)
										set @DescID = case when @len>=1 then SUBSTRING(@DescID,1,@Len-1) else '' end
										set @len2 = CHARINDEX('id="'+@DescID+'_', @body)
										IF (@LEN2 > 1)
										BEGIN
											set @BlocFin = SUBSTRING(@body,@len2,18+@len)
										END
																	  
										DECLARE @LabelWarning AS VARCHAR(MAX)
										SET @LabelWarning = ''
										SET @GOODMERGEFIELD = ISNUMERIC(@DESCID)
										IF @GOODMERGEFIELD = 1
										BEGIN
											set @DESCID = CONVERT(numeric,@DESCID)
											IF not ((@DescID between @VALEURTPL and @VALEURTPL+99)
												or (@DescID between @VALEUREVT*100 and @VALEUREVT*100+99)
												or (@DescID between 200 and 499) )
											BEGIN
												SET @GOODMERGEFIELD = 0
												SET @LabelWarning = ' - ' + @WarningCheckTemplate
											END
										END
										ELSE
											BEGIN
												SET @LabelWarning = ' - ' + @WarningConvertTemplate
											END

										IF @GOODMERGEFIELD <> 1
										BEGIN
											set @BODY = SUBSTRING(@body,0,@position-50) + @REPLACE_CHAMP + SUBSTRING(@body,@position-50,LEN(@body))
											if @EDNPOS <> 1
												begin
												set @LAB = (select Label from MAILTEMPLATE where MAILTEMPLATEid = @NFILEID)
												--select @LAB
												update MAILTEMPLATE set Label = (select Label + @LabelWarning from MAILTEMPLATE where MailTemplateId = @NFILEID)
												where MailTemplateId = @NFILEID
												set @LAB = (select Label from MAILTEMPLATE where MAILTEMPLATEid = @NFILEID)
												set @EDNPOS = 1
												end
										--select @LAB
																		
										end

									set @DESCID = CONVERT(varchar(max),@DESCID)
									set @body = replace(@body,@BlocFin,'')
									set @body = REPLACE(@body,'ednfielddescid="'+@descid+'"','ednd="'+@descid+'"')
										set @body = REPLACE(@body,'ednfieldname="','ednc="')
										set @body = REPLACE(@body,'class="MergeField""','')
										set @body = REPLACE(@body,'ednfieldtype="mergefield"','')
										--select 
										--     @position as position,
										--     @descID as descid,
										--     @len as long,
										--     @len2 as long2,
										--     @blocfin as blocfin,
										--     @body as body
									END

								SET @trackstatpos = -1

								-- Correction des liens de tracking

								WHILE (CHARINDEX(@TrackingURL,@body,@trackstatpos + 1)) <> 0
									BEGIN
										--select @trackstatpos
										set @trackstatpos = CHARINDEX(@TrackingURL,@body,@trackstatpos + 1)
										--select @trackstatpos
										set @position2 = @trackstatpos - LEN(@TrackingURLRoot) -- reculer depuis |.eudonet.com/v7/app/TrackStat pour se positionner avant <a : <a href="https://wwX|.eudonet.com/v7/app/TrackStat
										--select @position2
										set @body = substring(@body, 0, @position2) + @REPLACE_TXT + substring(@body, @position2, LEN(@body)) 
										set @trackstatpos = @trackstatpos + LEN(@REPLACE_TXT) + 1
										if @EDNPOS <> 1
												begin
												set @LAB = (select Label from MAILTEMPLATE where MAILTEMPLATEid = @NFILEID)
												--select @LAB
												update MAILTEMPLATE set Label = (select Label + ' - ' + @WarningCheckTemplate from MAILTEMPLATE where MailTemplateId = @NFILEID)
												where MailTemplateId = @NFILEID
												set @LAB = (select Label from MAILTEMPLATE where MAILTEMPLATEid = @NFILEID)
												set @EDNPOS = 1
												end
										--select @trackstatpos
									END
						
								set @body = REPLACE(@body,'https://www.eudonet.com/v7/datas/','https://xrm.eudonet.com/xrm/datas/')
								set @body = REPLACE(@body,'https://ww1.eudonet.com/v7/datas/','https://xrm.eudonet.com/xrm/datas/')
								set @body = REPLACE(@body,'https://ww2.eudonet.com/v7/datas/','https://xrm.eudonet.com/xrm/datas/')
								set @body = REPLACE(@body,'https://ww4.eudonet.com/v7/datas/','https://xrm.eudonet.com/xrm/datas/')
								set @body = REPLACE(@body,'https://ww5.eudonet.com/v7/datas/','https://xrm.eudonet.com/xrm/datas/')
								set @body = REPLACE(@body,'https://www.eudonet.com:443/v7/datas/','https://xrm.eudonet.com/xrm/datas/')
								set @body = REPLACE(@body,'https://ww1.eudonet.com:443/v7/datas/','https://xrm.eudonet.com/xrm/datas/')
								set @body = REPLACE(@body,'https://ww2.eudonet.com:443/v7/datas/','https://xrm.eudonet.com/xrm/datas/')
								set @body = REPLACE(@body,'https://ww4.eudonet.com:443/v7/datas/','https://xrm.eudonet.com/xrm/datas/')
								set @body = REPLACE(@body,'https://ww5.eudonet.com:443/v7/datas/','https://xrm.eudonet.com/xrm/datas/')
								set @body = REPLACE(@body,'http://www.eudonet.com/v7/datas/','https://xrm.eudonet.com/xrm/datas/')
								set @body = REPLACE(@body,'http://ww1.eudonet.com/v7/datas/','https://xrm.eudonet.com/xrm/datas/')
								set @body = REPLACE(@body,'http://ww2.eudonet.com/v7/datas/','https://xrm.eudonet.com/xrm/datas/')
								set @body = REPLACE(@body,'http://ww4.eudonet.com/v7/datas/','https://xrm.eudonet.com/xrm/datas/')
								set @body = REPLACE(@body,'http://ww5.eudonet.com/v7/datas/','https://xrm.eudonet.com/xrm/datas/')
								set @body = REPLACE(@body,'http://www.eudonet.com:443/v7/datas/','https://xrm.eudonet.com/xrm/datas/')
								set @body = REPLACE(@body,'http://ww1.eudonet.com:443/v7/datas/','https://xrm.eudonet.com/xrm/datas/')
								set @body = REPLACE(@body,'http://ww2.eudonet.com:443/v7/datas/','https://xrm.eudonet.com/xrm/datas/')
								set @body = REPLACE(@body,'http://ww4.eudonet.com:443/v7/datas/','https://xrm.eudonet.com/xrm/datas/')
								set @body = REPLACE(@body,'http://ww5.eudonet.com:443/v7/datas/','https://xrm.eudonet.com/xrm/datas/')

								Update MAILTEMPLATE set body=@body, [Type] = 1 where MailTemplateId=@NFILEID -- Type 1 : modèle de mail unitaire au format XRM
												  
								-- On double le modèle, pour le type 0 (emailing)
								INSERT INTO [dbo].[MAILTEMPLATE]
									(
									[Type]
									,[Label]
									,[Subject]
									,[Body]
									,[Body_HTML]
									,[BodyCSS]
									,[Histo]
									,[Tab]
									,[CreatedOn]
									,[CreatedBy]
									)
								SELECT
									0
									,[Label]
									,[Subject]
									,[Body]
									,[Body_HTML]
									,[BodyCSS]
									,[Histo]
									,[Tab]
									,[CreatedOn]
									,[CreatedBy] FROM MAILTEMPLATE MailTpl WHERE MAILTEMPLATEID = @NFILEID
									AND MailTpl.[Subject] NOT IN (
										SELECT [Subject] FROM [MAILTEMPLATE] 
										WHERE [TYPE] = 0
										AND [Tab] = MailTpl.Tab
										AND Body_HTML = MailTpl.Body_HTML)
												  
								set @COMPT = 1+@COMPT 
								--select @COMPT
								set @EDNPOS = 0
							FETCH CURS
							INTO @NFILEID
							END
						--SELECT @COMPT
						CLOSE CURS
			DEALLOCATE CURS

			Fetch Curs2 Into @idtab
		End -- While @@Fetch_status = 0
	Close Curs2 --Open Curs2
	
Deallocate Curs2

-- ETAPE 3 : Création des modèles qui n'auraient pas été dupliqués à la migration
INSERT INTO MAILTEMPLATE ([Type], [Label], [Subject], [Body], [Body_HTML], [BodyCSS], [Histo], [Tab], [CreatedOn], [CreatedBy])
SELECT 0, [Value], [Value], [Memo], [MemoFormat], [MemoCss], MailTpl.Histo, MailTpl.Tab, MailTpl.CreatedOn, MailTpl.CreatedBy
FROM [dbo].[CATALOG]
INNER JOIN MAILTEMPLATE MailTpl ON MailTpl.[Subject] = [CATALOG].Value
	AND MailTpl.[Type] = 1
	AND MailTpl.Body_HTML = [CATALOG].MemoFormat
WHERE DescId IN (
		SELECT left(descid, len(descid) - 1) + '7'
		FROM [DESC]
		WHERE [desc].type = 3
		)
	AND MailTpl.[Subject] NOT IN (
		SELECT [Subject]
		FROM [MAILTEMPLATE]
		WHERE [Type] = 0
			AND [Tab] = MailTpl.Tab
			AND Body_HTML = MailTpl.Body_HTML
		)

