if not exists (SELECT 1
		from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		where sys.tables.name like 'WORKFLOWACTIVITIES' and syscolumns.name like 'WFAcDescription_HTML' )
BEGIN
	ALTER TABLE [WORKFLOWACTIVITIES] ADD WFAcDescription_HTML [BIT] NULL
END

-- Champ systeme
if not exists (SELECT 1
		from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		where sys.tables.name like 'WORKFLOWACTIVITIES' and syscolumns.name like 'WFAcInfos' )
BEGIN
	ALTER TABLE [WORKFLOWACTIVITIES] ADD WFAcInfos nvarchar(max)  NULL
END
