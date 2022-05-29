IF NOT EXISTS(SELECT * FROM SYSOBJECTS where id = object_id(N'[dbo].[RGPDTabRuleParam]') and OBJECTPROPERTY(id, N'IsUserTable') = 1 )
BEGIN
	-- Création de la table RGPDTabRuleParam si elle n'existe pas
	
	CREATE TABLE [dbo].[RGPDTabRuleParam] (
			[Id] [int] IDENTITY(1,1) NOT NULL
			,[DescId] [numeric](18,0) NOT NULL			
			,[RGPDType] [int] NULL
			,[Active] [bit] NULL
			,[NbMonthsDeadline] [int] NULL
			,CONSTRAINT [PK_RGPDTabPref] PRIMARY KEY CLUSTERED ([Id] ASC) WITH (
					PAD_INDEX = OFF
					,STATISTICS_NORECOMPUTE = OFF
					,IGNORE_DUP_KEY = OFF
					,ALLOW_ROW_LOCKS = ON
					,ALLOW_PAGE_LOCKS = ON
				) ON [PRIMARY]
		)
	
	ALTER TABLE [dbo].[RGPDTabRuleParam] WITH CHECK ADD CONSTRAINT [FK_RGPDTabPref_DESC] FOREIGN KEY([DescId]) REFERENCES [dbo].[DESC] ([DescId]) ON DELETE CASCADE
	ALTER TABLE [dbo].[RGPDTabRuleParam] CHECK CONSTRAINT [FK_RGPDTabPref_DESC]
END
	