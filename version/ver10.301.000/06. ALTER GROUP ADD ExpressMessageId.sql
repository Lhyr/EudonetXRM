IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[GROUP]') 
         AND name = 'ExpressMessageId'
)
BEGIN
ALTER TABLE dbo.[GROUP] ADD ExpressMessageId numeric(18,0) NULL;  
END
