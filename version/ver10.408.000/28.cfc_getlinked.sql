/*
GCH/SPH/HLA le 19/11/2013 : gestion des liaisons custom (exemple : table Campaign avec TrackLink)
*/
ALTER FUNCTION [dbo].[cfc_getLinked](@nTab as numeric) RETURNS table AS  
  return  
  
 select   
	(select descType.[type] from [desc] as descType where descType.descid=(cast( [desc].[DescId] / 100 as int)  *100)) [type], 
	(cast( [desc].[DescId] / 100 as int)  *100)[RelationFileDescId]
,   
	[PopupDescId] as RelationPopupDescId,   
	[File] as [RelationFile],	  
	[Field] as [RelationField],  
	(cast( [desc].[popupdescid] / 100 as int)  *100) DescId,    
	(select [file] from [desc] as A where ((cast( [desc].[popupdescid] / 100 as int ) ) * 100)  = [A].[descid]) as [File],    
	(select [field]+'ID' from [desc] as A where ((cast( [desc].[popupdescid] as int ) / 100 ) * 100) = [A].[descid]) as [Field] ,  
	isnull(relation,0) isrelation  
	
 FROM [desc]  
	WHERE  isnull([popup],0) = 2   
	And  [popupdescid]  = (@nTab+1)  
 Union  
	select [type],descid,descid,[file],'PMID',300,'PM','PMID',1 from [desc] where 300 = @nTab and isnull([interpm],0)<>0   
 Union  
	select [type],descid,descid,[file],'PPID',200,'PP','PPID',1 from [desc] where 200 = @nTab  and isnull([interpp],0)<>0  
 Union  
	-- Liaison non système < 100000 
	select[type],descid,descid,[file],  
		case when isnull([type],0)=0 then 'PARENTEVTID' else 'EVTID' end,  
		case when isnull(intereventnum,0) = 0 then 100 else (intereventnum  + 10) * 100 end,  
		'EVENT' + case when isnull(intereventnum,0)<>0 then '_'+cast(intereventnum as varchar(50)) else '' end,   
		'EVTID',1 from [desc] where case when isnull(intereventnum,0) = 0 then 100 else (intereventnum  + 10) * 100 end= @nTab  and isnull([interevent],0)<>0  and @nTab < 100000
Union
	-- Liaison système >= 100000
	select[type],descid,descid,[file],  
		case when isnull([type],0)=0 then 'PARENTEVTID' else 'EVTID' end,  
		case when isnull(intereventnum,0) = 0 then 100 else (intereventnum  + 10) * 100 end,  		
		(select [file] from  [desc] subDesc where subDesc.descid  = case when isnull([desc].intereventnum,0) = 0  then 100 else ([desc].intereventnum  + 10) * 100 end ) z,
		(select [field] +'ID' from  [desc] subDesc where subDesc.descid  = case when isnull([desc].intereventnum,0) = 0  then 100 else ([desc].intereventnum  + 10) * 100 end ) zz,
		1
	from [desc] where case when isnull(intereventnum,0) = 0  then 100 else (intereventnum  + 10) * 100 end= @nTab  and isnull([interevent],0)<>0	and @nTab >= 100000 
 Union  
	select [type],descid,descid,[file], 'ADRID',400,'ADDRESS','ADRID',1 from   
		pref inner join [desc] on descid=tab   
	where  (@nTab=400 and isnull(adrjoin,0)<>0   and userid=0	)  
 --Union  
	--select 9,400,400,'ADDRESS', 'PPID',200,'PP','PPID',1 from [desc] where @nTab = 200 and descid=200  
 --Union  
	--select 9,400,400,'ADDRESS', 'PMID',300,'PM','PMID',1 from [desc] where @nTab = 300 and descid=300



