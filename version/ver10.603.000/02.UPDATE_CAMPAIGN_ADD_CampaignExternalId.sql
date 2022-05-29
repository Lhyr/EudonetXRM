--Tâche 2 480
--Ajout de la colonne CampaignExternalId dans la table CAMPAIGN

IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[CAMPAIGN]') 
         AND name = 'CampaignExternalId'
)
BEGIN
Alter Table [CAMPAIGN] 
Add CampaignExternalId Varchar(50)
end


