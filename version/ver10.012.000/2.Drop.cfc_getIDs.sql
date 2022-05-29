IF EXISTS ( SELECT [Type], Type_Desc,* FROM Sys.Objects where [name] like 'cfc_getIDs' )
BEGIN
	DROP FUNCTION [cfc_getIDs]
END
