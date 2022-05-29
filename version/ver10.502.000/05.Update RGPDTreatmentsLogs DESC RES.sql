DECLARE @nTab INT = 117000
DECLARE @sFile VARCHAR(20) = 'RGPDTreatmentsLogs'


/* Creation des nouveau champs */
--Nettoyage
DELETE FROM [RES] WHERE [ResId] IN (@nTab + 33, @nTab + 34)
DELETE FROM [DESC] WHERE [DescId] IN (@nTab + 33, @nTab + 34)

--DESC
INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) VALUES (@nTab + 33, @sFile, 'EndDate', 2, 0)
INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) VALUES (@nTab + 34, @sFile, 'Status', 10, 0)

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
	WHERE [DescId] IN (@nTab + 33, @nTab + 34)	
	


--RES
INSERT INTO [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05]) VALUES (@nTab + 33, 'Date de fin', 'End date', 'Enddatum', 'Einddatum', 'Fecha de fin', 'Data di fine')
INSERT INTO [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05]) VALUES (@nTab + 34, 'Statut', 'Status', 'Status', 'Statuut', 'Estatuto', 'Statuto')




--Mise à jour de la mise en page
UPDATE [DESC] SET [DispOrder] = 1, [Colspan] = 3, [RowSpan] = 0 WHERE [DescId] = @nTab + 1 --Libellé
UPDATE [DESC] SET [DispOrder] = 2, [Colspan] = 0, [RowSpan] = 0 WHERE [DescId] = @nTab + 31 --Date
UPDATE [DESC] SET [DispOrder] = 3, [Colspan] = 0, [RowSpan] = 0 WHERE [DescId] = @nTab + 2 --Délai d'effacement
UPDATE [DESC] SET [DispOrder] = 4, [Colspan] = 0, [RowSpan] = 0 WHERE [DescId] = @nTab + 7 --Type d'opération
UPDATE [DESC] SET [Disporder] = 5, [Colspan] = 0, [RowSpan] = 0 WHERE [DescId] = @nTab + 33 --Date de fin
--Champ Libre
UPDATE [DESC] SET [Disporder] = 7, [Colspan] = 0, [RowSpan] = 0 WHERE [DescId] = @nTab + 34 --Statut
UPDATE [DESC] SET [DispOrder] = 8, [Colspan] = 0, [RowSpan] = 0 WHERE [DescId] = @nTab + 22 --Onglets concernés
UPDATE [DESC] SET [DispOrder] = 9, [Colspan] = 0, [RowSpan] = 0 WHERE [DescId] = @nTab + 23 --Rubriques concernées
UPDATE [DESC] SET [DispOrder] = 10, [Colspan] = 0, [RowSpan] = 0 WHERE [DescId] = @nTab + 6 --Nb de fiches
UPDATE [DESC] SET [DispOrder] = 11, [Colspan] = 0, [RowSpan] = 0 WHERE [DescId] = @nTab + 9 --Référence
UPDATE [DESC] SET [DispOrder] = 12, [Colspan] = 0, [RowSpan] = 0 WHERE [DescId] = @nTab + 8 --RGPD
--Champ Libre
UPDATE [DESC] SET [DispOrder] = 14, [Colspan] = 0, [RowSpan] = 0 WHERE [DescId] = @nTab + 27 --Catégorie de personnes
UPDATE [DESC] SET [DispOrder] = 15, [Colspan] = 0, [RowSpan] = 0 WHERE [DescId] = @nTab + 25 --Catégorie données personnelles
UPDATE [DESC] SET [DispOrder] = 16, [Colspan] = 0, [RowSpan] = 0 WHERE [DescId] = @nTab + 26 --Données sensibles
UPDATE [DESC] SET [DispOrder] = 17, [Colspan] = 0, [RowSpan] = 0 WHERE [DescId] = @nTab + 28 --Responsable du traitement
UPDATE [DESC] SET [DispOrder] = 18, [Colspan] = 0, [RowSpan] = 0 WHERE [DescId] = @nTab + 29 --Délégué à la protection des données
UPDATE [DESC] SET [DispOrder] = 19, [Colspan] = 0, [RowSpan] = 0 WHERE [DescId] = @nTab + 30 --Représentant
UPDATE [DESC] SET [DispOrder] = 20, [Colspan] = 0, [RowSpan] = 0 WHERE [DescId] = @nTab + 19 --Destinataires
UPDATE [DESC] SET [DispOrder] = 21, [Colspan] = 0, [RowSpan] = 0 WHERE [DescId] = @nTab + 14 --Finalité du traitement
UPDATE [DESC] SET [DispOrder] = 22, [Colspan] = 0, [RowSpan] = 0 WHERE [DescId] = @nTab + 15 --Mesure de sécurité
UPDATE [DESC] SET [DispOrder] = 23, [Colspan] = 2, [RowSpan] = 2 WHERE [DescId] = @nTab + 21 --Commentaires
UPDATE [DESC] SET [DispOrder] = 24, [Colspan] = 0, [RowSpan] = 2 WHERE [DescId] = @nTab + 20 --Transferts Hors UE
UPDATE [DESC] SET [DispOrder] = 25, [Colspan] = 3, [RowSpan] = 0 WHERE [DescId] = @nTab + 32 --Informations complémentaires (Titre Séparateur)
UPDATE [DESC] SET [DispOrder] = 26, [Colspan] = 0, [RowSpan] = 0 WHERE [DescId] = @nTab + 4 --Liste des onglets concernés
UPDATE [DESC] SET [DispOrder] = 27, [Colspan] = 0, [RowSpan] = 0 WHERE [DescId] = @nTab + 5 --Liste des rubriques concernées
UPDATE [DESC] SET [DispOrder] = 28, [Colspan] = 0, [RowSpan] = 1 WHERE [DescId] = @nTab + 10 --Liste des rubriques RGPD concernées
UPDATE [DESC] SET [DispOrder] = 29, [Colspan] = 0, [RowSpan] = 0 WHERE [DescId] = @nTab + 18 --Liste des catégories de personnes
UPDATE [DESC] SET [DispOrder] = 30, [Colspan] = 0, [RowSpan] = 0 WHERE [DescId] = @nTab + 11 --Liste des responsables du traitement
UPDATE [DESC] SET [DispOrder] = 31, [Colspan] = 0, [RowSpan] = 0 WHERE [DescId] = @nTab + 12 --Liste des délégués à la protection des données
UPDATE [DESC] SET [DispOrder] = 32, [Colspan] = 0, [RowSpan] = 0 WHERE [DescId] = @nTab + 13 --Liste des représentants



