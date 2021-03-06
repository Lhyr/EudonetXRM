 
IF NOT EXISTS (SELECT 1
		from   EUDOTRAIT.sys.tables
		where EUDOTRAIT.sys.tables.name = 'EXTERNALMAILUNSUB'  )
BEGIN	

	CREATE TABLE EUDOTRAIT.[dbo].[EXTERNALMAILUNSUB](
		[id] [numeric](18, 0) IDENTITY(1,1) NOT NULL,
		[EMAIL] [varchar](500) NOT NULL,
		[CLIENTID] [numeric](18, 0) NOT NULL,
		[UID] [varchar](500) NOT NULL,
		[CAMPAIGNID] [numeric](18, 0) NOT NULL,
		[CATEGORYID] [numeric](18, 0) NOT NULL,
		[DATEUNSUB] [datetime] NULL,
		[HISTO] [bit] NULL,
	 CONSTRAINT [PK_EXTERNALMAILUNSUB] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	) ON [PRIMARY]

 

 
	ALTER TABLE EUDOTRAIT.[dbo].[EXTERNALMAILUNSUB]  WITH NOCHECK ADD  CONSTRAINT [FK_EXTERNALCLIENTS_EXTERNALMAILUNSUB] FOREIGN KEY([CLIENTID])
	REFERENCES EUDOTRAIT.[dbo].[EXTERNALCLIENTS] ([id])
 

	ALTER TABLE EUDOTRAIT.[dbo].[EXTERNALMAILUNSUB] CHECK CONSTRAINT [FK_EXTERNALCLIENTS_EXTERNALMAILUNSUB]
 

	ALTER TABLE EUDOTRAIT.[dbo].[EXTERNALMAILUNSUB] ADD  DEFAULT ((0)) FOR [HISTO]
	 


END

