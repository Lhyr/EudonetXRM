/*
SPH
D'après spécification fonctionnelle, la plannification des rapports est désormais liée fortement au rapport :
cad qu'un rapport ne peut avoir qu'une seule plannification.
Il est donc nécessaire de stocker dans le rapport cette information.
Elle sera stockée sous forme de sa représenation JSON cf eScheduleReportData


*/
if not exists (SELECT 1
	from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
	where sys.tables.name = 'REPORT' and syscolumns.name like 'SCHEDULEPARAM')
BEGIN
	exec('ALTER TABLE [REPORT] ADD [SCHEDULEPARAM] [varchar](5000) NULL');
END      
	