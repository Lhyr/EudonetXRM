declare @viewRuleId25 as varchar(max)
declare @viewRuleId26 as varchar(max)

declare @catModeImmediatId as numeric(18)
declare @catStatusEnCoursId as numeric(18)
declare @catModeRecurrentId as numeric(18)

declare @param25 as varchar(max)
declare @param26 as varchar(max)

--Curseur Tables EventStep
declare @tabId as numeric(18)

declare eventStep_cursor cursor for 
select distinct [DescId]
from [DESCADV] 
where [Parameter] = 42
and ltrim(rtrim([Value])) = '1'
and [DescId] % 100 = 0

open eventStep_cursor

fetch next from eventStep_cursor 
into @tabId

while @@FETCH_STATUS = 0
begin
	--traitement
	
	--Règle de visu sur Annuler Étape
	set @viewRuleId25 = null
	
	select @viewRuleId25 = [ViewRulesId]
	from [Desc] 
	where [DescId] = @tabId + 25

	if isnull(@viewRuleId25, '') = ''
	begin
	
		set @catModeImmediatId = null
		set @catStatusEnCoursId = null		
		
		select top 1 @catModeImmediatId = [DataId]
		from [FileData]
		where [DescId] = @tabId + 6
		and ltrim(rtrim([Data])) = '1'
		
		select top 1 @catStatusEnCoursId = [DataId]
		from [FileData]
		where [DescId] = @tabId + 24
		and ltrim(rtrim([Data])) = '3'	
		
		if isnull(@catModeImmediatId, 0) > 0 and isnull(@catStatusEnCoursId, 0) > 0
		begin
			set @param25 = null
			set @param25 = '&file_0=' + cast(@tabId as varchar) + '&field_0_0=' + cast(@tabId + 6 as varchar) + '&op_0_0=0&value_0_0=' + cast(@catModeImmediatId as varchar) + '&question_0_0=0'
			set @param25 = @param25 + '&and_0_1=1&field_0_1=' + cast(@tabId + 24 as varchar) + '&op_0_1=0&value_0_1=' + cast(@catStatusEnCoursId as varchar) + '&question_0_1=0'
			set @param25 = @param25 + '&and_0_2=1&field_0_2=' + cast(@tabId + 22 as varchar) + '&op_0_2=17&value_0_2=&question_0_2=0'
			set @param25 = @param25 + '&fileonly=0&negation=0&raz=0&random=0'
			
			INSERT INTO [FILTER] ([UserId], [Tab], [Libelle], [Type], [ViewPermId], [UpdatePermId], [DateLastModified], [Param])
			VALUES (null, @tabId, 'Règle de visu pour Annuler Etapes', 2, 0, 0, getdate(), @param25)
			set @viewRuleId25 = cast(scope_identity() as varchar)
			
			if isnull(@viewRuleId25, '') <> ''
			begin
				UPDATE [DESC] SET [ViewRulesId] = '$' + @viewRuleId25 + '$'
				WHERE [DescId] = @tabId + 25
			end
		end
	end
	
	
	--Règle de visu sur Désactivé
	set @viewRuleId26 = null
	
	select @viewRuleId26 = [ViewRulesId]
	from [Desc] 
	where [DescId] = @tabId + 26

	if isnull(@viewRuleId26, '') = ''
	begin
		set @catModeRecurrentId = null
		
		select top 1 @catModeRecurrentId = [DataId]
		from [FileData]
		where [DescId] = @tabId + 6
		and ltrim(rtrim([Data])) = '3'
		
		if isnull(@catModeRecurrentId, 0) > 0
		begin
			set @param26 = null
			set @param26 = '&file_0=' + cast(@tabId as varchar) + '&field_0_0=' + cast(@tabId + 6 as varchar) + '&op_0_0=0&value_0_0=' + cast(@catModeRecurrentId as varchar) + '&question_0_0=0&fileonly=0&negation=0&raz=0&random=0'
		
			INSERT INTO [FILTER] ([UserId], [Tab], [Libelle], [Type], [ViewPermId], [UpdatePermId], [DateLastModified], [Param])
			VALUES (null, @tabId, 'Règle de visu pour Désactivée', 2, 0, 0, getdate(), @param26)
			set @viewRuleId26 = cast(scope_identity() as varchar)
			
			if isnull(@viewRuleId26, '') <> ''
			begin
				UPDATE [DESC] SET [ViewRulesId] = '$' + @viewRuleId26 + '$'
				WHERE [DescId] = @tabId + 26
			end
		end
	end
	
	fetch next from eventStep_cursor 
    into @tabId
end 
close eventStep_cursor;
deallocate eventStep_cursor;