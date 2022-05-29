IF not exists (SELECT 1
	from sys.tables inner join syscolumns on syscolumns.id = sys.tables.object_id
	where sys.tables.name like 'WorkflowActivities' and syscolumns.name like 'WFAcAdrid' )
BEGIN	
	ALTER TABLE [WorkflowActivities] ADD [WFAcAdrid] NUMERIC(18,0)
END	


DELETE FROM [DESC] WHERE DESCID = 119404
DELETE FROM [RES] WHERE RESID = 119404


-- Libellé
INSERT INTO [DESC] ([DescId], [File], [Field], [Format], [Length],[popup], [popupdescid]  ) 
SELECT 119404, 'WorkflowActivities', 'WFAcAdrid', 1, 0,2,401
INSERT INTO [res] (resid, lang_00, lang_01 ) SELECT 119404, 'Adresse', 'Address'   
	
	
	
	
--Mise en page Table WorkflowStep
update [DESC] set [columns] = '200,500,25' where [DescId] = 119400  
update [DESC] set [DispOrder] = 1, X =0 , Y =0 ,[Rowspan] = 1, [Colspan] = 3 where [DescId] = 119401 	
update [DESC] set [DispOrder] = 2, X =0 , Y =1, [Rowspan] = 1, [Colspan] = 3 where [DescId] = 119402
update [DESC] set [DispOrder] = 3, X =0 , Y =2, [Rowspan] = 1, [Colspan] = 3 where [DescId] = 119403
update [DESC] set [DispOrder] = 4, X =0 , Y =3, [Rowspan] = 1, [Colspan] = 3 where [DescId] = 119404



-- champ
declare @descid as numeric(18,0)
declare @order as int

declare updtpos cursor for
select  [descid] from [DESC] where [descid] >=119440 and [descid] < 119470
set @order = 4
open updtpos 

FETCH updtpos INTO @descid

WHILE @@FETCH_STATUS = 0
BEGIN
	 

	 update [DESC] set [DispOrder] = @order +1 , X =0 , Y = @order, [Rowspan] = 1, [Colspan] = 3 where [DescId] = @descid

	 set @order = @order +1

	FETCH updtpos INTO @descid
END

close updtpos
deallocate updtpos
