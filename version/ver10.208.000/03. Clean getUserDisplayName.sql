if (select count(*) from sysobjects where 
([xtype] = 'FN'  or  [xtype] = 'IF' ) 
and [name] = 'getUserDisplayName' and id = object_id(N'[dbo].[getUserDisplayName]'))>0 Drop Function getUserDisplayName