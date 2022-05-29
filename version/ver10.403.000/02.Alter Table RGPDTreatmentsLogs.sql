IF EXISTS(SELECT * FROM SYSOBJECTS where id = object_id(N'[dbo].[RGPDTreatmentsLogs]') and OBJECTPROPERTY(id, N'IsUserTable') = 1 )
BEGIN
	DECLARE @bUnicode BIT	
	SELECT @bUnicode = [UNICODE] FROM [CONFIG] WHERE [UserId] = 0
	
	--Creation nouveau champ texte pour 01
	IF NOT EXISTS (SELECT 1
				FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
				WHERE sys.tables.name like 'RGPDTreatmentsLogs' and syscolumns.name like 'LogLabel' )
	BEGIN
		IF @bUnicode = 1
		BEGIN
			ALTER TABLE [RGPDTreatmentsLogs]
			ADD [LogLabel] nvarchar(150) null;
		END
		ELSE
		BEGIN
			ALTER TABLE [RGPDTreatmentsLogs]
			ADD [LogLabel] varchar(150) null;
		END
	END
	
	
	--Modifications de Date en DateTime
	IF EXISTS (SELECT 1
				FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
				WHERE sys.tables.name like 'RGPDTreatmentsLogs' and syscolumns.name like 'LogDate' )
	BEGIN
		UPDATE [RGPDTreatmentsLogs] SET
		[LogDate] = null
		WHERE ISDATE([LogDate]) <> 1
		
		ALTER TABLE [RGPDTreatmentsLogs] 
		ALTER COLUMN [LogDate] datetime null;
	END
	ELSE
	BEGIN
		ALTER TABLE [RGPDTreatmentsLogs] 
		ADD [LogDate] datetime null;
	END
	
	
	--Modifications de TabLabel varchar(max)/nvarchar(max)
	IF EXISTS (SELECT 1
				FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
				WHERE sys.tables.name like 'RGPDTreatmentsLogs' and syscolumns.name like 'TabLabel' )
	BEGIN
		IF @bUnicode = 1
		BEGIN
			ALTER TABLE [RGPDTreatmentsLogs] 
			ALTER COLUMN [TabLabel] nvarchar(max) null;
		END
		ELSE
		BEGIN
			ALTER TABLE [RGPDTreatmentsLogs] 
			ALTER COLUMN [TabLabel] varchar(max) null;
		END
	END
	
	IF NOT EXISTS (SELECT 1
				FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
				WHERE sys.tables.name like 'RGPDTreatmentsLogs' and syscolumns.name like 'TabLabel_HTML' )
	BEGIN
		ALTER TABLE [RGPDTreatmentsLogs]
		ADD [TabLabel_HTML] bit null;
	END
	
	--Modifications de DeadlineLabel en nvarchar(150)
	IF EXISTS (SELECT 1
				FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
				WHERE sys.tables.name like 'RGPDTreatmentsLogs' and syscolumns.name like 'DeadlineLabel' )
	AND @bUnicode = 1
	BEGIN
		ALTER TABLE [RGPDTreatmentsLogs] 
		ALTER COLUMN [DeadlineLabel] nvarchar(150) null;
	END
	
	--Modifications de RGPDType en numeric(18)
	IF EXISTS (SELECT 1
				FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
				WHERE sys.tables.name like 'RGPDTreatmentsLogs' and syscolumns.name like 'RGPDType' )
	BEGIN
		UPDATE [RGPDTreatmentsLogs] SET
		[RGPDType] = null
		WHERE ISNUMERIC([RGPDType]) <> 1
		
		ALTER TABLE [RGPDTreatmentsLogs] 
		ALTER COLUMN [RGPDType] numeric(18) null;
	END
	ELSE
	BEGIN
		ALTER TABLE [RGPDTreatmentsLogs] 
		ADD [RGPDType] numeric(18) null;
	END
	
	
	--Modifications de RGPDTypeLabel en varchar(100)/nvarchar(100)
	/*
	IF EXISTS (SELECT 1
				FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
				WHERE sys.tables.name like 'RGPDTreatmentsLogs' and syscolumns.name like 'RGPDTypeLabel' )
	AND @bUnicode = 1
	BEGIN
		ALTER TABLE [RGPDTreatmentsLogs] 
		ALTER COLUMN [RGPDTypeLabel] nvarchar(100) null;
	END
	ELSE
	BEGIN
		ALTER TABLE [RGPDTreatmentsLogs] 
		ALTER COLUMN [RGPDTypeLabel] varchar(100) null;
	END
	*/
	
	--Suppression de RGPDTypeLabel
	IF EXISTS (SELECT 1
				FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
				WHERE sys.tables.name like 'RGPDTreatmentsLogs' and syscolumns.name like 'RGPDTypeLabel' )
	BEGIN
		ALTER TABLE [RGPDTreatmentsLogs] 
		DROP COLUMN [RGPDTypeLabel];
	END
	
	
	--Modifications de FieldsLabel en nvarchar(max)
	IF EXISTS (SELECT 1
				FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
				WHERE sys.tables.name like 'RGPDTreatmentsLogs' and syscolumns.name like 'FieldsLabel' )
	AND @bUnicode = 1
	BEGIN
		ALTER TABLE [RGPDTreatmentsLogs] 
		ALTER COLUMN [FieldsLabel] nvarchar(max) null;		
	END
	
	IF NOT EXISTS (SELECT 1
				FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
				WHERE sys.tables.name like 'RGPDTreatmentsLogs' and syscolumns.name like 'FieldsLabel_HTML' )
	BEGIN
		ALTER TABLE [RGPDTreatmentsLogs]
		ADD [FieldsLabel_HTML] bit null;
	END
	
	
	--Creation des nouvelles rubriques
	IF NOT EXISTS (SELECT 1
				FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
				WHERE sys.tables.name like 'RGPDTreatmentsLogs' and syscolumns.name like 'IsRGPD' )
	BEGIN
		ALTER TABLE [RGPDTreatmentsLogs]
		ADD [IsRGPD] bit null;
	END
	
	IF NOT EXISTS (SELECT 1
				FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
				WHERE sys.tables.name like 'RGPDTreatmentsLogs' and syscolumns.name like 'ReferenceNumber' )
	BEGIN
		ALTER TABLE [RGPDTreatmentsLogs]
		ADD [ReferenceNumber] numeric(18) null;
	END
	
	IF NOT EXISTS (SELECT 1
				FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
				WHERE sys.tables.name like 'RGPDTreatmentsLogs' and syscolumns.name like 'RGPDFieldsLabel' )
	BEGIN
		IF @bUnicode = 1
		BEGIN
			ALTER TABLE [RGPDTreatmentsLogs]
			ADD [RGPDFieldsLabel] nvarchar(max) null;
		END
		ELSE
		BEGIN
			ALTER TABLE [RGPDTreatmentsLogs]
			ADD [RGPDFieldsLabel] varchar(max) null;
		END
	END
	
	IF NOT EXISTS (SELECT 1
				FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
				WHERE sys.tables.name like 'RGPDTreatmentsLogs' and syscolumns.name like 'RGPDFieldsLabel_HTML' )
	BEGIN
		ALTER TABLE [RGPDTreatmentsLogs]
		ADD [RGPDFieldsLabel_HTML] bit null;
	END
	
	IF NOT EXISTS (SELECT 1
				FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
				WHERE sys.tables.name like 'RGPDTreatmentsLogs' and syscolumns.name like 'TreatmentResponsibleID' )
	BEGIN
		ALTER TABLE [RGPDTreatmentsLogs]
		ADD [TreatmentResponsibleID] varchar(512) null;
	END
	
	IF NOT EXISTS (SELECT 1
				FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
				WHERE sys.tables.name like 'RGPDTreatmentsLogs' and syscolumns.name like 'TreatmentResponsibleLabel' )
	BEGIN
		IF @bUnicode = 1
		BEGIN
			ALTER TABLE [RGPDTreatmentsLogs]
			ADD [TreatmentResponsibleLabel] nvarchar(max) null;
		END
		ELSE
		BEGIN
			ALTER TABLE [RGPDTreatmentsLogs]
			ADD [TreatmentResponsibleLabel] varchar(max) null;
		END
	END
	
	IF NOT EXISTS (SELECT 1
				FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
				WHERE sys.tables.name like 'RGPDTreatmentsLogs' and syscolumns.name like 'TreatmentResponsibleLabel_HTML' )
	BEGIN
		ALTER TABLE [RGPDTreatmentsLogs]
		ADD [TreatmentResponsibleLabel_HTML] bit null;
	END
	
	
	IF NOT EXISTS (SELECT 1
				FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
				WHERE sys.tables.name like 'RGPDTreatmentsLogs' and syscolumns.name like 'DataProtectionDelegateID' )
	BEGIN
		ALTER TABLE [RGPDTreatmentsLogs]
		ADD [DataProtectionDelegateID] varchar(512) null;
	END
	
	IF NOT EXISTS (SELECT 1
				FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
				WHERE sys.tables.name like 'RGPDTreatmentsLogs' and syscolumns.name like 'DataProtectionDelegateLabel' )
	BEGIN
		IF @bUnicode = 1
		BEGIN
			ALTER TABLE [RGPDTreatmentsLogs]
			ADD [DataProtectionDelegateLabel] nvarchar(max) null;
		END
		ELSE
		BEGIN
			ALTER TABLE [RGPDTreatmentsLogs]
			ADD [DataProtectionDelegateLabel] varchar(max) null;
		END
	END
	
	IF NOT EXISTS (SELECT 1
				FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
				WHERE sys.tables.name like 'RGPDTreatmentsLogs' and syscolumns.name like 'DataProtectionDelegateLabel_HTML' )
	BEGIN
		ALTER TABLE [RGPDTreatmentsLogs]
		ADD [DataProtectionDelegateLabel_HTML] bit null;
	END
	
	
	IF NOT EXISTS (SELECT 1
				FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
				WHERE sys.tables.name like 'RGPDTreatmentsLogs' and syscolumns.name like 'RepresentativeID' )
	BEGIN
		ALTER TABLE [RGPDTreatmentsLogs]
		ADD [RepresentativeID] varchar(512) null;
	END
	
	IF NOT EXISTS (SELECT 1
				FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
				WHERE sys.tables.name like 'RGPDTreatmentsLogs' and syscolumns.name like 'RepresentativeLabel' )
	BEGIN
		IF @bUnicode = 1
		BEGIN
			ALTER TABLE [RGPDTreatmentsLogs]
			ADD [RepresentativeLabel] nvarchar(max) null;
		END
		ELSE
		BEGIN
			ALTER TABLE [RGPDTreatmentsLogs]
			ADD [RepresentativeLabel] varchar(max) null;
		END
	END
	
	IF NOT EXISTS (SELECT 1
				FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
				WHERE sys.tables.name like 'RGPDTreatmentsLogs' and syscolumns.name like 'RepresentativeLabel_HTML' )
	BEGIN
		ALTER TABLE [RGPDTreatmentsLogs]
		ADD [RepresentativeLabel_HTML] bit null;
	END
	
	
	IF NOT EXISTS (SELECT 1
				FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
				WHERE sys.tables.name like 'RGPDTreatmentsLogs' and syscolumns.name like 'TreatmentPurpose' )
	BEGIN
		IF @bUnicode = 1
		BEGIN
			ALTER TABLE [RGPDTreatmentsLogs]
			ADD [TreatmentPurpose] nvarchar(max) null;
		END
		ELSE
		BEGIN
			ALTER TABLE [RGPDTreatmentsLogs]
			ADD [TreatmentPurpose] varchar(max) null;
		END
	END
	
	IF NOT EXISTS (SELECT 1
				FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
				WHERE sys.tables.name like 'RGPDTreatmentsLogs' and syscolumns.name like 'TreatmentPurpose_HTML' )
	BEGIN
		ALTER TABLE [RGPDTreatmentsLogs]
		ADD [TreatmentPurpose_HTML] bit null;
	END
	
	IF NOT EXISTS (SELECT 1
				FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
				WHERE sys.tables.name like 'RGPDTreatmentsLogs' and syscolumns.name like 'SecurityMeasure' )
	BEGIN
		IF @bUnicode = 1
		BEGIN
			ALTER TABLE [RGPDTreatmentsLogs]
			ADD [SecurityMeasure] nvarchar(500) null;
		END
		ELSE
		BEGIN
			ALTER TABLE [RGPDTreatmentsLogs]
			ADD [SecurityMeasure] varchar(500) null;
		END
	END
	
	/*
	IF NOT EXISTS (SELECT 1
				FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
				WHERE sys.tables.name like 'RGPDTreatmentsLogs' and syscolumns.name like 'PersonnalDataCategories' )
	BEGIN
		IF @bUnicode = 1
		BEGIN
			ALTER TABLE [RGPDTreatmentsLogs]
			ADD [PersonnalDataCategories] nvarchar(max) null;
		END
		ELSE
		BEGIN
			ALTER TABLE [RGPDTreatmentsLogs]
			ADD [PersonnalDataCategories] varchar(max) null;
		END
	END
	
	IF NOT EXISTS (SELECT 1
				FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
				WHERE sys.tables.name like 'RGPDTreatmentsLogs' and syscolumns.name like 'PersonnalDataCategories_HTML' )
	BEGIN
		ALTER TABLE [RGPDTreatmentsLogs]
		ADD [PersonnalDataCategories_HTML] bit null;
	END
	*/
	
	IF EXISTS (SELECT 1
				FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
				WHERE sys.tables.name like 'RGPDTreatmentsLogs' and syscolumns.name like 'PersonnalDataCategories' )
	BEGIN
		ALTER TABLE [RGPDTreatmentsLogs]
		DROP COLUMN [PersonnalDataCategories];
	END
	
	IF EXISTS (SELECT 1
				FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
				WHERE sys.tables.name like 'RGPDTreatmentsLogs' and syscolumns.name like 'PersonnalDataCategories_HTML' )
	BEGIN
		ALTER TABLE [RGPDTreatmentsLogs]
		DROP COLUMN [PersonnalDataCategories_HTML];
	END	
	
	/*
	IF NOT EXISTS (SELECT 1
				FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
				WHERE sys.tables.name like 'RGPDTreatmentsLogs' and syscolumns.name like 'SensibleData' )
	BEGIN
		IF @bUnicode = 1
		BEGIN
			ALTER TABLE [RGPDTreatmentsLogs]
			ADD [SensibleData] nvarchar(max) null;
		END
		ELSE
		BEGIN
			ALTER TABLE [RGPDTreatmentsLogs]
			ADD [SensibleData] varchar(max) null;
		END
	END
	
	IF NOT EXISTS (SELECT 1
				FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
				WHERE sys.tables.name like 'RGPDTreatmentsLogs' and syscolumns.name like 'SensibleData_HTML' )
	BEGIN
		ALTER TABLE [RGPDTreatmentsLogs]
		ADD [SensibleData_HTML] bit null;
	END
	*/
	
	IF EXISTS (SELECT 1
				FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
				WHERE sys.tables.name like 'RGPDTreatmentsLogs' and syscolumns.name like 'SensibleData' )
	BEGIN
		ALTER TABLE [RGPDTreatmentsLogs]
		DROP COLUMN [SensibleData];
	END
	
	IF EXISTS (SELECT 1
				FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
				WHERE sys.tables.name like 'RGPDTreatmentsLogs' and syscolumns.name like 'SensibleData_HTML' )
	BEGIN
		ALTER TABLE [RGPDTreatmentsLogs]
		DROP COLUMN [SensibleData_HTML];
	END
	
	IF NOT EXISTS (SELECT 1
				FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
				WHERE sys.tables.name like 'RGPDTreatmentsLogs' and syscolumns.name like 'PeopleCategories' )
	BEGIN
		IF @bUnicode = 1
		BEGIN
			ALTER TABLE [RGPDTreatmentsLogs]
			ADD [PeopleCategories] nvarchar(max) null;
		END
		ELSE
		BEGIN
			ALTER TABLE [RGPDTreatmentsLogs]
			ADD [PeopleCategories] varchar(max) null;
		END
	END
	
	IF NOT EXISTS (SELECT 1
				FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
				WHERE sys.tables.name like 'RGPDTreatmentsLogs' and syscolumns.name like 'PeopleCategories_HTML' )
	BEGIN
		ALTER TABLE [RGPDTreatmentsLogs]
		ADD [PeopleCategories_HTML] bit null;
	END
	
	IF NOT EXISTS (SELECT 1
				FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
				WHERE sys.tables.name like 'RGPDTreatmentsLogs' and syscolumns.name like 'Recipients' )
	BEGIN
		IF @bUnicode = 1
		BEGIN
			ALTER TABLE [RGPDTreatmentsLogs]
			ADD [Recipients] nvarchar(500) null;
		END
		ELSE
		BEGIN
			ALTER TABLE [RGPDTreatmentsLogs]
			ADD [Recipients] varchar(500) null;
		END
	END
	
	IF NOT EXISTS (SELECT 1
				FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
				WHERE sys.tables.name like 'RGPDTreatmentsLogs' and syscolumns.name like 'OutsideEUTransferts' )
	BEGIN
		IF @bUnicode = 1
		BEGIN
			ALTER TABLE [RGPDTreatmentsLogs]
			ADD [OutsideEUTransferts] nvarchar(max) null;
		END
		ELSE
		BEGIN
			ALTER TABLE [RGPDTreatmentsLogs]
			ADD [OutsideEUTransferts] varchar(max) null;
		END
	END
	
	IF NOT EXISTS (SELECT 1
				FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
				WHERE sys.tables.name like 'RGPDTreatmentsLogs' and syscolumns.name like 'OutsideEUTransferts_HTML' )
	BEGIN
		ALTER TABLE [RGPDTreatmentsLogs]
		ADD [OutsideEUTransferts_HTML] bit null;
	END
	
	IF NOT EXISTS (SELECT 1
				FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
				WHERE sys.tables.name like 'RGPDTreatmentsLogs' and syscolumns.name like 'Comments' )
	BEGIN
		IF @bUnicode = 1
		BEGIN
			ALTER TABLE [RGPDTreatmentsLogs]
			ADD [Comments] nvarchar(max) null;
		END
		ELSE
		BEGIN
			ALTER TABLE [RGPDTreatmentsLogs]
			ADD [Comments] varchar(max) null;
		END
	END
	
	IF NOT EXISTS (SELECT 1
				FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
				WHERE sys.tables.name like 'RGPDTreatmentsLogs' and syscolumns.name like 'Comments_HTML' )
	BEGIN
		ALTER TABLE [RGPDTreatmentsLogs]
		ADD [Comments_HTML] bit null;
	END
	
	
	
	IF NOT EXISTS (SELECT 1
				FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
				WHERE sys.tables.name like 'RGPDTreatmentsLogs' and syscolumns.name like 'TabID' )
	BEGIN
		ALTER TABLE [RGPDTreatmentsLogs]
		ADD [TabID] varchar(512) null;
	END
	
	
	IF NOT EXISTS (SELECT 1
				FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
				WHERE sys.tables.name like 'RGPDTreatmentsLogs' and syscolumns.name like 'FieldsID' )
	BEGIN
		ALTER TABLE [RGPDTreatmentsLogs]
		ADD [FieldsID] varchar(512) null;
	END
	
	IF NOT EXISTS (SELECT 1
				FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
				WHERE sys.tables.name like 'RGPDTreatmentsLogs' and syscolumns.name like 'RGPDFieldsID' )
	BEGIN
		ALTER TABLE [RGPDTreatmentsLogs]
		ADD [RGPDFieldsID] varchar(512) null;
	END
	
	IF NOT EXISTS (SELECT 1
				FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
				WHERE sys.tables.name like 'RGPDTreatmentsLogs' and syscolumns.name like 'PersonnalDataCategoriesID' )
	BEGIN
		ALTER TABLE [RGPDTreatmentsLogs]
		ADD [PersonnalDataCategoriesID] varchar(512) null;
	END
	
	IF NOT EXISTS (SELECT 1
				FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
				WHERE sys.tables.name like 'RGPDTreatmentsLogs' and syscolumns.name like 'SensibleDataID' )
	BEGIN
		ALTER TABLE [RGPDTreatmentsLogs]
		ADD [SensibleDataID] varchar(512) null;
	END
	
	IF NOT EXISTS (SELECT 1
				FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
				WHERE sys.tables.name like 'RGPDTreatmentsLogs' and syscolumns.name like 'PeopleCategoriesID' )
	BEGIN
		ALTER TABLE [RGPDTreatmentsLogs]
		ADD [PeopleCategoriesID] varchar(512) null;
	END
	
	--Modif temporaire
	IF NOT EXISTS (SELECT 1
				FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
				WHERE sys.tables.name like 'RGPDTreatmentsLogs' and syscolumns.name like 'RGPDTreatmentsLogs91' )
	BEGIN
		ALTER TABLE [RGPDTreatmentsLogs]
		ADD [RGPDTreatmentsLogs91] int null;
	END
	
	
END