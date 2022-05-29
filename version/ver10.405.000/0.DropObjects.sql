 IF EXISTS (select name from sys.sysobjects where type = 'P' and name = 'xsp_MAJFORMULATRIGGER') 
	Drop PROCEDURE dbo.xsp_MAJFORMULATRIGGER

IF EXISTS(
	SELECT so.name,sc.name FROM SYSOBJECTS so
left join syscolumns sc on so.id = sc.id
 where so.id = object_id(N'[dbo].[FORMULATRIGGER]') and OBJECTPROPERTY(so.id, N'IsUserTable') = 1
 and sc.name='formuladescid')
	DROP TABLE FORMULATRIGGER

--	select * from FORMULATRIGGER