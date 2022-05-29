
declare @vers as int =  cast(substring(cast(SERVERPROPERTY('productversion') as varchar), 0, 3) as int)

if(@vers < 11)
begin
	print 'version non prise en charge'
	return;
end

begin try
	begin tran;
		--On sauve les meubles

		IF (OBJECT_ID (N'[dbo].[DESCADV_OLD]', N'U') IS NOT NULL)
			drop table [dbo].[DESCADV_OLD];

		select [Id], [DescId], [Parameter], [Value], [Category]
			into [dbo].[DESCADV_OLD]
			from [dbo].[DESCADV] 		
		where Parameter = 45		

		declare @tblJsonEX as Table(nTab int, Summary nvarchar(max), Wizard nvarchar(max));
		declare @SummaryOfChanges as table(Change nvarchar(20));  

		--on récupère tous les onglets principaux qui n'ont pas de JSON de déclaré.
		with cte_descadv as (
			select DescId
				from [dbo].[DESCADV] as da
			where da.Parameter = 45
				and ((ISNULL(da.[value], '') != '' 
					and (
						iif(CHARINDEX('"jsonsummary":',  replace(Lower(da.[value]), ' ', '')) > 0, 
							CHARINDEX('"jsonsummary":{}', replace(Lower(da.[value]), ' ', '')), 
							CHARINDEX('"summary":{}', replace(Lower(da.[value]), ' ', ''))) < 1)
					and (
						iif(CHARINDEX('"jsonwizardbar":',  replace(Lower(da.[value]), ' ', '')) > 0, 
							CHARINDEX('"jsonwizardbar":{}', replace(Lower(da.[value]), ' ', '')), 
							CHARINDEX('"wizardbar":{}', replace(Lower(da.[value]), ' ', ''))) < 1))
				)
		),
		--on récupère les catalogues pour la zone résumé
		cte_catadv as (
			select DescId - (DescId % 100) as nTab,
				   DescId,
				   ROW_NUMBER() over(partition by DescId - (DescId % 100) order by DescId) as num
				from [dbo].[DESC] as dCat 
			where dCat.[Format] = 1 
				and dCat.Popup = 3 
				and dCat.PopupDescId = dCat.DescId
		),
		--on récupère les catalogues en étape pour la zone assistant
		cte_catStep as (
			select d.DescId - (d.DescId % 100) as nTab,
				   d.DescId,
				   ROW_NUMBER() over(partition by d.DescId - (d.DescId % 100) order by d.DescId) as num
				from [desc] as d
					left join [filedataparam] fdp on fdp.[descid] = d.[descid]
			where stepMode = 1 and Multiple = 0
				and d.PopupDescId = d.DescId
		),
		--on récupère les images pour la zone résumé
		cte_img as (
			select DescId - (DescId % 100) as nTab,
				   DescId,
				   ROW_NUMBER() over(partition by DescId - (DescId % 100) order by DescId) as num
				from [dbo].[DESC] as dImg 
			where dImg.[Format] = 13
		)
		insert into @tblJsonEX(nTab, Summary, Wizard)
		select distinct d.DescId,
				case d.DescId when 300 then N'{"title": 301, "sTitle": 310, "avatar": 375, "inputs": [{"DescId": 305}, {"DescId": 306},{"DescId": 308}]}'
								when 200 then N'{"title": 201, "sTitle": 213, "avatar": 275, "inputs": [{"DescId": 204}, {"DescId": 208},{"DescId": 217}]}'
								when 400 then N'{"title": 401, "sTitle": 403, "avatar": 0, "inputs": [{"DescId": 402}, {"DescId": 408},{"DescId": 405},{"DescId": 412},{"DescId": 411}]}'
								else  concat(N'{"title": ', d.DescId+1)
								+ case when isnull(cat.DescId, 0) > 0 then concat(', "sTitle": ' , cat.DescId) else '' end
								+ case when isnull(img.DescId, 0) > 0 then concat(' , "avatar": ' , img.DescId) else '' end 
								+', "inputs": ['
								+ case when d.Interpp = 1 then '{"DescId": 201}' + iif(d.InterPM = 1, ',', '') else '' end
								+ case when d.InterPM = 1 then '{"DescId": 301}' + iif(isnull(d.InterEventNum, 0) > 0, ',', '') else '' end
								+ case when isnull(d.InterEvent, 0) > 0 then concat('{"DescId": ' ,((case when d.InterEventNum > 0 then (d.InterEventNum + 10) else 1 end * 100) + 1), '}')  else '' end
								+']}'
			    end as Summary,
				case when isnull(step.nTab, 0) > 0 

					then concat('{DescId: ', step.Descid, ', HidePreviousButton: true, HideNextButton: true, WelcomeBoard: {Display: "hide", Body: {lang_00: "Vous pourrez ensuite visualiser ou modifier les champs de cette étape, la validation des champs se fera en sortant de celui-ci.", lang_01: "You will then be able to view or modify the fields in this step, the validation of the fields will be done by exiting it."}, Title: {lang_00: "Pour démarrer l''assistant, veuillez sélectionner une étape", lang_01: "To start the wizard, please select a step"}},')
						+ 'FieldsById: [' + substring(xlmStep.fields, 0, len(xlmStep.fields)) + ']}'
					else ''
				end as Wizard
			from [dbo].[DESC] as d
				left join [dbo].[DESCADV] as da on da.DescId = d.DescId 
				left join (select nTab, DescId  from cte_catadv where num < 2) as cat on cat.nTab = d.DescId
				left join (select nTab, DescId  from cte_img where num < 2) as img on img.nTab = d.DescId
				left join (select nTab, DescId  from cte_catStep where num < 2) as step on step.nTab = d.DescId
				cross apply (select concat('{DataId: ', isnull(DataId, 0), ',') 
									+ concat('DataIdPrevious:[', isnull(substring(prev.xmlPrev, 0, len(prev.xmlPrev)), 0) , '],')
									+ concat('DataIdNext:[',  isnull(substring(foll.xmlFoll, 0, len(foll.xmlFoll)), 0) , '],')
									+ 'DisplayedFields: []'
									+ '},'
								from [dbo].[FILEDATA] as fdt
									cross apply(select concat(isnull(DataId, 0), ',')
													from  [dbo].[FILEDATA] as fdtPrev 
												where fdtPrev.DescId = fdt.DescId 
													and fdtPrev.DataId < fdt.DataId
												for xml path('')) as prev(xmlPrev)
									cross apply(select concat(isnull(DataId, 0), ',')
													from  [dbo].[FILEDATA] as fdtFoll 
												where fdtFoll.DescId = fdt.DescId 
													and fdtFoll.DataId > fdt.DataId
												for xml path('')) as foll(xmlFoll)
							  where fdt.DescId = step.DescId
							for xml path('')) as xlmStep(fields)
		where d.DescId % 100 = 0
			and d.DescId > 0
			and d.[Type] = 0
			and not exists (select * from cte_descadv where cte_descadv.DescId = d.DescId)
		order by DescId
		
		;with cte_pageJson as (
			select nTab,
				  case when isnull(tbl.Summary, '') != '' then tbl.Summary else '{}' end as Summary,
				  case when isnull(tbl.Wizard, '') != '' then tbl.Wizard else '{}' end as Wizard,
				  concat('{"Page":{"Tab":', tbl.nTab , 
					',"WizardBarArea":{"id":"stepsLine","type":"stepsLine","dataGsMinWidth":7,"dataGsMinHeight":7,"dataGsX":0,"dataGsY":0,"dataGsWidth":12,"dataGsHeight":11,"NoResize":true,"NoDraggable":true,"NoLock":true},"ActivityArea":{"id":"activite","type":"activite","dataGsMinWidth":3,"dataGsMinHeight":11,"dataGsX":9,"dataGsY":11,"dataGsWidth":3,"dataGsHeight":11,"NoResize":true,"NoDraggable":true,"NoLock":true},"DetailArea":{"id":"signets","type":"signets","dataGsMinWidth":7,"dataGsMinHeight":7,"dataGsX":0,"dataGsY":11,"dataGsWidth":9,"dataGsHeight":11,"NoResize":true,"NoDraggable":true,"NoLock":true},"Activity":true,"JsonSummary":', 
					case when isnull(tbl.Summary, '') != '' then tbl.Summary else '{}' end,
					',"JsonWizardBar":' , 
					case when isnull(tbl.Wizard, '') != '' then tbl.Wizard else '{}' end,
					',"NbCols":2}}') as [json]
				from  @tblJsonEX as tbl
		)
		merge [dbo].[descadv] as tgt
			using cte_pageJson as src on tgt.DescId = src.nTab and tgt.parameter = 45
		when matched then 
			update set tgt.[value] = case when isnull(tgt.[value], '') = '' then  src.[json]
										  when CHARINDEX('"jsonsummary":{}', replace(Lower(tgt.[value]), ' ', '')) > 0 
											then  replace(tgt.[value] collate Latin1_General_CI_AS, '"jsonsummary":{}', '"JsonSummary":'+ src.Summary)
										  when CHARINDEX('"summary":{}', replace(Lower(tgt.[value]), ' ', '')) > 0 
											then replace(tgt.[value] collate Latin1_General_CI_AS, '"summary":{}', '"JsonSummary":'+ src.Summary)
										  when CHARINDEX('"jsonwizardbar":{}', replace(Lower(tgt.[value]), ' ', '')) > 0
											then replace(tgt.[value] collate Latin1_General_CI_AS, '"jsonwizardbar":{}', '"JsonWizardBar":'+ src.Wizard)
										  when CHARINDEX('"wizardbar":{}', replace(Lower(tgt.[value]), ' ', '')) > 0 
											then replace(tgt.[value] collate Latin1_General_CI_AS, '"wizardbar":{}', '"JsonWizardBar":'+ src.Wizard)
										  when CHARINDEX('"jsonsummary":{}', replace(Lower(tgt.[value]), ' ', '')) > 0 
												and CHARINDEX('"jsonwizardbar":{}', replace(Lower(tgt.[value]), ' ', '')) > 0
											then replace(replace(tgt.[value] collate Latin1_General_CI_AS, '"jsonwizardbar":{}', '"JsonWizardBar":'+ src.Wizard), '"jsonsummary":{}', '"JsonSummary":'+ src.Summary)
										  when CHARINDEX('"summary":{}', replace(Lower(tgt.[value]), ' ', '')) > 0 
												and CHARINDEX('"wizardbar":{}', replace(Lower(tgt.[value]), ' ', '')) > 0
											then replace(replace(tgt.[value] collate Latin1_General_CI_AS, '"wizardbar":{}', '"JsonWizardBar":'+ src.Wizard), '"summary":{}', '"JsonSummary":'+ src.Summary)
									 end
		when not matched then
			insert(descid, parameter, [value]) values(src.nTab, 45, src.[json])
		output $action into @SummaryOfChanges;

	commit tran;
end try
begin catch
	rollback tran;

	declare @ErrorMessage as nvarchar(max);  
    declare @ErrorSeverity as int;  
    declare @ErrorState as int;  
  
    select @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();  
  
    RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);  
end catch
