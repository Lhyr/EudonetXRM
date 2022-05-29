IF NOT EXISTS(SELECT * FROM SYSOBJECTS where id = object_id(N'[dbo].[RGPDTreatmentsLogs]') and OBJECTPROPERTY(id, N'IsUserTable') = 1 )
BEGIN
	-- Création de la table RGPDTreatmentsLogs si elle n'existe pas
	
	CREATE TABLE [dbo].[RGPDTreatmentsLogs] (
			[RGPDTreatmentsLogsID] [int] IDENTITY(1,1) NOT NULL
			,[LogDate] [datetime] NULL
			,[RGPDType] [int] NULL
			,[NbMonthsDeadline] [int] NULL
			,[Tab] [numeric](18,0) NULL
			,[Fields] varchar(max) NULL
			,[NbRecords] [int] NULL
			,[TabLabel] VARCHAR(250) NULL
			,[FieldsLabel] VARCHAR(MAX) NULL
			,[DeadlineLabel] VARCHAR(150) NULL
			,[RGPDTypeLabel] VARCHAR(50) NULL
			,RGPDTreatmentsLogs84 [bit] NULL,[RGPDTreatmentsLogs88] [varchar](1000) NULL, [RGPDTreatmentsLogs92] [varchar](1000) NULL, [RGPDTreatmentsLogs95] [datetime] NULL, [RGPDTreatmentsLogs96] [datetime] NULL, [RGPDTreatmentsLogs97] [numeric](18, 0) NULL, [RGPDTreatmentsLogs98] [numeric](18, 0) NULL, [RGPDTreatmentsLogs99] [numeric](18, 0) NULL
			,CONSTRAINT [PK_RGPDTreatmentsLogs] PRIMARY KEY CLUSTERED ([RGPDTreatmentsLogsID] ASC) WITH (
					PAD_INDEX = OFF
					,STATISTICS_NORECOMPUTE = OFF
					,IGNORE_DUP_KEY = OFF
					,ALLOW_ROW_LOCKS = ON
					,ALLOW_PAGE_LOCKS = ON
				) ON [PRIMARY]
		) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
	
	--On doit pouvoir supprimer les tables sans supprimer les logs, donc pas de foreign key sur Tab/Descid
	
END

-- CREATION DES DESC/RES

DECLARE @nTab INT = 117000

BEGIN
	DELETE [RES]
	WHERE ResId >= @nTab AND ResId < @nTab + 100

	DELETE [DESCADV]
	WHERE DescID >= @nTab AND DescID < @nTab + 100
	
	DELETE [DESC]
	WHERE DescID >= @nTab AND DescID < @nTab + 100

	-- Table RGPDTreatmentsLogs
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length], [type], [ActiveTab], [BreakLine])
	SELECT @nTab, 'RGPDTreatmentsLogs', 'RGPDTreatmentsLogs', 0, 0, 0, 0, 25

	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05)
	SELECT @nTab, 'Journal des traitements RGPD', 'RGPD Treatments logs', 'RGPD Treatments logs', 'RGPD Treatments logs', 'RGPD Treatments logs', 'RGPD Treatments logs'

	-- LogDate
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length])
	SELECT @nTab + 1, 'RGPDTreatmentsLogs', 'LogDate', 2, 0

	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05)
	SELECT @nTab + 1, 'Date', 'Date', 'Date', 'Date', 'Date', 'Date'

	-- Délai de mise en oeuvre
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length])
	SELECT @nTab + 2, 'RGPDTreatmentsLogs', 'DeadlineLabel', 1, 150

	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05)
	SELECT @nTab + 2, 'Délai de mise en oeuvre', 'Timing of implementation', 'Timing of implementation', 'Timing of implementation', 'Timing of implementation', 'Timing of implementation'

	-- Type d'opération
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length])
	SELECT @nTab + 3, 'RGPDTreatmentsLogs', 'RGPDTypeLabel', 1, 50

	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05)
	SELECT @nTab + 3, 'Type d''opération', 'Operation type', 'Operation type', 'Operation type', 'Operation type', 'Operation type'
	
	-- Onglet concerné
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length])
	SELECT @nTab + 4, 'RGPDTreatmentsLogs', 'TabLabel', 1, 50

	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05)
	SELECT @nTab + 4, 'Onglet concerné', 'Involved tab', 'Involved tab', 'Involved tab', 'Involved tab', 'Involved tab'
	
	-- Rubriques concernées
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length])
	SELECT @nTab + 5, 'RGPDTreatmentsLogs', 'FieldsLabel', 1, 500

	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05)
	SELECT @nTab + 5, 'Rubriques concernées', 'Involved fields', 'Involved fields', 'Involved fields', 'Involved fields', 'Involved fields'
	
	-- Nombre de fiches
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length])
	SELECT @nTab + 6, 'RGPDTreatmentsLogs', 'NbRecords', 10, 0

	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05)
	SELECT @nTab + 6, 'Nb de fiches', 'Records number', 'Records number', 'Records number', 'Records number', 'Records number'
	
	-- Champs système
	-- 84 et 84_res
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length], [disporder])
	SELECT @nTab + 84, 'RGPDTreatmentsLogs', 'RGPDTreatmentsLogs84', 3, 0, 84

	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05)
	SELECT @nTab + 84, 'Confidentielle', 'Confidential', 'Confidential', 'Confidential', 'Confidential', 'Confidential'


	-- 88 et 88_res
	INSERT INTO [desc] ([DescId], [File], [Field], [Format], [Length], [disporder])
	SELECT @nTab + 88, 'RGPDTreatmentsLogs', 'RGPDTreatmentsLogs88', 8, 0, 88

	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05)
	SELECT @nTab + 88, 'Journal de', 'Log of', 'Log of', 'Log of', 'Log of', 'Log of'

	-- 95 et 95_res
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length])
	SELECT @nTab + 95, 'RGPDTreatmentsLogs', 'RGPDTreatmentsLogs95', 2, 0

	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05)
	SELECT @nTab + 95, 'Créé le', 'Created on', 'Created on', 'Created on', 'Created on', 'Created on'

	-- 96 et 96_res
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length])
	SELECT @nTab + 96, 'RGPDTreatmentsLogs', 'RGPDTreatmentsLogs96', 2, 0

	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05)
	SELECT @nTab + 96, 'Modifié le', 'Modified on', 'Modified on', 'Modified on', 'Modified on', 'Modified on'

	-- 97 et 97_res
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length])
	SELECT @nTab + 97, 'RGPDTreatmentsLogs', 'RGPDTreatmentsLogs97', 8, 0

	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05)
	SELECT @nTab + 97, 'Créé par', 'Created by', 'Created by', 'Created by', 'Created by', 'Created by'

	-- 98 et 98_res
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length])
	SELECT @nTab + 98, 'RGPDTreatmentsLogs', 'RGPDTreatmentsLogs98', 8, 0

	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05)
	SELECT @nTab + 98, 'Modifié par', 'Modified by', 'Modified by', 'Modified by', 'Modified by', 'Modified by'

	-- 99 et 99_res
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length])
	SELECT @nTab + 99, 'RGPDTreatmentsLogs', 'RGPDTreatmentsLogs99', 8, 0

	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05)
	SELECT @nTab + 99, 'Appartient à', 'Belongs to', 'Belongs to', 'Belongs to', 'Belongs to', 'Belongs to'
	
	--Pref/selections
	
	DELETE
	FROM selections
	WHERE tab = @nTab

	DELETE
	FROM pref
	WHERE tab = @nTab

	INSERT INTO PREF (tab, userid, listcol)
	SELECT @nTab, 0, '117001;117002;117003;117004;117005;117006'

	INSERT INTO PREF (tab, userid, listcol)
	SELECT @nTab, userid, '117001;117002;117003;117004;117005;117006'
	FROM [USER]
	
	INSERT INTO SELECTIONS (tab, label, listcol, listcolwidth, userid)
	SELECT @nTab, 'Vue par défaut', '117001;117002;117003;117004;117005;117006', '0;0;0;0', 0

	INSERT INTO SELECTIONS (tab, label, listcol, listcolwidth, userid)
	SELECT @nTab, 'Vue par défaut', '117001;117002;117003;117004;117005;117006', '0;0;0;0', userid
	FROM [USER]

	UPDATE pref
	SET selectid = selections.selectid
	FROM [PREF]
	INNER JOIN selections ON selections.userid = [pref].userid AND [selections].[tab] = [pref].[tab]
	WHERE [PREF].[tab] = @nTab
	
	IF NOT EXISTS (SELECT 1 FROM [DESCADV] WHERE Parameter = 8 AND descid = @nTab)
	BEGIN
		INSERT INTO [DESCADV] (Value, Parameter, Category, DescId)
		SELECT 1, 8, 2, @nTab
	END
	ELSE
	BEGIN
		UPDATE [DESCADV] SET Value = 1 WHERE [DESCID] = @nTab AND Parameter = 8
	END
END