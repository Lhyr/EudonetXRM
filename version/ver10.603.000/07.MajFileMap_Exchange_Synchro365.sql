IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[FILEMAP_EXCHANGE]') 
         AND name = 'ChangeKey'
)
BEGIN
	ALTER TABLE [FILEMAP_EXCHANGE] ADD ChangeKey NVARCHAR(100) NULL
end

IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[FILEMAP_EXCHANGE]') 
         AND name = 'ExchangeAccountUserId'
)
BEGIN
	ALTER TABLE [FILEMAP_EXCHANGE] ADD ExchangeAccountUserId INT NULL
end