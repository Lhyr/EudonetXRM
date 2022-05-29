/*GCH le 13/06/2014 : demandes #31297 et #31296 : Modèle doivent être associé à la table de création*/

declare @nTab int, @nFileId int, @sBody as varchar(max)
declare @nStartPos as int, @sPattern as varchar(500)

begin try
	declare curs cursor for
		select tab,[body],MAILTEMPLATEid from MAILTEMPLATE where Tab = 106000
	open curs
		fetch next from curs into @nTab, @sBody,@nFileId
	while @@FETCH_STATUS=0
	begin
		--on recherche le 1er champs de fusion et on utilise la table du descid du champ de fusion comme table pour le modèle
		set @sPattern = '"mergefield" ednd="'
		set @nStartPos = charindex(@sPattern,@sBody)
		--print @nStartPos
		if(@nStartPos > 0)
		begin
			set @nStartPos = @nStartPos+datalength(@sPattern)
			set @sBody = substring(@sBody,@nStartPos,datalength(@sbody)-@nStartPos)
			
			set @sPattern = '"'
			set @nStartPos = charindex(@sPattern,@sBody)
			if(@nStartPos > 0)
			begin
				set @sBody= left(@sBody,@nStartPos-datalength(@sPattern))			
				if ISNUMERIC(@sBody) = 1
				begin
					set @nTab = CONVERT(int,@sBody)
					set @nTab = @nTab-@nTab%100
					update MAILTEMPLATE set Tab =@nTab where MAILTEMPLATEid = @nFileId
				end
			end
		end
		if @nTab = 106000
		begin
			set @nTab = 200	--PAR DEFAUT pon rattache à Contact pour pas perdre le modèle définitivement
			update MAILTEMPLATE set Tab =@nTab where MAILTEMPLATEid = @nFileId
		end
		fetch next from curs into @nTab, @sBody,@nFileId
	end
	close curs
	deallocate curs
end try
begin catch
     /* select @Erreur = 
                       convert(varchar(max),ERROR_NUMBER())-- AS ErrorNumber
                       +' '+convert(varchar(max),ERROR_SEVERITY())-- AS ErrorSeverity
                       +' '+convert(varchar(max),ERROR_STATE())-- AS ErrorState
                       +' '+convert(varchar(max),ERROR_PROCEDURE())-- AS ErrorProcedure
                       +' '+convert(varchar(max),ERROR_LINE())-- AS ErrorLine
                       +' '+ERROR_MESSAGE()-- AS ErrorMessage
       */                
end catch
      