IF NOT EXISTS(SELECT * FROM SYSOBJECTS where id = object_id(N'[dbo].[FILEMAP_EXCHANGE]') and OBJECTPROPERTY(id, N'IsUserTable') = 1 )
BEGIN
	-- Création de la table FILEMAP_EXCHANGE si elle n'existe pas
	
	CREATE TABLE [dbo].[FILEMAP_EXCHANGE] (
			 [FileMapId] [numeric](18, 0) IDENTITY (1, 1) NOT NULL,
                [Source] [numeric](18, 0) NULL,
                [SourceFileId] [varchar] (500)  NULL,
                [SecondarySourceFileId] [varchar] (500)  NULL,
                [EudonetFileId] [numeric](18, 0) NULL,
                [Tab] [numeric](18, 0) NULL,
                [UserId] [numeric](18, 0) NULL,
                [SourceFile] [varchar] (500) NULL,
                [AdrId] [numeric](18, 0) NULL,
                [Type] [numeric](18, 0) NULL,
                [MasterSourceFileId] [varchar] (500) COLLATE SQL_Latin1_General_CP1_CS_AS NULL,
                [SecondaryMasterSourceFileId] [varchar] (500) COLLATE SQL_Latin1_General_CP1_CS_AS NULL,
                [MasterEudonetFileId] [numeric](18, 0) NULL,
                [Orig] [varchar] (50) COLLATE SQL_Latin1_General_CP1_CS_AS NULL
				) ON [PRIMARY]
END
	