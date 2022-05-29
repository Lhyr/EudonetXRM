IF EXISTS ( SELECT [Type], Type_Desc,* FROM Sys.Objects where [name] like 'efc_split' )
BEGIN
	DROP FUNCTION [efc_split]
END
