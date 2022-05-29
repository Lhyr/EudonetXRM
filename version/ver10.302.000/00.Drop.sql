IF EXISTS ( SELECT [Type], Type_Desc,* FROM Sys.Objects where [name] like 'efc_GetHash' )
BEGIN
	DROP FUNCTION [efc_GetHash]
END
