IF NOT EXISTS (SELECT 1
		from   sys.tables
		where sys.tables.name = 'MAPLANG'  )
BEGIN

	CREATE TABLE [dbo].[MAPLANG](
		[id] [numeric](18, 0) IDENTITY(1,1) NOT NULL,
		[disabled] [bit] NULL,
		[LANG_ID] [int] NOT NULL,
		[LANG_LABEL] [varchar](100) NOT NULL,
		[LANG_SYSID] [int] NOT NULL
	) ON [PRIMARY]
 

END


IF (select count(id) from [MAPLANG]) =0
BEGIN


INSERT INTO MAPLANG (disabled, LANG_ID, LANG_LABEL, LANG_SYSID) 
 
	SELECT 
	
		0,
		cast(replace(colname,'LANG_','') as int) ,
		name,
		cast(replace(colname,'LANG_','') as int) 
		
	FROM eudores..APP 
	 
	 unpivot (name for colname in( LANG_00,
	LANG_01,
	LANG_02,
	LANG_03,
	LANG_04,
	LANG_05,
	LANG_06,
	LANG_07,
	LANG_08,
	LANG_09,
	LANG_10)) uxrar 
	 WHERE ID=0

END
