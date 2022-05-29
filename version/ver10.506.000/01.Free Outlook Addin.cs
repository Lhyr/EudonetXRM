using Com.Eudonet.Internal;
using Com.Eudonet.Xrm;
using EudoExtendedClasses;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Text;
using Com.Eudonet.Core.Model;
using System.Data;


public class VersionFile
{
    /// <summary>
    ///  Mise à jour du mappin pour l'addin outlook à partir du mapping mobile
    /// </summary>
    /// <param name="sender"></param>
    public static void Upgrade(Object sender)
    {
        eUpgrader upgraderSender = (eUpgrader)sender;
        //ePrefSQL pref = upgraderSender.Pref;
        eudoDAL eDal = upgraderSender.EDal;

        const string sOutlookAddinCode = "OUTLOOKADDIN";
        String error = String.Empty;
        string sql = "";
        try
        {
            eOutlookAddinSetting setting = new eOutlookAddinSetting();
            setting.LoadFromMobileMapping(eDal);
            int iExtId;

            //Extension a créer
            sql = "INSERT INTO [EXTENSION](EXTENSIONTYPE, EXTENSIONCODE, EXTENSIONSTATUS, EXTENSIONPARAM, EXTENSION95)" + Environment.NewLine
                    + "SELECT @ExtensionType, @ExtensionCode, @ExtensionStatus, @ExtensionParam, GetDate()" + Environment.NewLine
                    + "WHERE NOT EXISTS (SELECT EXTENSIONID FROM [EXTENSION] WHERE EXTENSIONCODE = @ExtensionCode);" + Environment.NewLine
                    + "UPDATE [EXTENSION] SET EXTENSIONPARAM = @ExtensionParam, EXTENSIONACTIVATIONKEY=NULL WHERE EXTENSIONCODE = @ExtensionCode AND (ISNULL(EXTENSIONPARAM,'') = '{}' OR ISNULL(EXTENSIONPARAM,'') = '') ";

            RqParam rq = new RqParam(sql);
            rq.AddInputParameter("@ExtensionType", SqlDbType.VarChar, EXTENSION_TYPE.ADMIN_EXTENSIONS_FROMSTORE);
            rq.AddInputParameter("@ExtensionCode", SqlDbType.VarChar, sOutlookAddinCode);
            rq.AddInputParameter("@ExtensionStatus", SqlDbType.VarChar, EXTENSION_STATUS.STATUS_NON_INSTALLED);
            rq.AddInputParameter("@ExtensionParam", SqlDbType.VarChar, setting.ToJsonString());



            eDal.ExecuteNonQuery(rq, out error);

            if (error.Length > 0)
            {
                throw new XRMUpgradeException(error, "FreeOutlookAddin : Insertion/Mise à jour dans la table Extension", "10.506.0000");
            }



        }
        catch
        {
            throw;
        }
        finally
        {
        }
    }
}