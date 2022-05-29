 
/*
GCH/SPH/HLA le 19/11/2013 : gestion des liaisons custom (exemple : table Campaign avec TrackLink)
SPH 21/02/2017 : Ajout de campaignstat
*/
ALTER FUNCTION [dbo].[cfc_getLiaison](@nTab as numeric) RETURNS table AS  
  return 
	SELECT	[DescId], [PopupDescId],  [File], [Field], (cast( [desc].[popupdescid] / 100 as int)  *100) RelationFileDescId,   
		(select [type] from [desc] as A where ((cast( [desc].[popupdescid] / 100 as int ) ) * 100)  = [A].[descid]) as  [Type],  
		(select [file] from [desc] as A where ((cast( [desc].[popupdescid] / 100 as int ) ) * 100)  = [A].[descid]) as 	RelationFile,  
		(select [field]+'ID' from [desc] as A where ((cast( [desc].[popupdescid] as int ) / 100 ) * 100) = [A].[descid]) as RelationField ,  
		isnull(relation,0) isrelation 
 from 	[desc] where  isnull([popup],0) = 2 and [descid] > @nTab  and [descid] <  (@nTab+100)  And [popupdescid] In (select [descid]+1 from [desc] where cast([descid] as int) % 100 = 0 and isnull([type],0) = 0) 
 Union 	select descid,descid,[file],'PMID',300,0,'PM','PMID',1 from [desc] where descid= @nTab and isnull([interpm],0)<>0  
 Union 	select descid,descid,[file],'PPID',200,0,'PP','PPID',1 from [desc] where descid= @nTab  and isnull([interpp],0)<>0 
 Union 
		-- Liaison non système < 100000
		select descid,descid,[file], 
		case when isnull([type],0)=0 then 'PARENTEVTID' else 'EVTID' end,  
		case when isnull(intereventnum,0) = 0 then 100 else (intereventnum  + 10) * 100 end, 
		0,
		'EVENT' + case when isnull(intereventnum,0)<>0 then '_'+cast(intereventnum as varchar(50)) else '' end, 
		'EVTID',
		1 from [desc] where descid= @nTab  and isnull([interevent],0)<>0 and @nTab < 100000 
Union 
		-- Liaison système >= 100000
		select descid,descid,[file], 
		case when isnull([type],0)=0 then 'PARENTEVTID' else 'EVTID' end,  
		case when isnull(intereventnum,0) = 0 then 100 else (intereventnum  + 10) * 100 end, 
		0,
		(select [file] from  [desc] subDesc where subDesc.descid  = case when isnull([desc].intereventnum,0) = 0  then 100 else ([desc].intereventnum  + 10) * 100 end ) z,
		(select [field] +'ID' from  [desc] subDesc where subDesc.descid  = case when isnull([desc].intereventnum,0) = 0  then 100 else ([desc].intereventnum  + 10) * 100 end ) zz,
		1 from [desc] where descid= @nTab  and isnull([interevent],0)<>0 and @nTab >= 100000 
Union 	select descid,descid,[file], 'ADRID',400,9,'ADDRESS','ADRID',1 from pref inner join [desc] on descid=tab where tab= @nTab and isnull(adrjoin,0)<>0 and userid=0 
 Union 	select 400,400,'ADDRESS', 'PPID',200,0,'PP','PPID',1 from [desc] where @nTab = 400 and descid=200 
 Union 	select 400,400,'ADDRESS', 'PMID',300,0,'PM','PMID',1 from [desc] where @nTab = 400 and descid=300
 union  select 106000,106000,'CAMPAIGN','CAMPAIGNID', 111000,0, 'CampaignStats','EVTID' ,1  where @nTab = 106000
 union  select 106000,106000,'CAMPAIGN','CAMPAIGNID', 111100,0, 'CampaignStatsADV','EVTID' ,1  where @nTab = 106000