if (select  count(1) from sysobjects where name ='esp_updateExternalTracking' and xtype='P')>0
	drop procedure [esp_updateExternalTracking]