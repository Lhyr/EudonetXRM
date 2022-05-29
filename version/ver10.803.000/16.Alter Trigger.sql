-- Champ systeme
if not exists (SELECT 1
		from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		where sys.tables.name like 'WORKFLOWTRIGGER' and syscolumns.name like 'WFTJobId' )
BEGIN
	ALTER TABLE WORKFLOWTRIGGER ADD WFTJobId  int 
END

 