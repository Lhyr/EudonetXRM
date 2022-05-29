 
 /*
 
 Permet de vérifier les dépendances d'un champ quelconque d'une table quelquconaces

 select * from [efc_getFieldDependancyAll]('CONFIG','USERID')

 */
 CREATE FUNCTION [dbo].[efc_getFieldDependancyAll](@table as varchar(1000), @field as varchar(100)) RETURNS table AS 
 return 
 
 select 
 object_name(sd.referenced_major_id) as [TABLE],
 sc.name as [FIELD],
 
object_name(sd.object_id) [DEPENDANCE],

  

case 
when sysobjects.xtype ='AF' then  'Aggregate function (CLR)'
when sysobjects.xtype ='C'  then 'CHECK constraint'
when sysobjects.xtype ='D'  then 'DEFAULT (constraint or stand-alone)'
when sysobjects.xtype ='F'  then 'FOREIGN KEY constraint'
when sysobjects.xtype ='FN' then 'SQL scalar function'
when sysobjects.xtype ='FS' then 'Assembly (CLR) scalar-function'
when sysobjects.xtype ='FT' then 'Assembly (CLR) table-valued function'
when sysobjects.xtype ='IF' then 'SQL inline table-valued function'
when sysobjects.xtype ='IT' then 'Internal table'
when sysobjects.xtype ='P' then 'SQL Stored Procedure'
when sysobjects.xtype ='PC' then 'Assembly (CLR) stored-procedure'
when sysobjects.xtype ='PG' then 'Plan guide'
when sysobjects.xtype ='PK' then 'PRIMARY KEY constraint'
when sysobjects.xtype ='R' then 'Rule (old-style, stand-alone)'
when sysobjects.xtype ='RF' then 'Replication-filter-procedure'
when sysobjects.xtype ='S'  then 'System base table'
when sysobjects.xtype ='SN' then 'Synonym'
when sysobjects.xtype ='SQ' then 'Service queue'
when sysobjects.xtype ='TA' then 'Assembly (CLR) DML trigger'
when sysobjects.xtype ='TF' then 'SQL table-valued-function'
when sysobjects.xtype ='TR' then 'SQL DML trigger'
when sysobjects.xtype ='TT' then 'Table type'
when sysobjects.xtype ='U' then 'Table (user-defined)'
when sysobjects.xtype ='UQ' then 'UNIQUE constraint'
when sysobjects.xtype ='V'  then 'View'
when sysobjects.xtype ='X'  then 'Extended stored procedure'
else 'Autre'
end


 [TYPE_DEPENDANCE] ,
/*
object_name(sd.referenced_minor_id),
sd.referenced_minor_id,
*/
sm.definition

 from sys.sql_dependencies  sd
inner join sys.sql_modules sm on sm.object_id = sd.object_id
inner join sys.syscolumns sc on sd.referenced_minor_id = sc.colid and sc.id = sd.referenced_major_id
--inner join [desc] on  object_name(sd.referenced_major_id) Collate FRENCH_CI_AI = [FILE] Collate FRENCH_CI_AI and [desc].[field] Collate FRENCH_CI_AI = sc.name  Collate FRENCH_CI_AI
inner join sys.sysobjects on sysobjects.id  = sd.object_id 
where 

object_name(sd.referenced_major_id) = @table
and
sc.name = @field
--[desc].[descid] = @nDescId
