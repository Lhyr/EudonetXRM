IF NOT EXISTS(SELECT * FROM SYSOBJECTS where id = object_id(N'[dbo].[TOKEN]') and OBJECTPROPERTY(id, N'IsUserTable') = 1 )
BEGIN
 
	CREATE TABLE [dbo].[TOKEN] (
			--Champs Syst�mes
			[TOKENID]  int IDENTITY(1,1) NOT NULL,

			[KEY] VARCHAR(200) NOT NULL, --Cl� d'itentification

			[TYPE] INT NOT NULL, -- type de token
			[VALUE] NVARCHAR(max) NOT NULL, -- Information li�e au tokent
			[USERID] INT NOT NULL, -- Userid du token
			[EXPIRATIONDATE] DATETIME NOT NULL CONSTRAINT DF_EXPIRATIONDATE DEFAULT DATEADD(DAY, 1, GETDATE())  , -- Expiration
			[CREATIONDATE] DATETIME  NOT NULL CONSTRAINT DF_CREATIONDATE DEFAULT GETDATE(),	-- Cr�ation
			[DISABLED] BIT NOT NULL	 CONSTRAINT DF_DISABLED DEFAULT 0
			 
		) ON [PRIMARY]
	
	--Cl� primaire
	ALTER TABLE [dbo].[TOKEN] WITH NOCHECK ADD CONSTRAINT [PK_TOKEN] PRIMARY KEY CLUSTERED ( [TOKENID] ) ON [PRIMARY]
	 
 
	
		CREATE UNIQUE NONCLUSTERED INDEX [IX_KEY_TYPE] ON [dbo].[TOKEN]
		(
			[KEY] ASC,
			[TYPE] ASC
		)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)




END


