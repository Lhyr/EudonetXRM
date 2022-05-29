IF EXISTS ( SELECT [Type], Type_Desc,* FROM Sys.Objects where [name] like 'getTabsFieldsDisplay' )
BEGIN
	DROP FUNCTION [getTabsFieldsDisplay]
END
