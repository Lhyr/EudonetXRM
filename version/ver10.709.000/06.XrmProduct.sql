IF NOT EXISTS(SELECT XrmProductId FROM [XrmProduct] WHERE ProductCode = 'FBN')
	INSERT INTO [XrmProduct] (ProductCode, RangeStart, RangeEnd) VALUES ('FBN', 10000, 60099)
	
	
IF NOT EXISTS(SELECT XrmProductId FROM [XrmProduct] WHERE ProductCode = 'Wordline')
	INSERT INTO [XrmProduct] (ProductCode, RangeStart, RangeEnd) VALUES ('Wordline', 1100, 60099)
	
IF NOT EXISTS(SELECT XrmProductId FROM [XrmProduct] WHERE ProductCode = 'Evénements avancés')
	INSERT INTO [XrmProduct] (ProductCode, RangeStart, RangeEnd) VALUES ('Evénements avancés', 1100, 60099)	
	
	
IF NOT EXISTS(SELECT XrmProductId FROM [XrmProduct] WHERE ProductCode = 'Observatoire économique')
	INSERT INTO [XrmProduct] (ProductCode, RangeStart, RangeEnd) VALUES ('Observatoire économique', 1100, 60099)	