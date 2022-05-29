IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[CAMPAIGN]') 
         AND name = 'OnBreak' 
)
BEGIN
	ALTER TABLE [CAMPAIGN] Add OnBreak INT NOT NULL DEFAULT 0
END 
