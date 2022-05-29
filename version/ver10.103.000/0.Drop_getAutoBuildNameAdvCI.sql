IF EXISTS ( SELECT [Type], Type_Desc,* FROM Sys.Objects where [name] like 'getAutoBuildNameAdvCI' )
BEGIN
	DROP FUNCTION [getAutoBuildNameAdvCI]
END