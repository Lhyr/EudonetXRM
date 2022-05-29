--Tâche 4 328
--Ajout des colonnes dans la table CAMPAIGN

IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[CAMPAIGN]') 
         AND name = 'OptInEnabled'
)
BEGIN
Alter Table [CAMPAIGN] 
Add OptInEnabled bit DEFAULT 1
end

IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[CAMPAIGN]') 
         AND name = 'OptOutEnabled'
)
BEGIN
Alter Table [CAMPAIGN] 
Add OptOutEnabled bit DEFAULT 1
end

IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[CAMPAIGN]') 
         AND name = 'NoConsentEnabled'
)
BEGIN
Alter Table [CAMPAIGN] 
Add NoConsentEnabled bit DEFAULT 0
end

UPDATE [RES] SET LANG_00 =  'Type de campagne', LANG_01 = 'Type of campaign', LANG_02 = 'Type of campaign', LANG_03 = 'Type of campaign', LANG_04 = 'Type of campaign', LANG_05 = 'Type of campaign'  WHERE RESID = 106008

