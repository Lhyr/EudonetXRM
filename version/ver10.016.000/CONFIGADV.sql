declare @IsItNullable as bit
set @IsItNullable = 0
select @IsItNullable = isnull(sys.columns.is_nullable,0) from sys.columns inner join sys.objects on sys.objects.object_id = sys.columns.object_id where sys.columns.[name] = 'Category' and sys.objects.[name] = 'CONFIGADV'
if(@IsItNullable = 0)
begin
	alter table [dbo].[CONFIGADV] alter column [Category] [int] NULL;
end


