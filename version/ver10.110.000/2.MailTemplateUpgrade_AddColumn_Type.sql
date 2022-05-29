/***********************************************************************************************************
MAB le 14/08/2015

Ajout d'une colonne [Type] sur la table [MAILTEMPLATE] permettant de distinguer le type de modèle de mail
	0 : Modèle d'e-mailing
	1 : Modèle de mail unitaire
	...
***********************************************************************************************************/

IF NOT EXISTS(
	SELECT OBJ.* FROM SYSOBJECTS OBJ
	INNER JOIN sys.columns COL on COL.object_id = OBJ.id
	WHERE OBJECTPROPERTY(OBJ.id, N'IsUserTable') = 1
	AND OBJ.id = object_id(N'[dbo].[MAILTEMPLATE]')
	AND COL.name = 'Type'
)
BEGIN
	ALTER TABLE [dbo].[MAILTEMPLATE] ADD [Type] [smallint] NULL DEFAULT 0
END
