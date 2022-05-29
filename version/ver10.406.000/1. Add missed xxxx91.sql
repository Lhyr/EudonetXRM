
-------------------------------------------  USER ----------------------------------------------------------------
-- Desc USER
DELETE FROM [DESC] WHERE DescId = 101091	
INSERT INTO [DESC]( DescId, [File], Field,Format) SELECT 101091,'USER','USER91', 30

-- Ressources 
DELETE FROM [RES] WHERE ResId = 101091
INSERT INTO RES ( ResId,LANG_00,LANG_01,LANG_02,LANG_03,LANG_04,LANG_05,LANG_06,LANG_07,LANG_08,LANG_09)
SELECT 101091,'Annexes', 'Attachments', 'Anlagen', 'Bijlagen', 'Anexos', 'Allegati', 'Attachments','Attachments', 'Attachments', 'Attachments'
		
-- Colonne SQL
IF NOT EXISTS (SELECT 1 FROM sys.tables INNER JOIN syscolumns ON syscolumns.id = sys.tables.object_id WHERE sys.tables.name LIKE 'USER' AND syscolumns.name LIKE 'USER91' )
	exec sp_executesql N'ALTER TABLE [USER] ADD [USER91] Int CONSTRAINT __DF__101091 DEFAULT 0'
	
-------------------------------------------  RGPDTreatmentsLogs ----------------------------------------------------------------		
-- DESC RGPDTreatmentsLogs
DELETE FROM [DESC] WHERE DescId = 117091	
INSERT INTO [DESC]( DescId, [File], Field,Format) SELECT 117091,'RGPDTreatmentsLogs','RGPDTreatmentsLogs91', 30

-- Ressources  RGPDTreatmentsLogs
DELETE FROM [RES] WHERE ResId = 117091
INSERT INTO RES ( ResId,LANG_00,LANG_01,LANG_02,LANG_03,LANG_04,LANG_05,LANG_06,LANG_07,LANG_08,LANG_09)
SELECT 117091,'Annexes', 'Attachments', 'Anlagen', 'Bijlagen', 'Anexos', 'Allegati', 'Attachments','Attachments', 'Attachments', 'Attachments'
		
-- Colonne SQL RGPDTreatmentsLogs
IF NOT EXISTS (SELECT 1 FROM sys.tables INNER JOIN syscolumns ON syscolumns.id = sys.tables.object_id WHERE sys.tables.name LIKE 'RGPDTreatmentsLogs' AND syscolumns.name LIKE 'RGPDTreatmentsLogs91' )
	exec sp_executesql N'ALTER TABLE [RGPDTreatmentsLogs] ADD [RGPDTreatmentsLogs91] Int CONSTRAINT __DF__117091 DEFAULT 0'