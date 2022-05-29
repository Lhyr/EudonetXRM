if (select  count(1) from sysobjects where name ='xsp_CreateWebTabSpecif' and xtype='P')>0
	drop procedure [xsp_CreateWebTabSpecif]
	
if (select  count(1) from sysobjects where name ='esp_CreateVirtualTemplate' and xtype='P')>0
	drop procedure [esp_CreateVirtualTemplate]