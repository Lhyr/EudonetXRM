-- Champ systeme
if not exists (SELECT 1
		from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		where sys.tables.name like 'WORKFLOWSTEP' and syscolumns.name like 'WFSTargetDescid' )
BEGIN
	ALTER TABLE WORKFLOWSTEP ADD WFSTargetDescid  int 
END

 