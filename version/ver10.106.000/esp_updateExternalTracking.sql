 
/*
*	Met à jour les options pour le tracking externalisé
*	SPH  
parameter :
@enabled : activation/désactivation
@website : url du site
@xrmPath : path a partir de website de l'appli xrm
@internalwebsiteroot : path a partir de website des webservices XRM
exec [esp_updateExternalTracking] '1','eudo','aeaeaeaze','http://simonexternalsite/','xrm.dev', 'xrmws','http://sph-pc/xrm.dev/'
		
select * from configadv
select * from eudotrait..externalclients
--delete from configadv
*/

CREATE proCEDURE [dbo].[esp_updateExternalTracking]
	@enabled bit,
	@clientname varchar(500),
	@serial varchar(500),
	@websiteroot varchar(800),
	@xrmPath varchar(100),
	@xrmwsPath varchar(100),
	@internalwebsiteroot varchar(800)
AS 

set nocount on

If ((select count(1) from configadv where parameter='EXTERNAL_TRACKING_ENABLED' )= 0)
	insert into configadv (parameter,value) select 'EXTERNAL_TRACKING_ENABLED',@enabled
else
	update configadv set [value] = @enabled where [parameter] = 'EXTERNAL_TRACKING_ENABLED'


If ((select count(1) from configadv where parameter='EXTERNAL_TRACKING_SERIAL' )= 0)
	insert into configadv (parameter,value) select 'EXTERNAL_TRACKING_SERIAL',@serial
else
	update configadv set [value] = @serial where [parameter] = 'EXTERNAL_TRACKING_SERIAL'
	
	
	
If ((select count(1) from configadv where parameter='EXTERNAL_TRACKING_URL' )= 0)
	insert into configadv (parameter,value) select 'EXTERNAL_TRACKING_URL',@websiteroot
else
	update configadv set [value] = @websiteroot where [parameter] = 'EXTERNAL_TRACKING_URL'
	
If ((select count(1) from configadv where parameter='EXTERNAL_TRACKING_XRM_PATH' )= 0)
	insert into configadv (parameter,value) select 'EXTERNAL_TRACKING_XRM_PATH',@xrmPath
else
	update configadv set [value] = @xrmPath where [parameter] = 'EXTERNAL_TRACKING_XRM_PATH'
	
	
If ((select count(1) from configadv where parameter='EXTERNAL_TRACKING_WS_PATH' )= 0)
	insert into configadv (parameter,value) select 'EXTERNAL_TRACKING_WS_PATH',@xrmwsPath
else
	update configadv set [value] = @xrmwsPath where [parameter] = 'EXTERNAL_TRACKING_WS_PATH'	
		

If ((select count(1) from configadv where parameter='EXTERNAL_TRACKING_INTERNAL_URL' )= 0)
	insert into configadv (parameter,value) select 'EXTERNAL_TRACKING_INTERNAL_URL',@internalwebsiteroot
else
	update configadv set [value] = @internalwebsiteroot where [parameter] = 'EXTERNAL_TRACKING_INTERNAL_URL'	
		
		
IF   (select count(1) from eudotrait..externalclients ec
inner join eudotrait..externalmodules em on ec.moduleid=em.moduleid and modulecodename='EXTERNALTRACKING'
where serial=@serial	)=0
BEGIN

	INSERT INTO eudotrait..externalclients(moduleid,clientname, serial,[enabled]) 
	select moduleid,@clientname,@serial,@enabled from  eudotrait..externalmodules where modulecodename='EXTERNALTRACKING'


END
else
	UPDATE eudotrait..externalclients set enabled=@enabled where serial=@serial and moduleid = (select top 1 moduleid from eudotrait..externalmodules where modulecodename='EXTERNALTRACKING')

	
	
	
set nocount off	