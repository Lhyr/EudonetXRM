IF NOT EXISTS(SELECT * FROM SYSOBJECTS where id = object_id(N'[dbo].[XrmWidgetParam]') and OBJECTPROPERTY(id, N'IsUserTable') = 1 )
BEGIN
	-- Création de la table XrmWidgetParam si elle n'existe pas
	
	CREATE TABLE [dbo].[XrmWidgetParam] (
		[XrmWidgetParamId] [int] IDENTITY(1, 1) NOT NULL
		,[XrmWidgetId] [numeric](18, 0) NOT NULL
		,[ParamName] [varchar](50) NOT NULL
		,[ParamValue] [varchar](max) NULL
		,CONSTRAINT [PK_XrmWidgetParam] PRIMARY KEY CLUSTERED ([XrmWidgetParamId] ASC) WITH (
			PAD_INDEX = OFF
			,STATISTICS_NORECOMPUTE = OFF
			,IGNORE_DUP_KEY = OFF
			,ALLOW_ROW_LOCKS = ON
			,ALLOW_PAGE_LOCKS = ON
			) ON [PRIMARY]
		) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

	ALTER TABLE [dbo].[XrmWidgetParam] WITH NOCHECK ADD CONSTRAINT [FK_XrmWidgetParam_XrmWidget] FOREIGN KEY ([XrmWidgetId]) REFERENCES [dbo].[XrmWidget]([XrmWidgetId]) ON DELETE CASCADE 
	ALTER TABLE [dbo].[XrmWidgetParam] CHECK CONSTRAINT [FK_XrmWidgetParam_XrmWidget]

	-- Remplissage de la table avec les données existantes de XrmWidget.ContentParam
	
	DECLARE @id AS NUMERIC
	DECLARE @param AS VARCHAR(MAX)

	DECLARE Curs CURSOR
	FOR
	SELECT XrmWidgetId, ContentParam FROM [XrmWidget]

	OPEN Curs

	FETCH Curs
	INTO @id, @param

	WHILE @@Fetch_status = 0
	BEGIN
		  INSERT INTO [XrmWidgetParam] (XrmWidgetId, ParamName, ParamValue) 
		  SELECT @id AS [WidgetId], LOWER(SUBSTRING(ELEMENT, 0, CHARINDEX('=', ELEMENT))) AS [Param], SUBSTRING(ELEMENT, CHARINDEX('=', ELEMENT) + 1, LEN(ELEMENT)) AS [Value]
		  FROM [dbo].[efc_splitCTE](@param, '&')
		  WHERE ELEMENT IS NOT NULL

		  FETCH Curs
		  INTO @id, @param
	END

	CLOSE Curs

	DEALLOCATE Curs
	
END
