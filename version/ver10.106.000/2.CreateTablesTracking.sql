 
 
IF NOT EXISTS (SELECT 1
		from   EUDOTRAIT.sys.tables
		where EUDOTRAIT.sys.tables.name = 'EXTERNALMODULES'  )
BEGIN		
	CREATE TABLE EUDOTRAIT.[dbo].EXTERNALMODULES(	
		[MODULEID] NUMERIC(18,0) IDENTITY(1,1) not null,
		[MODULENAME] [varchar](500)  NOT NULL,
		[MODULECODENAME] [varchar](500)  NOT NULL,
		[VERSION] [varchar](50)  NULL
 CONSTRAINT [PK_EXTERNALMODULES] PRIMARY KEY CLUSTERED 
(
	[MODULEID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

END


IF NOT EXISTS (SELECT 1 FROM EUDOTRAIT.[dbo].[EXTERNALMODULES] WHERE MODULECODENAME='EXTERNALTRACKING')
	INSERT INTO EUDOTRAIT.[dbo].EXTERNALMODULES(MODULECODENAME, MODULENAME,VERSION) SELECT    'EXTERNALTRACKING','Module de tracking externalis√©', '0.5'

	

	

IF NOT EXISTS (SELECT 1
		from   EUDOTRAIT.sys.tables
		where EUDOTRAIT.sys.tables.name = 'EXTERNALCLIENTS'  )
BEGIN		
	CREATE TABLE EUDOTRAIT.[dbo].EXTERNALCLIENTS(
		[id] [numeric](18, 0) IDENTITY(1,1) NOT NULL,
		[MODULEID] NUMERIC(18,0) not null,
		[serial] [varchar](500)  NOT NULL,
		[clientname] [varchar](50) NOT NULL,
		[ENABLED] BIT NULL,
 CONSTRAINT [PK_EXTERNALCLIENTS] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

END



IF NOT EXISTS (SELECT 1
		from   EUDOTRAIT.sys.tables
		where EUDOTRAIT.sys.tables.name = 'EXTERNALTRACKING'  )
BEGIN

	CREATE TABLE EUDOTRAIT.[dbo].[EXTERNALTRACKING](
		[id] [numeric](18, 0) IDENTITY(1,1) NOT NULL,
		[serial] [varchar](500) NOT NULL,
		[uid] [varchar](500) NOT NULL,
		[campaignid] [numeric](18, 0) NOT NULL,
		[token] [varchar](500) NOT NULL,
		[p] [varchar](500) NOT NULL,
		[cs] [varchar](500) NOT NULL,		
		[ip] [varchar](100) NOT NULL,		
		[navigateurheader] [varchar](5000) NOT NULL,		
		[datelink] [datetime] NOT  NULL,
 CONSTRAINT [PK_EXTERNALTRACKING] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

 
END 

IF NOT EXISTS (SELECT 1
		from   EUDOTRAIT.sys.tables
		where EUDOTRAIT.sys.tables.name = 'EXTERNALCAMPAIGN'  )
BEGIN		
	CREATE TABLE EUDOTRAIT.[dbo].[EXTERNALCAMPAIGN](
		[id] [numeric](18, 0) IDENTITY(1,1) NOT NULL,
		[serial] [varchar](500)  NOT NULL,
		[uid] [varchar](50)  NOT NULL,
		[campaignid] [numeric](18, 0) NOT NULL,
		[DateTrackExpirate] [datetime]   NULL,
		[datepurge] [datetime]   NULL,
 CONSTRAINT [PK_EXTERNALCAMPAIGN] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

END

