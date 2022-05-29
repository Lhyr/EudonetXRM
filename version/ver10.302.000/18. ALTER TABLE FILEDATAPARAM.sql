-- Représentation étape sur un catalogue
IF NOT EXISTS ( SELECT 1
		  from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		  where sys.tables.name like 'FILEDATAPARAM' and syscolumns.name = 'StepMode')
BEGIN            
	ALTER TABLE [FILEDATAPARAM] ADD [StepMode] BIT DEFAULT NULL	
END

-- Attribut "Couleur" de l'étape sélectionnée pour un catalogue étape
IF NOT EXISTS ( SELECT 1
		  from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		  where sys.tables.name like 'FILEDATAPARAM' and syscolumns.name = 'SelectedValueColor')
BEGIN            
	ALTER TABLE [FILEDATAPARAM] ADD [SelectedValueColor] NVARCHAR(200) DEFAULT NULL	
END
 
-- Attribut "Suite" pour un catalogue étape
IF NOT EXISTS ( SELECT 1
		  from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		  where sys.tables.name like 'FILEDATAPARAM' and syscolumns.name = 'SequenceMode')
BEGIN            
	ALTER TABLE [FILEDATAPARAM] ADD [SequenceMode] BIT DEFAULT NULL	
END
 
