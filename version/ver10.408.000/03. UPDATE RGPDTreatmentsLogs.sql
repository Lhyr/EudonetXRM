DECLARE @tabRGPD AS INT = 117000

--Ajout Titre Séparateur
IF NOT EXISTS (SELECT 1
		FROM sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		WHERE sys.tables.name like 'RGPDTreatmentsLogs' and syscolumns.name like 'TitleAdditionalInformation' )
BEGIN
	ALTER TABLE [RGPDTreatmentsLogs] ADD [TitleAdditionalInformation] BIT NULL
END

IF NOT EXISTS(SELECT 1 FROM [DESC] WHERE [DescId] = @tabRGPD + 32)
BEGIN
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) SELECT @tabRGPD + 32, 'RGPDTreatmentsLogs', 'TitleAdditionalInformation', 15, 1
	
	UPDATE [DESC] SET
	[Case] = 0
	,[Historic] = 0
	,[Obligat] = 0
	,[Multiple] = 0
	,[Popup] = 0
	,[ActiveTab] = 0
	,[ActiveBkmPP] = 1
	,[ActiveBkmPM] = 1
	,[ActiveBkmEvent] = 1
	,[GetLevel] = 1
	,[ViewLevel] = 1
	,[UserLevel] = 0
	,[InterPP] = 0
	,[InterPM] = 0
	,[InterEvent] = 0
	,[TabIndex] = 0
	,[Bold] = 0
	,[Italic] = 0
	,[Underline] = 0
	,[Flat] = 0
	,[Disabled] = 0
	,[Unicode] = 0
	,[NbrBkmInline] = 0
	,[TreatmentMaxRows] = 0
	,[TreeViewUserList] = 0
	,[FullUserList] = 0
	,[BreakLine] = 0
	,[NoCascadePPPM] = 0
	,[NoCascadePMPP] = 0
	,[AutoCompletion] = 0
	where [DescId] = @tabRGPD + 32
END

IF NOT EXISTS(SELECT 1 FROM [RES] WHERE [ResId] = @tabRGPD + 32)
BEGIN
	INSERT INTO [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05]) select @tabRGPD + 32, 'Informations complémentaires', 'Additional information', 'Additional information', 'Additional information', 'Additional information', 'Additional information'
END




--Mise à jour de la mise en page
UPDATE [DESC] SET [Columns] = '210,A,25,225,A,25,285,A,25' WHERE [DescId] = @tabRGPD

UPDATE [DESC] SET [DispOrder] = 1, [Colspan] = 3, [RowSpan] = 0 WHERE [DescId] = @tabRGPD + 1 --Libellé
UPDATE [DESC] SET [DispOrder] = 2, [Colspan] = 0, [RowSpan] = 0 WHERE [DescId] = @tabRGPD + 31 --Date
UPDATE [DESC] SET [DispOrder] = 3, [Colspan] = 0, [RowSpan] = 0 WHERE [DescId] = @tabRGPD + 2 --Délai d'effacement
UPDATE [DESC] SET [DispOrder] = 4, [Colspan] = 0, [RowSpan] = 0 WHERE [DescId] = @tabRGPD + 7 --Type d'opération
UPDATE [DESC] SET [DispOrder] = 5, [Colspan] = 0, [RowSpan] = 0 WHERE [DescId] = @tabRGPD + 22 --Onglets concernés
UPDATE [DESC] SET [DispOrder] = 6, [Colspan] = 0, [RowSpan] = 0 WHERE [DescId] = @tabRGPD + 23 --Rubriques concernées
UPDATE [DESC] SET [DispOrder] = 7, [Colspan] = 0, [RowSpan] = 0 WHERE [DescId] = @tabRGPD + 6 --Nb de fiches
UPDATE [DESC] SET [DispOrder] = 8, [Colspan] = 0, [RowSpan] = 0 WHERE [DescId] = @tabRGPD + 9 --Référence
UPDATE [DESC] SET [DispOrder] = 9, [Colspan] = 0, [RowSpan] = 0 WHERE [DescId] = @tabRGPD + 8 --RGPD
--Champ Libre
UPDATE [DESC] SET [DispOrder] = 11, [Colspan] = 0, [RowSpan] = 0 WHERE [DescId] = @tabRGPD + 27 --Catégorie de personnes
UPDATE [DESC] SET [DispOrder] = 12, [Colspan] = 0, [RowSpan] = 0 WHERE [DescId] = @tabRGPD + 25 --Catégorie données personnelles
UPDATE [DESC] SET [DispOrder] = 13, [Colspan] = 0, [RowSpan] = 0 WHERE [DescId] = @tabRGPD + 26 --Données sensibles
UPDATE [DESC] SET [DispOrder] = 14, [Colspan] = 0, [RowSpan] = 0 WHERE [DescId] = @tabRGPD + 28 --Responsable du traitement
UPDATE [DESC] SET [DispOrder] = 15, [Colspan] = 0, [RowSpan] = 0 WHERE [DescId] = @tabRGPD + 29 --Délégué à la protection des données
UPDATE [DESC] SET [DispOrder] = 16, [Colspan] = 0, [RowSpan] = 0 WHERE [DescId] = @tabRGPD + 30 --Représentant
UPDATE [DESC] SET [DispOrder] = 17, [Colspan] = 0, [RowSpan] = 0 WHERE [DescId] = @tabRGPD + 19 --Destinataires
UPDATE [DESC] SET [DispOrder] = 18, [Colspan] = 0, [RowSpan] = 0 WHERE [DescId] = @tabRGPD + 14 --Finalité du traitement
UPDATE [DESC] SET [DispOrder] = 19, [Colspan] = 0, [RowSpan] = 0 WHERE [DescId] = @tabRGPD + 15 --Mesure de sécurité
UPDATE [DESC] SET [DispOrder] = 20, [Colspan] = 2, [RowSpan] = 2 WHERE [DescId] = @tabRGPD + 21 --Commentaires
UPDATE [DESC] SET [DispOrder] = 21, [Colspan] = 0, [RowSpan] = 2 WHERE [DescId] = @tabRGPD + 20 --Transferts Hors UE
UPDATE [DESC] SET [DispOrder] = 22, [Colspan] = 3, [RowSpan] = 0 WHERE [DescId] = @tabRGPD + 32 --Informations complémentaires (Titre Séparateur)
UPDATE [DESC] SET [DispOrder] = 23, [Colspan] = 0, [RowSpan] = 0 WHERE [DescId] = @tabRGPD + 4 --Liste des onglets concernés
UPDATE [DESC] SET [DispOrder] = 24, [Colspan] = 0, [RowSpan] = 0 WHERE [DescId] = @tabRGPD + 5 --Liste des rubriques concernées
UPDATE [DESC] SET [DispOrder] = 25, [Colspan] = 0, [RowSpan] = 1 WHERE [DescId] = @tabRGPD + 10 --Liste des rubriques RGPD concernées
UPDATE [DESC] SET [DispOrder] = 26, [Colspan] = 0, [RowSpan] = 0 WHERE [DescId] = @tabRGPD + 18 --Liste des catégories de personnes
UPDATE [DESC] SET [DispOrder] = 27, [Colspan] = 0, [RowSpan] = 0 WHERE [DescId] = @tabRGPD + 11 --Liste des responsables du traitement
UPDATE [DESC] SET [DispOrder] = 28, [Colspan] = 0, [RowSpan] = 0 WHERE [DescId] = @tabRGPD + 12 --Liste des délégués à la protection des données
UPDATE [DESC] SET [DispOrder] = 29, [Colspan] = 0, [RowSpan] = 0 WHERE [DescId] = @tabRGPD + 13 --Liste des représentants

--UPDATE [DESC] SET [DispOrder] = NULL, [Colspan] = 0, [RowSpan] = 0 WHERE [DescId] = @tabRGPD + 24 --Rubriques RGPD concernées
