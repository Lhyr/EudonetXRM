SET NOCOUNT ON;

IF NOT EXISTS (
		SELECT *
		FROM SYSOBJECTS
		WHERE id = object_id(N'[dbo].[XrmProduct]') AND OBJECTPROPERTY(id, N'IsUserTable') = 1
		)
BEGIN
	CREATE TABLE [dbo].[XrmProduct] (
		XrmProductId INT IDENTITY(1, 1) NOT NULL,
		-- Code du produit
		ProductCode [varchar](250) NULL,
		-- Début plage
		RangeStart INT NULL,
		-- Fin plage
		RangeEnd INT NULL,
		-- Params systeme
		XrmProduct84 [bit] NULL, -- Confidentielle
		[XrmProduct88] [varchar](1000) NULL, [XrmProduct92] [varchar](1000) NULL, [XrmProduct95] [datetime] NULL, [XrmProduct96] [datetime] NULL, [XrmProduct97] [numeric](18, 0) NULL, [XrmProduct98] [numeric](18, 0) NULL, [XrmProduct99] [numeric](18, 0) NULL,
		) ON [PRIMARY]

	ALTER TABLE [dbo].[XrmProduct]
		WITH NOCHECK ADD CONSTRAINT [PK_XrmProduct] PRIMARY KEY CLUSTERED ([XrmProductId]) ON [PRIMARY]
END
