-- Champ systeme
if not exists (SELECT 1
		from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		where sys.tables.name like 'WORKFLOWACTION' and syscolumns.name like 'WFADelayCount' )
BEGIN
	ALTER TABLE WORKFLOWACTION ADD WFADelayCount  int 
END

if not exists (SELECT 1
		from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		where sys.tables.name like 'WORKFLOWACTION' and syscolumns.name like 'WFADelayType' )
BEGIN
	ALTER TABLE WORKFLOWACTION ADD WFADelayType  int 
END

 