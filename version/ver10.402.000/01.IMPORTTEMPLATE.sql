IF  NOT EXISTS (SELECT * FROM sys.objects 
WHERE object_id = OBJECT_ID(N'[dbo].[IMPORTTEMPLATE]') AND type in (N'U'))

BEGIN
CREATE TABLE [dbo].[IMPORTTEMPLATE](
	[ImportTemplateId] [INT] IDENTITY(1,1) NOT NULL,
	[UserId] [INT] NULL,
	[Libelle] [nvarchar](1000) NULL,
	[Tab] [INT] NULL,
	[ViewPermId] [INT] NULL,
	[UpdatePermId] [INT] NULL,
	[ViewRulesId] [nvarchar](500) NULL,
	[DateLastModified] [datetime] NULL,
	[Param] [nvarchar](MAX) NULL,
 CONSTRAINT [PKC_Mapping] PRIMARY KEY CLUSTERED 
(
	[ImportTemplateId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
end
