IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[USER]') 
         AND name = 'USER_CODE' 
)
BEGIN
	ALTER TABLE [USER] Add USER_CODE varchar(6)
END 

IF NOT EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[USER]') 
         AND name = 'CODE_CREATION_DATE' 
)
BEGIN
	ALTER TABLE [USER] Add CODE_CREATION_DATE datetime
END 