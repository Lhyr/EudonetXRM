IF EXISTS ( SELECT [Type], Type_Desc,* FROM Sys.Objects where [name] like 'efc_getNotAllowedField' )
BEGIN
	DROP FUNCTION [efc_getNotAllowedField]
END
