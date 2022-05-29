IF NOT EXISTS (SELECT 1
		from  sys.tables
		where sys.tables.name = 'res_specifs'  )
BEGIN

	CREATE TABLE [dbo].[RES_SPECIFS](
		[SpecifId] [numeric](18, 0) NOT NULL,
		[LANG_00] [nvarchar](255) NULL,
		[LANG_01] [nvarchar](255) NULL,
		[LANG_02] [nvarchar](255) NULL,
		[LANG_03] [nvarchar](255) NULL,
		[LANG_04] [nvarchar](255) NULL,
		[LANG_05] [nvarchar](255) NULL,
		[LANG_06] [nvarchar](255) NULL,
		[LANG_07] [nvarchar](255) NULL,
		[LANG_08] [nvarchar](255) NULL,
		[LANG_09] [nvarchar](255) NULL,
		[LANG_10] [nvarchar](255) NULL,
	 CONSTRAINT [PK_RES_SPECIFS] PRIMARY KEY CLUSTERED 
	(
		[SpecifId] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]


	ALTER TABLE [dbo].[RES_SPECIFS]  WITH CHECK ADD  CONSTRAINT [FK_RES_SPECIFS_SPECIFS] FOREIGN KEY([SpecifId])
	REFERENCES [dbo].[SPECIFS] ([SpecifId])

	ALTER TABLE [dbo].[RES_SPECIFS] CHECK CONSTRAINT [FK_RES_SPECIFS_SPECIFS]

END
