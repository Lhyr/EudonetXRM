if (select  count(1) from sysobjects where name ='efc_splitCTE' and xtype='IF')>0
	drop function [efc_splitCTE]

if (select count(1) from sysobjects where name ='efc_RemoveCsVValue' and xtype='FN')>0
	drop function [efc_RemoveCsVValue]
