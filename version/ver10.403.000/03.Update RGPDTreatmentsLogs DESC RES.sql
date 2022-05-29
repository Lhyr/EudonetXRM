DECLARE @nTab INT = 117000
DECLARE @sFile VARCHAR(20) = 'RGPDTreatmentsLogs'

--Modifications rubrique DateTime
DELETE FROM [RES] WHERE [ResId] = @nTab + 1

UPDATE [DESC] SET
[DescId] = @nTab + 31
WHERE [DescId] = @nTab + 1
AND [Field] = 'LogDate'

UPDATE [DESC] SET
[Format] = 2
,[Length] = 0
,[Default] = '<DATETIME>'
WHERE [DescId] = @nTab + 31

IF NOT EXISTS (SELECT 1 FROM [RES] WHERE [ResId] = @nTab + 31)
BEGIN
	INSERT INTO [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05]) VALUES (@nTab + 31, 'Date', 'Date', 'Date', 'Date', 'Date', 'Date')
END

--Création Rubrique 01
IF NOT EXISTS (SELECT 1 FROM [DESC] WHERE [DescId] = @nTab + 1)
BEGIN
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) VALUES (@nTab + 1, @sFile, 'LogLabel', 1, 150)
END

IF NOT EXISTS (SELECT 1 FROM [RES] WHERE [ResId] = @nTab + 1)
BEGIN
	INSERT INTO [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05]) VALUES (@nTab + 1, 'Libellé', 'Label', 'Label', 'Label', 'Label', 'Label')
END


--Modification rubrique DeadlineLabel
DELETE FROM [RES] WHERE [ResId] = @nTab + 2
INSERT INTO [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05])
VALUES (@nTab + 2, 'Délai d''effacement', 'Period before deletion', 'Period before deletion', 'Period before deletion', 'Period before deletion', 'Period before deletion')

--Modification rubrique RGPDTypeLabel
/*
UPDATE [DESC] SET
[Length] = 100
WHERE [DescId] = @nTab + 3
*/
DELETE FROM [DESC] WHERE [DescId] = @nTab + 3
DELETE FROM [RES] WHERE [ResId] = @nTab + 3


--Modification rubrique TabLabel
UPDATE [DESC] SET
[Format] = 9
,[Length] = 8000
WHERE [DescId] = @nTab + 4

DELETE FROM [RES] WHERE [ResId] = @nTab + 4
INSERT INTO [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05])
VALUES (@nTab + 4, 'Onglets concernés', 'Involved tabs', 'Involved tabs', 'Involved tabs', 'Involved tabs', 'Involved tabs')

--Modification rubrique FieldsLabel
UPDATE [DESC] SET
[Format] = 9
,[Length] = 8000
WHERE [DescId] = @nTab + 5


/* Creation des nouveau champs */
--Nettoyage
DELETE FROM [RES] WHERE [ResId] BETWEEN @nTab + 7 AND @nTab + 30
DELETE FROM [DESC] WHERE [DescId] BETWEEN @nTab + 7 AND @nTab + 30

--DESC
INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) VALUES (@nTab + 7, @sFile, 'RGPDType', 10, 0)
INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) VALUES (@nTab + 8, @sFile, 'IsRGPD', 3, 0)
INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) VALUES (@nTab + 9, @sFile, 'ReferenceNumber', 10, 0)
INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) VALUES (@nTab + 10, @sFile, 'RGPDFieldsLabel', 9, 8000)
INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) VALUES (@nTab + 11, @sFile, 'TreatmentResponsibleLabel', 9, 8000)
INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) VALUES (@nTab + 12, @sFile, 'DataProtectionDelegateLabel', 9, 8000)
INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) VALUES (@nTab + 13, @sFile, 'RepresentativeLabel', 9, 8000)
INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) VALUES (@nTab + 14, @sFile, 'TreatmentPurpose', 9, 8000)
INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) VALUES (@nTab + 15, @sFile, 'SecurityMeasure', 1, 500)
--INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) VALUES (@nTab + 16, @sFile, 'PersonnalDataCategories', 9, 8000)
--INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) VALUES (@nTab + 17, @sFile, 'SensibleData', 9, 8000)
INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) VALUES (@nTab + 18, @sFile, 'PeopleCategories', 9, 8000)
INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) VALUES (@nTab + 19, @sFile, 'Recipients', 1, 500)
INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) VALUES (@nTab + 20, @sFile, 'OutsideEUTransferts', 9, 8000)
INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) VALUES (@nTab + 21, @sFile, 'Comments', 9, 8000)

INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length], [Popup]) VALUES (@nTab + 22, @sFile, 'TabID', 1, 512, 5)
INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length], [Popup]) VALUES (@nTab + 23, @sFile, 'FieldsID', 1, 512, 5)
INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) VALUES (@nTab + 24, @sFile, 'RGPDFieldsID', 1, 512)
INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) VALUES (@nTab + 25, @sFile, 'PersonnalDataCategoriesID', 1, 512)
INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) VALUES (@nTab + 26, @sFile, 'SensibleDataID', 1, 512)
INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) VALUES (@nTab + 27, @sFile, 'PeopleCategoriesID', 1, 512)
INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) VALUES (@nTab + 28, @sFile, 'TreatmentResponsibleID', 8, 0)
INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) VALUES (@nTab + 29, @sFile, 'DataProtectionDelegateID', 8, 0)
INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length]) VALUES (@nTab + 30, @sFile, 'RepresentativeID', 8, 0)

UPDATE [DESC] SET 
[Multiple] = 0
WHERE [DescId] BETWEEN @nTab + 8 AND @nTab + 30

UPDATE [DESC] SET 
[Multiple] = 1
WHERE [DescId] IN (@nTab + 22, @nTab + 23, @nTab + 24, @nTab + 25, @nTab + 26,  @nTab + 27, @nTab + 28, @nTab + 29, @nTab + 30)

-- Affichage
UPDATE [DESC] SET Columns = '150,A,25,150,A,25,150,A,25' where [DescId] = 117000
UPDATE [DESC] SET 
	[Disporder] = [DescId] % 100
	WHERE [DescId] > 117000 AND [DescId] < 117100
	
UPDATE [DESC] SET [Disporder] = 1, [ColSpan] = 3, [ReadOnly] = 1 where [DescId] = 117001

--UPDATE [DESC] SET Disporder = 1 where [DescId] = 117001
UPDATE [DESC] SET [Disporder] = 2 where [DescId] = 117031
UPDATE [DESC] SET [Disporder] = 3 where [DescId] = 117002
UPDATE [DESC] SET [Disporder] = 4, [Obligat] = 1 where [DescId] = 117007

UPDATE [DESC] SET [Disporder] = 5, [Obligat] = 1, [RowSpan] = 1 where [DescId] = 117022
UPDATE [DESC] SET [Disporder] = 6, [RowSpan] = 1 where [DescId] = 117023
UPDATE [DESC] SET [Disporder] = 7, [ReadOnly] = 1, [RowSpan] = 1 where [DescId] = 117010

UPDATE [DESC] SET [Disporder] = 8, [ReadOnly] = 1 where [DescId] = 117009
UPDATE [DESC] SET [Disporder] = 9, [ReadOnly] = 1 where [DescId] = 117008
UPDATE [DESC] SET [Disporder] = 10, [ReadOnly] = 0 where [DescId] = 117006

UPDATE [DESC] SET [Disporder] = 11, [ReadOnly] = 1 where [DescId] = 117028
UPDATE [DESC] SET [Disporder] = 12, [ReadOnly] = 1 where [DescId] = 117029
UPDATE [DESC] SET [Disporder] = 13, [ReadOnly] = 1 where [DescId] = 117030

UPDATE [DESC] SET [Disporder] = 14, [ReadOnly] = 1 where [DescId] = 117027
UPDATE [DESC] SET [Disporder] = 15, [ReadOnly] = 1 where [DescId] = 117014
--UPDATE [DESC] SET [Disporder] = 16, [ReadOnly] = 1 where [DescId] = 117016


UPDATE [DESC] SET [Disporder] = 17, [ReadOnly] = 0 where [DescId] = 117019
UPDATE [DESC] SET [Disporder] = 18, [ReadOnly] = 0, [RowSpan] = 2 where [DescId] = 117021
UPDATE [DESC] SET [Disporder] = 19, [ReadOnly] = 0, [RowSpan] = 2 where [DescId] = 117020

UPDATE [DESC] SET [Disporder] = 20, [ReadOnly] = 0 where [DescId] = 117015
UPDATE [DESC] SET [Disporder] = 21, [ReadOnly] = 1 where [DescId] = 117004
UPDATE [DESC] SET [Disporder] = 22, [ReadOnly] = 1 where [DescId] = 117005

--UPDATE [DESC] SET [Disporder] = 23, [ReadOnly] = 1 where [DescId] = 117017
UPDATE [DESC] SET [Disporder] = 24, [ReadOnly] = 1 where [DescId] = 117018
UPDATE [DESC] SET [Disporder] = 25, [ReadOnly] = 1 where [DescId] = 117011

UPDATE [DESC] SET [Disporder] = 26, [ReadOnly] = 1 where [DescId] = 117012
UPDATE [DESC] SET [Disporder] = 27, [ReadOnly] = 1 where [DescId] = 117013

UPDATE [DESC] SET [Disporder] = 16, [ReadOnly] = 1 where [DescId] = 117025
UPDATE [DESC] SET [Disporder] = 23, [ReadOnly] = 1 where [DescId] = 117026

UPDATE [DESC] SET [Disporder] = NULL where [DescId] = 117024

--RES
UPDATE [RES] SET 
[LANG_00] = 'Liste des onglets concernés'
,[LANG_01] = 'List of involved tabs'
,[LANG_02] = 'List of involved tabs'
,[LANG_03] = 'List of involved tabs'
,[LANG_04] = 'List of involved tabs'
,[LANG_05] = 'List of involved tabs'
WHERE [ResId] = @nTab + 4

UPDATE [RES] SET 
[LANG_00] = 'Liste des rubriques concernées'
,[LANG_01] = 'List of  involved fields'
,[LANG_02] = 'List of  involved fields'
,[LANG_03] = 'List of  involved fields'
,[LANG_04] = 'List of  involved fields'
,[LANG_05] = 'List of  involved fields'
WHERE [ResId] = @nTab + 5

INSERT INTO [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05]) VALUES (@nTab + 7, 'Type d''opération', 'Operation type', 'Operation type', 'Operation type', 'Operation type', 'Operation type')
INSERT INTO [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05]) VALUES (@nTab + 8, 'RGPD', 'GDPR', 'GDPR', 'GDPR', 'GDPR', 'GDPR')
INSERT INTO [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05]) VALUES (@nTab + 9, 'Référence', 'Reference', 'Reference', 'Reference', 'Reference', 'Reference')
INSERT INTO [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05]) VALUES (@nTab + 10, 'Liste des rubriques RGPD concernées', 'List of involved GDPR fields', 'List of involved GDPR fields', 'List of involved GDPR fields', 'List of involved GDPR fields', 'List of involved GDPR fields')
INSERT INTO [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05]) VALUES (@nTab + 11, 'Liste des responsables du traitement', 'List of processing managers', 'List of processing managers', 'List of processing managers', 'List of processing managers', 'List of processing managers')
INSERT INTO [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05]) VALUES (@nTab + 12, 'Liste des délégués à la protection des données', 'List of data protection officers', 'List of data protection officers', 'List of data protection officers', 'List of data protection officers', 'List of data protection officers')
INSERT INTO [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05]) VALUES (@nTab + 13, 'Liste des représentants', 'List of representatives', 'List of representatives', 'List of representatives', 'List of representatives', 'List of representatives')
INSERT INTO [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05]) VALUES (@nTab + 14, 'Finalité du traitement', 'Purpose of processing', 'Purpose of processing', 'Purpose of processing', 'Purpose of processing', 'Purpose of processing')
INSERT INTO [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05]) VALUES (@nTab + 15, 'Mesure de sécurité', 'Security measure', 'Security measure', 'Security measure', 'Security measure', 'Security measure')
--INSERT INTO [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05]) VALUES (@nTab + 16, 'Catégorie données personnelles (texte)', 'Personal data category (text)', 'Personal data category (text)', 'Personal data category (text)', 'Personal data category (text)', 'Personal data category (text)')
--INSERT INTO [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05]) VALUES (@nTab + 17, 'Données sensibles (texte)', 'Sensitive data (text)', 'Sensitive data (text)', 'Sensitive data (text)', 'Sensitive data (text)', 'Sensitive data (text)')
INSERT INTO [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05]) VALUES (@nTab + 18, 'Liste des catégories de personnes', 'List of categories of individuals', 'List of categories of individuals', 'List of categories of individuals', 'List of categories of individuals', 'List of categories of individuals')
INSERT INTO [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05]) VALUES (@nTab + 19, 'Destinataires', 'Recipients', 'Recipients', 'Recipients', 'Recipients', 'Recipients')
INSERT INTO [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05]) VALUES (@nTab + 20, 'Transferts Hors UE', 'Transfers outside of EU', 'Transfers outside of EU', 'Transfers outside of EU', 'Transfers outside of EU', 'Transfers outside of EU')
INSERT INTO [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05]) VALUES (@nTab + 21, 'Commentaires', 'Notes', 'Notes', 'Notes', 'Notes', 'Notes')

INSERT INTO [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05]) VALUES (@nTab + 22, 'Onglets concernés', 'Involved tabs', 'Involved tabs', 'Involved tabs', 'Involved tabs', 'Involved tabs')
INSERT INTO [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05]) VALUES (@nTab + 23, 'Rubriques concernées', 'Involved fields', 'Involved fields', 'Involved fields', 'Involved fields', 'Involved fields')
INSERT INTO [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05]) VALUES (@nTab + 24, 'Rubriques RGPD concernées', 'Involved GDPR fields', 'Involved GDPR fields', 'Involved GDPR fields', 'Involved GDPR fields', 'Involved GDPR fields')
INSERT INTO [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05]) VALUES (@nTab + 25, 'Catégorie données personnelles', 'Personal data category', 'Personal data category', 'Personal data category', 'Personal data category', 'Personal data category')
INSERT INTO [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05]) VALUES (@nTab + 26, 'Données sensibles', 'Sensitive data', 'Sensitive data', 'Sensitive data', 'Sensitive data', 'Sensitive data')
INSERT INTO [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05]) VALUES (@nTab + 27, 'Catégorie de personnes', 'Category of individuals', 'Category of individuals', 'Category of individuals', 'Category of individuals', 'Category of individuals')
INSERT INTO [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05]) VALUES (@nTab + 28, 'Responsable du traitement', 'Processing manager', 'Processing manager', 'Processing manager', 'Processing manager', 'Processing manager')
INSERT INTO [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05]) VALUES (@nTab + 29, 'Délégué à la protection des données', 'Data Protection Officer (DPO)', 'Data Protection Officer (DPO)', 'Data Protection Officer (DPO)', 'Data Protection Officer (DPO)', 'Data Protection Officer (DPO)')
INSERT INTO [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05]) VALUES (@nTab + 30, 'Représentant', 'Representative', 'Representative', 'Representative', 'Representative', 'Representative')


--Modif temporaire
/*
IF NOT EXISTS (SELECT 1 FROM [DESC] WHERE [DescId] = @nTab + 91)
BEGIN
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length], [DispOrder]) VALUES (@nTab + 91, @sFile, 'RGPDTreatmentsLogs91', 30, 0, 91)	
	
END

IF NOT EXISTS (SELECT 1 FROM [RES] WHERE [ResId] = @nTab + 91)
BEGIN
	INSERT INTO [RES] ([ResId], [LANG_00], [LANG_01], [LANG_02], [LANG_03], [LANG_04], [LANG_05]) VALUES (@nTab + 91, 'Annexes', '', '', '', '', '') --TODO Anglais
END
*/
