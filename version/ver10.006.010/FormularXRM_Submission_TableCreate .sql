/* ************************************************************
 *   MOU 26-06-2014 
 *   Table qui enrigistre les actions de l'utilisateur sur un formulaire
 *   cf. 24897
 ************************************************************************* */

IF NOT EXISTS(SELECT * FROM SYSOBJECTS where id = object_id(N'[dbo].[FORMULARXRM_SUBMISSION]') and OBJECTPROPERTY(id, N'IsUserTable') = 1 )
BEGIN
CREATE TABLE [dbo].[FORMULARXRM_SUBMISSION](

	[Id] [NUMERIC](18, 0)  IDENTITY(1,1) NOT NULL,
	
	-- La reference au formulaire
	[FormularId]  [NUMERIC](18, 0) NOT NULL,
	
	-- la Personne invitée ou la personne ciblée
	[TplId] [NUMERIC](18, 0) NOT NULL,
	
	-- Requete d'update (Pour le debug)
	[SubmitQuery] [TEXT] NULL,
	
	-- Date de soumission
	[SubmitDate] [datetime] NOT NULL,
	
	-- L'ip de la machine 
	[SubmitIP] [VARCHAR](30) NULL,
	
	-- L'utilisateur qui a soummis le formulaire 
	[UserId] [NUMERIC](18, 0) NULL
 
) ON [PRIMARY] 

ALTER TABLE [dbo].[FORMULARXRM_SUBMISSION] WITH NOCHECK ADD CONSTRAINT [PK_FormularXrm_Submission] PRIMARY KEY NONCLUSTERED ( [Id] ) ON [PRIMARY]
END
