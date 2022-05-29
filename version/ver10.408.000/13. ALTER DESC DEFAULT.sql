-- MAB - #65 684 - Augmentation de la taille de la colonne DEFAULT de DESC pour les champs Mémo
ALTER TABLE [DESC]
ALTER COLUMN [DEFAULT] VARCHAR(MAX)