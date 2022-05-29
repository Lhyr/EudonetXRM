IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'PK_UnsubscribeMail' AND object_id = object_id(N'[dbo].[UnsubscribeMail]'))
	AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'PKC_UnsubscribeMail' AND object_id = object_id(N'[dbo].[UnsubscribeMail]'))
BEGIN
	CREATE CLUSTERED INDEX [PKC_UnsubscribeMail] ON [dbo].[UnsubscribeMail]
	(
		[UnsubscribeMailId] ASC,
		[EvtId] ASC
	) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
END
