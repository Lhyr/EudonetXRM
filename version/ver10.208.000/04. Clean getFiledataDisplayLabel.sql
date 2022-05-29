if (select count(*) from sysobjects where 
([xtype] = 'FN'  or  [xtype] = 'IF' ) 
and [name] = 'getFiledataDisplayLabel' and id = object_id(N'[dbo].[getFiledataDisplayLabel]'))>0 Drop Function getFiledataDisplayLabel