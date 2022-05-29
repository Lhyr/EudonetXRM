SET NOCOUNT ON;

DECLARE @nTab INT = 116000

BEGIN
	DELETE [RES]
	WHERE ResId >= @nTab AND ResId < @nTab + 100

	DELETE [FileDataParam]
	WHERE DescID >= @nTab AND DescID < @nTab + 100

	DELETE [UserValue]
	WHERE DescID >= @nTab AND DescID < @nTab + 100

	DELETE [DESCADV]
	WHERE DescID >= @nTab AND DescID < @nTab + 100
	
	DELETE [DESC]
	WHERE DescID >= @nTab AND DescID < @nTab + 100

	-- Table Produit
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length], [type], [ActiveTab], [BreakLine])
	SELECT @nTab, 'XrmProduct', 'XrmProduct', 0, 0, 0, 1, 25

	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05)
	SELECT @nTab, '[Administration Produit]', '[Product Administration]', '[Product Administration]', '[Product Administration]', '[Product Administration]', '[Product Administration]'

	-- Champ Code produit
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length], [Disporder])
	SELECT @nTab + 1, 'XrmProduct', 'ProductCode', 1, 1000, 1

	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05)
	SELECT @nTab + 1, 'Code Produit', 'Product Code', 'Product Code', 'Product Code', 'Product Code', 'Product Code'

	-- Champ Plage de début
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length], [Disporder])
	SELECT @nTab + 2, 'XrmProduct', 'RangeStart', 10, 0, 2

	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05)
	SELECT @nTab + 2, 'Plage de début', 'Range start', 'Range start', 'Range start', 'Range start', 'Range start'

	-- Champ Plage de fin
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length], [Disporder])
	SELECT @nTab + 3, 'XrmProduct', 'RangeEnd', 10, 0, 3

	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05)
	SELECT @nTab + 3, 'Plage de fin', 'Range end', 'Range end', 'Range end', 'Range end', 'Range end'

	-- Champs système
	-- 84 et 84_res
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length], [disporder])
	SELECT @nTab + 84, 'XrmProduct', 'XrmProduct84', 3, 0, 84

	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05)
	SELECT @nTab + 84, 'Confidentielle', 'Confidential', 'Confidential', 'Confidential', 'Confidential', 'Confidential'


	-- 88 et 88_res
	INSERT INTO [desc] ([DescId], [File], [Field], [Format], [Length], [disporder])
	SELECT @nTab + 88, 'XrmProduct', 'XrmProduct88', 8, 0, 88

	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05)
	SELECT @nTab + 88, 'Produit de', 'Product of', 'Product of', 'Product of', 'Product of', 'Product of'

	-- 95 et 95_res
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length])
	SELECT @nTab + 95, 'XrmProduct', 'XrmProduct95', 2, 0

	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05)
	SELECT @nTab + 95, 'Créé le', 'Created on', 'Created on', 'Created on', 'Created on', 'Created on'

	-- 96 et 96_res
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length])
	SELECT @nTab + 96, 'XrmProduct', 'XrmProduct96', 2, 0

	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05)
	SELECT @nTab + 96, 'Modifié le', 'Modified on', 'Modified on', 'Modified on', 'Modified on', 'Modified on'

	-- 97 et 97_res
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length])
	SELECT @nTab + 97, 'XrmProduct', 'XrmProduct97', 8, 0

	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05)
	SELECT @nTab + 97, 'Créé par', 'Created by', 'Created by', 'Created by', 'Created by', 'Created by'

	-- 98 et 98_res
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length])
	SELECT @nTab + 98, 'XrmProduct', 'XrmProduct98', 8, 0

	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05)
	SELECT @nTab + 98, 'Modifié par', 'Modified by', 'Modified by', 'Modified by', 'Modified by', 'Modified by'

	-- 99 et 99_res
	INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length])
	SELECT @nTab + 99, 'XrmProduct', 'XrmProduct99', 8, 0

	INSERT INTO [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04, lang_05)
	SELECT @nTab + 99, 'Appartient à', 'Belongs to', 'Belongs to', 'Belongs to', 'Belongs to', 'Belongs to'

	--Pref/selections
	
	DELETE
	FROM selections
	WHERE tab = @nTab

	DELETE
	FROM pref
	WHERE tab = @nTab

	INSERT INTO PREF (tab, userid, listcol)
	SELECT @nTab, 0, '116001;116002;116003'

	INSERT INTO PREF (tab, userid, listcol)
	SELECT @nTab, userid, '116001;116002;116003'
	FROM [USER]
	
	INSERT INTO SELECTIONS (tab, label, listcol, listcolwidth, userid)
	SELECT @nTab, 'Vue par défaut', '116001;116002;116003', '0;0;0;0', 0

	INSERT INTO SELECTIONS (tab, label, listcol, listcolwidth, userid)
	SELECT @nTab, 'Vue par défaut', '116001;116002;116003', '0;0;0;0', userid
	FROM [USER]

	UPDATE pref
	SET selectid = selections.selectid
	FROM [PREF]
	INNER JOIN selections ON selections.userid = [pref].userid AND [selections].[tab] = [pref].[tab]
	WHERE [PREF].[tab] = @nTab
	
	
	if not exists (select 1 from [descadv] where Parameter=8 and descid=@nTab)
	BEGIN
		INSERT INTO [DESCADV] (Value, Parameter,Category,DescId)
		SELECT 1, 8, 2, @nTab
	END
	ELSE
	BEGIN
		UPDATE [DESCADV] SET Value = 1 WHERE [DESCID] = @nTab AND Parameter = 8
	END
END
