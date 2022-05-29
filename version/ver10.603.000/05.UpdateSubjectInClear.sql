DECLARE @tabName VARCHAR(100) ;
DECLARE @columnName VARCHAR(100);

SET @columnName = 'SubjectInClear';
SET @tabName = 'CAMPAIGN';

IF (EXISTS (SELECT * 
                 FROM sys.tables 
                 WHERE  name = @tabName))
BEGIN
    IF (EXISTS (SELECT * 
                 FROM sys.columns
                 WHERE object_id = OBJECT_ID(N'[dbo].'+@tabName) 
                  AND name = @columnName))
		BEGIN
			ALTER TABLE dbo.CAMPAIGN
			ALTER COLUMN SubjectInClear nvarchar(max) NULL;
		END
		
END
ELSE
RETURN;