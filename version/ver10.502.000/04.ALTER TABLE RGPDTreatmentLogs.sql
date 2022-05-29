IF EXISTS(SELECT * FROM SYSOBJECTS where id = object_id(N'[dbo].[RGPDTreatmentsLogs]') and OBJECTPROPERTY(id, N'IsUserTable') = 1 )
BEGIN
	--Creation nouveau champ Date de fin
	IF NOT EXISTS (SELECT 1
				FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
				WHERE sys.tables.name like 'RGPDTreatmentsLogs' and syscolumns.name like 'EndDate' )
	BEGIN
		ALTER TABLE [RGPDTreatmentsLogs]
		ADD [EndDate] datetime null;
	END

	
	--Creation nouveau champ Statut
	IF NOT EXISTS (SELECT 1
				FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
				WHERE sys.tables.name like 'RGPDTreatmentsLogs' and syscolumns.name like 'Status' )
	BEGIN
		ALTER TABLE [RGPDTreatmentsLogs]
		ADD [Status] numeric(18) null
	END


END
