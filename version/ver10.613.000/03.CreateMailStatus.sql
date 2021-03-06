 
 /*
US: 2 550 / 4048
SPHAM 
*/
 
IF NOT EXISTS(SELECT * FROM SYSOBJECTS where id = object_id(N'[dbo].[MAILSTATUS]') and OBJECTPROPERTY(id, N'IsUserTable') = 1 )
BEGIN
  
	CREATE TABLE [dbo].[MAILSTATUS](
		[MAILSTATUSID] [int] IDENTITY(1,1) NOT NULL,
		[MAILSTATUSEMAIL] [nvarchar](200) NOT NULL,
		[MAILSTATUSSEUDO] [int] NULL,
		[MAILSTATUSTECH] [int] NULL,
		[MAILSTATUSSUBTECH] [int] NULL,
	 CONSTRAINT [PK_MAILSTATUSID] PRIMARY KEY CLUSTERED 
	(
		[MAILSTATUSID] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON/*, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF*/) ON [PRIMARY],
	 CONSTRAINT [AK_MAILSTATUSEMAIL] UNIQUE NONCLUSTERED 
	(
		[MAILSTATUSEMAIL] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON/*, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF*/) ON [PRIMARY]
	) ON [PRIMARY]
 


  
 



END
 