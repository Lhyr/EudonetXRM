DECLARE @idSubjectInClear INT = 106045
DECLARE @idCampaignPreHeader INT = 106047
DECLARE @bIsUnicode BIT = 0

IF EXISTS (
		SELECT *
		FROM [CONFIGADV]
		WHERE [Parameter] = 'FULL_UNICODE'
			AND [Value] = '1'
		)
BEGIN
	SET @bIsUnicode = 1
END

IF NOT EXISTS (
		SELECT 1
		FROM sys.tables
		WHERE name LIKE 'ORMSTATLOGS'
		)
BEGIN

	CREATE TABLE [dbo].[ORMSTATLOGS](
		[Id] [int] IDENTITY(1,1) NOT NULL,
		[Date] [datetime] NOT NULL,
		[Name] [varchar](200) NOT NULL,
		[AssemblyName] [varchar](200) NOT NULL,
		[Version] [varchar](15) NOT NULL,
	 CONSTRAINT [PK_ORMSTATLOGS] PRIMARY KEY CLUSTERED
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]
	 

	CREATE NONCLUSTERED INDEX [IX_NAMES] ON [dbo].[ORMSTATLOGS]
	(
		[Date] ASC,
		[Name] ASC,
		[AssemblyName] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
	 

	EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Nom du déclencheur' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ORMSTATLOGS', @level2type=N'COLUMN',@level2name=N'Name'
	 

	EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Nom de internal' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'ORMSTATLOGS', @level2type=N'COLUMN',@level2name=N'AssemblyName'
	
	
		IF @bIsUnicode = 1
		BEGIN
			ALTER TABLE [ORMSTATLOGS] ALTER COLUMN [Name] [nvarchar](200) 
			ALTER TABLE [ORMSTATLOGS] ALTER COLUMN [AssemblyName] [nvarchar](200) 
		END
END	 