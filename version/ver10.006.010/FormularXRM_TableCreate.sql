IF NOT EXISTS(SELECT * FROM SYSOBJECTS where id = object_id(N'[dbo].[FORMULARXRM]') and OBJECTPROPERTY(id, N'IsUserTable') = 1 )

/* ************************************************************
 *   MOU 26-06-2014 
 *   Table qui enrigistre les formulaires
 *   cf. 24897
 ************************************************************************* */
BEGIN
	CREATE TABLE [dbo].[FORMULARXRM] (
		[FormularXrmId] [numeric](18, 0)  IDENTITY(1,1) NOT NULL,
		
		-- Le nom donné au formulaire
		[Label] [VARCHAR](255) NULL,
		
		--Date limite et le type de soummission unique ou pas
		[ExpirationDate] [DATETIME] NULL,
		[IsUniqueSubmit] [BIT] NULL,		
		
		-- Les permissions
		[ViewPermId] [NUMERIC](18, 0)  NULL,
		[UpdatePermId] [NUMERIC](18, 0) NULL,
		
		-- Descid de ++/cible etendu et l'utilisateur
		[Tab] [numeric](18, 0)  NULL,		
		
		-- Le corps de formulaire et les styles
		[Body] [TEXT] NULL,
		[BodyCss]  [VARCHAR](MAX) NULL,
			
		-- Corps de message après soumission - 
		[SubmissionBody] [TEXT] NULL,
		[SubmissionBodyCss] [VARCHAR](MAX) NULL,
		
		-- Url de redirection
		[SubmissionRedirectUrl] [VARCHAR](300) NULL,
		
		-- Messages à afficher lorsque le formulaire a expiré ou a déjà été soumis
		[LabelExpired] [VARCHAR](600) NULL,				
		[LabelAlreadySubmitted] [VARCHAR](600) NULL,
		
		-- CHAMPS SYSTEME 		
		[CreatedOn] [DATETIME] NULL,
		[CreatedBy] [NUMERIC](18, 0) NOT NULL,
		[ModifiedOn] [DATETIME] NULL,
		[ModifiedBy] [NUMERIC](18, 0) NOT NULL,
		[UserId] [NUMERIC](18, 0) NOT NULL
		
	) ON [PRIMARY]
	
	ALTER TABLE [dbo].[FORMULARXRM] WITH NOCHECK ADD CONSTRAINT [PK_FormularXrm] PRIMARY KEY NONCLUSTERED ( [FormularXrmId] ) ON [PRIMARY]
END
