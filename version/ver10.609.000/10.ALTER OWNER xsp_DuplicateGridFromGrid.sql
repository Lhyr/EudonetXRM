 BEGIN TRY  
      EXEC sp_changeobjectowner 'eudonet.xsp_DuplicateGridFromGrid', 'dbo';  
END TRY  
BEGIN CATCH  
      
END CATCH 
