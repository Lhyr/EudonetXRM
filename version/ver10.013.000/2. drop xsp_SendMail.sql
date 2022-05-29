IF EXISTS (select name from sys.sysobjects where type = 'P' and name = 'xsp_SendMail') 
	Drop PROCEDURE dbo.xsp_SendMail