
if not exists (SELECT 1
		from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		where sys.tables.name like 'WORKFLOWTRACKING' and syscolumns.name like 'WFTrDetails' )
BEGIN
	ALTER TABLE WORKFLOWTRACKING ADD WFTrDetails nvarchar(max) NULL

END

 