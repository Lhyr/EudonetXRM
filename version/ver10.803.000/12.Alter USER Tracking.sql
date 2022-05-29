-- Champ systeme
if not exists (SELECT 1
		from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		where sys.tables.name like 'WORKFLOWTRACKING' and syscolumns.name like 'WFTrTag' )
BEGIN
	ALTER TABLE WORKFLOWTRACKING ADD WFTrTag varchar(100)  NULL default('')
END

EXEC('UPDATE WORKFLOWTRACKING set WFTrTag = '''' WHERE WFTrTag IS  NULL')