if (select count(1) from sysobjects where name ='efc_RemoveCsVValue' and xtype='FN')>0
	drop function [efc_RemoveCsVValue]

if (select count(1) from sysobjects where name ='efc_RemoveCsVMultiValue' and xtype='FN')>0
	drop function [efc_RemoveCsVMultiValue]
