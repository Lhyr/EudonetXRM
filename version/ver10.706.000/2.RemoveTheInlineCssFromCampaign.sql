--Tâche 4 759
--Remove the inline CSS field switch from Campaign
DECLARE @idMailTemplateInlineCSS INT = 107013
DECLARE @nameContraint nvarchar(128)

IF  EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[CAMPAIGN]') 
         AND name = 'InlineCss'
)
BEGIN

          SELECT @nameContraint = name
          FROM sys.default_constraints
          WHERE parent_object_id = object_id(N'dbo.CAMPAIGN')
          AND col_name(parent_object_id, parent_column_id) = 'InlineCss';
          IF @nameContraint IS NOT NULL
              EXECUTE('ALTER TABLE [dbo].[CAMPAIGN] DROP CONSTRAINT [' + @nameContraint + ']')

Alter Table [CAMPAIGN] 
DROP COLUMN  InlineCss;
end

IF  EXISTS (
  SELECT * 
  FROM   sys.columns 
  WHERE  object_id = OBJECT_ID(N'[dbo].[MAILTEMPLATE]') 
         AND name = 'InlineCss'
)
BEGIN

          SELECT @nameContraint = name
          FROM sys.default_constraints
          WHERE parent_object_id = object_id(N'dbo.MAILTEMPLATE')
          AND col_name(parent_object_id, parent_column_id) = 'InlineCss';
          IF @nameContraint IS NOT NULL
              EXECUTE('ALTER TABLE [dbo].[MAILTEMPLATE] DROP CONSTRAINT [' + @nameContraint + ']')

Alter Table [MAILTEMPLATE] 
DROP COLUMN  InlineCss;
end

IF EXISTS (
		SELECT *
		FROM [DESC]
		WHERE [DescId] = @idMailTemplateInlineCSS
		)
BEGIN
	DELETE FROM [DESC] WHERE [DescId] = @idMailTemplateInlineCSS
END

IF EXISTS (
		SELECT *
		FROM [RES]
		WHERE [RESID] = @idMailTemplateInlineCSS
		)
BEGIN
	DELETE FROM [RES] WHERE [RESID] = @idMailTemplateInlineCSS
END


