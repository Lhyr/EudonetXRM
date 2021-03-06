IF NOT EXISTS (SELECT 1
		from   EUDOTRAIT.sys.tables
		where EUDOTRAIT.sys.tables.name = 'SUBSCRIPTIONS'  )
BEGIN	

	CREATE TABLE EUDOTRAIT.[dbo].[SUBSCRIPTIONS](
		[id] [numeric](18, 0) IDENTITY(1,1) NOT NULL,
		[HashDatabase] [varchar](MAX) NOT NULL,
		[HashUser] [varchar](MAX) NOT NULL,
		[ExpirationDate] [Datetime] NOT NULL,
	 CONSTRAINT [PK_SUBSCRIPTIONS] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	) ON [PRIMARY]


	 


END