if not exists (SELECT 1
		from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		where sys.tables.name like 'WORKFLOWSCENARIO' and syscolumns.name like 'WFSCFirstActionId' )
BEGIN
	ALTER TABLE [WORKFLOWSCENARIO] ADD WFSCFirstActionId [int] NULL 	
END
