using System;
using EudoQuery;
using Com.Eudonet.Xrm;
using Com.Eudonet.Internal;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Com.Eudonet.Core.Model;

public class VersionFile
{
    public static void Upgrade(Object sender)
    {

        eUpgrader upgraderSender = (eUpgrader)sender;
        upgraderSender.AddReferenceAssembly(typeof(System.Web.HttpContext));

        eudoDAL _eDal = upgraderSender.EDal;
        ePrefSQL _pref = upgraderSender.Pref;

        bool bInternalOpen = false;
        string error = string.Empty;
        System.Text.StringBuilder errorLog = new System.Text.StringBuilder();
	    DataTableReaderTuned dtr = null;       
	
    	try
        {         
            if (!_eDal.IsOpen)
			{
		     	bInternalOpen = true;
			   _eDal.OpenDatabase();            }
			
			// On vérifie les contraintes par défaut non conforme à DESC
			// Ici le catalog unitaire ne doit pas avoir une contrainte par défaut
            string sSqlverif = @"
				select  d.[File], d.Field, d.DescId, d.Format, d.Popup, d.Multiple, c.name cnme, dc.name dcname, dc.type dctype 
				from sys.default_constraints dc
				inner join sys.columns c on dc.parent_column_id = c.column_id and c.object_id = dc.parent_object_id
				inner join sys.tables t on t.object_id = c.object_id 
				inner join [desc] d on d.Field = c.name and d.[File] = t.name and format=1 and [POPUP]=3 and isnull(d.Multiple,0) = 0
				where dc.type ='D'			
			 ";
           
		    RqParam rqVerif = new RqParam(sSqlverif); 
		
			
			dtr = _eDal.Execute(rqVerif, out error);
			if (!String.IsNullOrEmpty(error))
				throw new Exception(error);
			
			if (dtr == null)
				return;

			string command = @"
			BEGIN TRY
                /*On tente de netoyer*/			
				ALTER TABLE {0} DROP CONSTRAINT {1};				
  		    END TRY
            BEGIN CATCH				
            END CATCH
			";
            string requieredDefaultName = string.Empty;
			string defaultConstraintName = string.Empty;
			while (dtr.Read())
			{
			    requieredDefaultName = string.Concat("__DF__",  dtr.GetEudoNumeric("DescId"));
				defaultConstraintName = dtr.GetString("dcname");
				
				// Si la contrainte coresspond on la supprime
				if (requieredDefaultName.Equals(defaultConstraintName))
				{
     				_eDal.ExecuteNonQuery(new RqParam(string.Format(command, dtr.GetString("File"), defaultConstraintName)), out error);
					if (!String.IsNullOrEmpty(error))
						throw new Exception(error);
				}
			}            
        }
        finally
        { 
		  if (dtr != null)
                dtr.Dispose();	
		
            if (bInternalOpen &&  _eDal != null)
                _eDal.CloseDatabase();
        }
    }
}