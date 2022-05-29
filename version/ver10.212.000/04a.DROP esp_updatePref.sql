/*
*	DROP PROCEDURE
*	BSE - 30/11/2016
*/

IF EXISTS ( SELECT * 
            FROM   sysobjects 
            WHERE  id = object_id(N'[dbo].[esp_updatePref]') 
                   and OBJECTPROPERTY(id, N'IsProcedure') = 1 )
BEGIN
    DROP PROCEDURE [dbo].[esp_updatePref]
END