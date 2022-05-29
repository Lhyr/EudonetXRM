IF EXISTS ( SELECT [Type], Type_Desc,* FROM Sys.Objects where [name] like 'getAutoBuildNameCI' )
BEGIN
	DROP FUNCTION [getAutoBuildNameCI]
END
		