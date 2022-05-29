
/***********************************************************************************************************
KHA le 15/07/2014
Procédure permettant de créer un signet web
@specType : 
	1 : liens favoris
	3 : Appel depuis une formule du bas
	4 : EudoPart
	5 : Champ/signet de type page web
	6 : depuis un rapport spécifique
	7 : Champ de type lien
	8 : Administration V7
	9 : Menu droit depuis une fiche
	10: Menu droit depuis une liste
	
@openMode : 
	1 : ModalDialog
	2 : cachée
	3 : Iframe (champ/Signet de type page web, EudoPart)
	4 : nouvelle fenêtre 
	
@tab : descid de la table à laquelle est rattachée la spécif
@url : chemin relatif de la specif appelée 
@urlparam : peut contenir par exemple les dimensions de la modaldialog ("h=450&w=500")
***********************************************************************************************************/

ALTER PROC [dbo].[xsp_AddSpecif] @specType As Numeric, 
						@openMode As Numeric, 
						@tab As Numeric, 
						@label  as Varchar (255), 
						@Url as Varchar(255), 
						@urlParam as Varchar(255),
						@specId As Numeric out
						
AS
BEGIN
INSERT INTO SPECIFS (SpecifType
				   , OpenMode
				   , Source
				   , Tab
				   , Label
				   , URL
				   , URLParam)
SELECT @specType
	 , @openMode
	 , 2
	 , @tab
	 , @label
	 , @Url
	 , @urlParam
SET @specId = SCOPE_IDENTITY()
	
	-- Lien favoris
	IF @specType = 1 
	BEGIN
		INSERT INTO HOMEPAGE (Type
							, Libelle
							, Value)
		SELECT 6
			 , @label
			 , cast(@specId AS VARCHAR(255))
	END
	
	--Rapports spécifiques
	IF @specType = 6 
	BEGIN
		INSERT INTO REPORT (UserId
						  , Type
						  , Libelle
						  , Param
						  , Tab
						  , DateLastModified)
		SELECT NULL
			 , 4
			 , @label
			 , cast(@specId AS VARCHAR(255))
			 , @tab
			 , getdate()
	END


	-- EUDOPART
	IF @specType = 4 
	BEGIN
		INSERT INTO EUDOPART (EudoPartTitle
							, EudoPartContentType
							, EudoPartContent
							, EudoPartIcon)
		SELECT @label
			 , 8
			 , cast(@specId AS VARCHAR(255))
			 , ''
	END

END