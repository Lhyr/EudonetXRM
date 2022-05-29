if not exists (SELECT 1
		from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		where sys.tables.name like 'PAYMENTTRANSACTION' and syscolumns.name like 'PAYTRANEUDOSTATUS' )
BEGIN
	ALTER TABLE [PAYMENTTRANSACTION] ADD PAYTRANEUDOSTATUS numeric(18,0) default 0
	exec('update PAYMENTTRANSACTION set PAYTRANEUDOSTATUS = 0')
END

INSERT INTO [DESC](DescId, [File], Field, [Format], Length)
SELECT 119511, 'PAYMENTTRANSACTION', 'PAYTRANEUDOSTATUS', 10, 0
WHERE NOT EXISTS (
	SELECT DescId FROM [DESC] WHERE DescId = 119511
)

INSERT INTO [RES](ResId, LANG_00)
SELECT 119511, 'Statut du paiement'
WHERE NOT EXISTS (
	SELECT ResId FROM [RES] WHERE ResId = 119511
)


 update [DESC] set [DispOrder] = 11, [Rowspan] = 1, [Colspan] = 1 where [DescId] = 119511 




 if not exists (SELECT 1
		from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
		where sys.tables.name like 'PAYMENTTRANSACTION' and syscolumns.name like 'PAYTRANFORMULARINFOS' )
BEGIN
	ALTER TABLE [PAYMENTTRANSACTION] ADD [PAYTRANFORMULARINFOS] nvarchar(1000) 
END

INSERT INTO [DESC](DescId, [File], Field, [Format], Length)
SELECT 119512, 'PAYMENTTRANSACTION', 'PAYTRANFORMULARINFOS', 1, 1000
WHERE NOT EXISTS (
	SELECT DescId FROM [DESC] WHERE DescId = 119512
)

INSERT INTO [RES](ResId, LANG_00)
SELECT 119512, 'Informations Formulaire'
WHERE NOT EXISTS (
	SELECT ResId FROM [RES] WHERE ResId = 119512
)


 UPDATE [DESC] SET [DispOrder] = 12, [Rowspan] = 1, [Colspan] = 1 where [DescId] = 119512