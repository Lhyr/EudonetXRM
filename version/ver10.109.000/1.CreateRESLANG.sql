IF NOT EXISTS(SELECT 1 FROM EUDORES.[dbo].[APP] WHERE Id = 0)
	INSERT EUDORES.[dbo].[APP] ([Id], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05], [LANG_06], [LANG_07], [LANG_08], [LANG_09], [LANG_10]) VALUES (CAST(0 AS Numeric(18, 0)), N'Français', N'English', N'Deutsch', N'Nederlands', N'Español', N'Italiano', N'', N'', N'', N'', N'')
IF NOT EXISTS (SELECT 1
		from   EUDORES.sys.tables
		where EUDORES.sys.tables.name = 'LANG'  )
BEGIN
	-- CREATION DE LA TABLE LANG
	CREATE TABLE 
	
	EUDORES.[dbo].[LANG](
		[Id] [numeric](18, 0) NOT NULL,
		[Libelle] [nvarchar](500) NULL,
		CONSTRAINT [PK_LANG] PRIMARY KEY NONCLUSTERED 
		(
			[Id] ASC
		)
			WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	) ON [PRIMARY]

	


truncate table EUDORES.[dbo].lang
	-- INITIALISATION DE LA TABLE LANG
INSERT INTO EUDORES.[dbo].lang( id,  libelle)
SELECT CAST(REPLACE(b,'LANG_','') AS NUMERIC) LANG ,a FROM (
	SELECT id, lang_00,lang_01,lang_02,lang_03,lang_04,lang_05,lang_06,lang_07,lang_08,lang_09,lang_10 
	FROM EUDORES.[dbo].app WHERE id = 0
) z
UNPIVOT
	(a FOR b IN (lang_00,lang_01,lang_02,lang_03,lang_04,lang_05,lang_06,lang_07,lang_08,lang_09,lang_10)) AS u

END

IF NOT EXISTS (SELECT 1
		from   EUDORES.sys.tables
		where EUDORES.sys.tables.name = 'RESAPP'  )
BEGIN
	 
    -- CREATION DE LA TABLE RES
	CREATE TABLE EUDORES.[dbo].[RESAPP](
		[Id] [numeric](18, 0) IDENTITY(1,1) NOT NULL,
		[ResId] [numeric](18, 0) NOT NULL,
		[Id_Lang] [numeric](18, 0) NOT NULL,
		[Lang] [nvarchar](1000) NULL,
		CONSTRAINT [PK_RES] PRIMARY KEY NONCLUSTERED 
		(
			[Id] ASC
		)
			WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	) ON [PRIMARY]

	
	CREATE UNIQUE CLUSTERED INDEX [IDX_LANG_RESID] ON EUDORES.[dbo].[RESAPP] 
(
	[ResId] ASC,
	[Id_Lang] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	
	


truncate table EUDORES.[dbo].resapp
-- INITIALISATION DE LA TABLE RES
insert into EUDORES.[dbo].RESAPP( resid, id_lang, lang)
select id,cast(replace(b,'LANG_','') as numeric) LANG ,a from(
select id, lang_00,lang_01,lang_02,lang_03,lang_04,lang_05,lang_06,lang_07,lang_08,lang_09,lang_10 from EUDORES.[dbo].app  --where id=0
) z
unpivot
 (a for b in (lang_00,lang_01,lang_02,lang_03,lang_04,lang_05,lang_06,lang_07,lang_08,lang_09,lang_10)) as u
	 
	 
END	 