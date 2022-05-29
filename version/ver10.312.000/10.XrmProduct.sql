IF NOT EXISTS(SELECT XrmProductId FROM [XrmProduct] WHERE ProductCode = 'Association professionnelle')
	INSERT INTO [XrmProduct] (ProductCode, RangeStart, RangeEnd) VALUES ('Association professionnelle', 10000, 17099)  
	
IF NOT EXISTS(SELECT XrmProductId FROM [XrmProduct] WHERE ProductCode = 'Culture')
	INSERT INTO [XrmProduct] (ProductCode, RangeStart, RangeEnd) VALUES ('Culture', 10000, 17099)

IF NOT EXISTS(SELECT XrmProductId FROM [XrmProduct] WHERE ProductCode = 'Equity')
	INSERT INTO [XrmProduct] (ProductCode, RangeStart, RangeEnd) VALUES ('Equity', 10000, 17099)

IF NOT EXISTS(SELECT XrmProductId FROM [XrmProduct] WHERE ProductCode = 'ESR')
	INSERT INTO [XrmProduct] (ProductCode, RangeStart, RangeEnd) VALUES ('ESR', 10000, 17099)

IF NOT EXISTS(SELECT XrmProductId FROM [XrmProduct] WHERE ProductCode = 'Fondation')
	INSERT INTO [XrmProduct] (ProductCode, RangeStart, RangeEnd) VALUES ('Fondation', 10000, 17099)

IF NOT EXISTS(SELECT XrmProductId FROM [XrmProduct] WHERE ProductCode = 'Franchise')
	INSERT INTO [XrmProduct] (ProductCode, RangeStart, RangeEnd) VALUES ('Franchise', 10000, 17099)

IF NOT EXISTS(SELECT XrmProductId FROM [XrmProduct] WHERE ProductCode = 'Immobilier')
	INSERT INTO [XrmProduct] (ProductCode, RangeStart, RangeEnd) VALUES ('Immobilier', 10000, 17099)

IF NOT EXISTS(SELECT XrmProductId FROM [XrmProduct] WHERE ProductCode = 'Pôle et cluster')
	INSERT INTO [XrmProduct] (ProductCode, RangeStart, RangeEnd) VALUES ('Pôle et cluster', 10000, 17099)

IF NOT EXISTS(SELECT XrmProductId FROM [XrmProduct] WHERE ProductCode = 'Secteur Public')
	INSERT INTO [XrmProduct] (ProductCode, RangeStart, RangeEnd) VALUES ('Secteur Public', 10000, 17099)

UPDATE [XrmProduct] SET RangeStart = 17100, RangeEnd = 29999 WHERE ProductCode = 'GESCOM'

DELETE FROM [XrmProduct] WHERE ProductCode IN ('Produits', 'BU_FEDE')