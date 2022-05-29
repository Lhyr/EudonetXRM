--GCH - 26/01/2015 - #36624 - Paramétrage dans XRM de la plateforme SMTP office 365 comme dans la V7 - 0 - Ajout du nouveau champ dans config SMTPEmailingEnableSsl

if not exists (SELECT 1
		from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		where sys.tables.name like 'CONFIG' and syscolumns.name like 'SMTPEmailingSSL' )
BEGIN
	ALTER TABLE [CONFIG] ADD SMTPEmailingSSL BIT NULL
END