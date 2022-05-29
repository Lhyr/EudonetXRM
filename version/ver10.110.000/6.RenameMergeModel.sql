Declare @Reportid as Numeric
Declare @Param as varchar(max)
DECLARE @COMPT AS NUMERIC
Declare @Pos as numeric
Declare @Pos2 as numeric
Declare @Newv as varchar(max) = 'https://xrm.eudonet.com/XRM'
Declare Curs Cursor For


     select ReportId,Param from [REPORT] where Param like '%www%' or Param like '%ww2%' or Param like '%ww4%'  or Param like '%ww5%'                          

      Open Curs
      Fetch Curs Into @Reportid,@Param
	  SET @COMPT=0
      While @@Fetch_status = 0
      Begin  
	  if  (CHARINDEX('wordfilepath=http', @Param)) <> 0 and (CHARINDEX('wordfileoutput=http', @Param)) <> 0
	  begin
	  goto Stop1
	  end
	  if (CHARINDEX('wordfilepath=http', @Param)) <> 0
	  Begin
			 set @Pos2 = charindex('wordfilepath',@Param,0)
			 set @Pos = charindex('Fdatas',@Param,0)
			set @Pos = @Pos -2
			set @Param = substring(@Param,0,@Pos2+13) + @Newv + substring(@Param,@Pos,len(@Param))
			select @Param
			update REPORT set Param = @Param where ReportId = @Reportid		 
	 End
	   if (CHARINDEX('wordfileoutput=http', @Param)) <> 0
	  Begin
			 set @Pos2 = charindex('wordfileoutput',@Param,0)
			 set @Pos = charindex('Fdatas',@Param,0)
			set @Pos = @Pos -2
			set @Param = substring(@Param,0,@Pos2+15) + @Newv + substring(@Param,@Pos,len(@Param))
			select @Param
			update REPORT set Param = @Param where ReportId = @Reportid		 
	 End
	 			Stop1:
                 set @COMPT = 1+@COMPT 
				  Fetch Curs Into @Reportid,@Param
      End
	   SELECT @COMPT
      Close Curs
Deallocate Curs