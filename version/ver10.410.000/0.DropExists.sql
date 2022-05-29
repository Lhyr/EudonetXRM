/**
*	Concatène des valeurs de catalogue sans retourner de doublons
*	HLA - 06/02/2013
*/
set nocount on;
IF EXISTS (select name from sys.sysobjects where type = 'P' and name = 'xsp_UpdateHomePageUserAssign') 
BEGIN
	DROP PROCEDURE xsp_UpdateHomePageUserAssign
END
