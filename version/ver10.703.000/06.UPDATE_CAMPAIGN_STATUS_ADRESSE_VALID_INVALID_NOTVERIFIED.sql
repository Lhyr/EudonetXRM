--Tâche 4 400
--Ajout des colonnes dans la table CAMPAIGN (Adresse valide,Adresse invalide,Adresse non vérifiée)

IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[CAMPAIGN]') 
         AND name = 'ADRESSSTATUSPARAM'
)
BEGIN
Alter Table [CAMPAIGN] 
Add ADRESSSTATUSPARAM NVarchar(max)
end


