if (select  count(1) from sysobjects where name ='xsp_SendSMS' and xtype='P')>0
	drop procedure [xsp_SendSMS]
