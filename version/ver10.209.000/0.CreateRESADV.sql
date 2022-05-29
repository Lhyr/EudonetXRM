IF NOT EXISTS (SELECT 1
		from  sys.tables
		where sys.tables.name = 'RESADV'  )
BEGIN

CREATE TABLE [dbo].[RESADV](

	[ID] [int] IDENTITY(1,1) NOT NULL,
	[DESCID] [int] NOT NULL,
	[ID_LANG] [smallint] NOT NULL,
	[LANG] [nvarchar](max) NOT NULL,
	[TYPE] [smallint] NOT NULL
)  






END
