
--ALTER TABLE [TEMPLATE_24] ALTER COLUMN [MergeFields] [varchar](MAX) NULL;

declare @tabName varchar(200)
declare @colName varchar(200)
declare @colType int
declare @constName varchar(100)

declare @alterTable nvarchar(max)

DECLARE @ErrorMessage NVARCHAR(4000);
DECLARE @ErrorSeverity INT;
DECLARE @ErrorState INT;

DECLARE myCursor CURSOR
FOR select tab.[name] as tabName, col.[name] as colName, col.[user_type_id] as colType, const.[name] as constName
from sys.columns col
	inner join sys.tables tab on col.object_id = tab.object_id
	left join SYS.DEFAULT_CONSTRAINTS const on const.object_id = col.default_object_id
where col.name = 'MergeFields' and col.system_type_id != 165
order by tabName, constName

OPEN myCursor

FETCH NEXT FROM myCursor INTO @tabName, @colName, @colType, @constName

WHILE @@fetch_status = 0
BEGIN
	--print  @tabName +'/'+ @colName +'/'+ @constName

	IF ISNULL(@constName, '') <> ''
	BEGIN
		set @alterTable = 'ALTER TABLE ['+@tabName+'] DROP CONSTRAINT '+@constName+';'
		BEGIN TRY
			exec(@alterTable)
		END TRY
		BEGIN CATCH
			PRINT 'Query : ' + @alterTable;

			--THROW;
			-- HLA - THROW compatible a partir de SQL 2012, on passe donc à RAISERROR pour rester compatible
			SELECT 
				@ErrorMessage = ERROR_MESSAGE(),
				@ErrorSeverity = ERROR_SEVERITY(),
				@ErrorState = ERROR_STATE();

			RAISERROR (@ErrorMessage,	-- Message text.
					   @ErrorSeverity,	-- Severity.
					   @ErrorState	-- State.
					   );
		END CATCH
	END
	
	set @alterTable = 'ALTER TABLE ['+@tabName+'] ALTER COLUMN [MergeFields] [varbinary](MAX) NULL;'
	BEGIN TRY
		exec(@alterTable)
	END TRY
	BEGIN CATCH
		PRINT 'Query : ' + @alterTable;

		--THROW;
		-- HLA - THROW compatible a partir de SQL 2012, on passe donc à RAISERROR pour rester compatible
		SELECT 
			@ErrorMessage = ERROR_MESSAGE(),
			@ErrorSeverity = ERROR_SEVERITY(),
			@ErrorState = ERROR_STATE();

		RAISERROR (@ErrorMessage,	-- Message text.
				   @ErrorSeverity,	-- Severity.
				   @ErrorState	-- State.
				   );
	END CATCH

	FETCH NEXT FROM myCursor INTO @tabName, @colName, @colType, @constName
END

CLOSE myCursor
DEALLOCATE myCursor
