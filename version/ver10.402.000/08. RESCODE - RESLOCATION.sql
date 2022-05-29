IF NOT EXISTS(SELECT * FROM SYSOBJECTS WHERE id = object_id(N'[dbo].[RESLOCATION]') AND OBJECTPROPERTY(id, N'IsUserTable') = 1 )
BEGIN
	CREATE TABLE [dbo].[RESLOCATION](
	[ResLocationId] [int] IDENTITY(1,1) NOT NULL,
	[ResPath] [nvarchar](max) NULL,
	[Nature] [int] NOT NULL,
	[Identifier] [nvarchar](max) NULL
	 CONSTRAINT [PK_RESLOCATION] PRIMARY KEY CLUSTERED 
	(
		[ResLocationId] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END

IF NOT EXISTS(SELECT * FROM SYSOBJECTS WHERE id = object_id(N'[dbo].[RESCODE]') AND OBJECTPROPERTY(id, N'IsUserTable') = 1 )
BEGIN
	CREATE TABLE [dbo].[RESCODE](
		[ResCodeId] [numeric](18, 0) IDENTITY(1,1) NOT NULL,
		[Code] [int] NOT NULL,
		[LangId] [int] NOT NULL,
		[ResValue] [nvarchar](max) NULL,
		[ResLocationId] [int] NULL,
    CONSTRAINT [PK_RESCODE] PRIMARY KEY CLUSTERED 
	(
		[ResCodeId] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

	ALTER TABLE [dbo].[RESCODE]  WITH CHECK ADD  CONSTRAINT [FK_RESCODE_RESLOCATION] FOREIGN KEY([ResLocationId])
	REFERENCES [dbo].[RESLOCATION] ([ResLocationId])
	ON DELETE CASCADE

	ALTER TABLE [dbo].[RESCODE] CHECK CONSTRAINT [FK_RESCODE_RESLOCATION]

END

