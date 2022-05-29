using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Data;
public class VersionFile
{
    /// <summary>
    /// Mise à jour du les paramètres de sms linkmobility dans la table EXTENSION a partir du ConfigADV
    /// </summary>
    /// <param name="sender"></param>
    public static void Upgrade (object sender)
    {
        string sql = string.Empty;
        string error = string.Empty;
        eUpgrader upgraderSender = (eUpgrader)sender;
        eudoDAL eDal = upgraderSender.EDal;
		    ePrefSQL prefsql = upgraderSender.Pref;
			
		         sql = @"SELECT top 1 USERID FROM [USER] WHERE USERLEVEL >= 99 and IsNull(UserDisabled ,0) = 0 order by UserLevel desc";
            int UID = eDal.ExecuteScalar<int>(new RqParam(sql), out error);
            if (error.Length > 0)
                throw eDal.InnerException ?? new EudoException(error);


            ePrefUser prefuser = ePrefUser.GetNew(prefsql, new eUserInfo(UID, eDal));
			
			
 
        const string sSmsNetMessage = "SMS_NETMESSAGE";
        const string ConfigADVSmsSettings = "SMS_SETTINGS";

        try
        {
            sql = "IF EXISTS((SELECT 1 FROM EXTENSION WHERE EXTENSIONCODE = @ExtensionCode))" + Environment.NewLine
               + "BEGIN" + Environment.NewLine

               + "set @ExtensionParam = ( select  CONFIGADV.Value FROM CONFIGADV WHERE Parameter = @ConfigADVSmsSettings);" + Environment.NewLine

               + "IF (@ExtensionParam <> '')" + Environment.NewLine
               + "BEGIN" + Environment.NewLine

               + "UPDATE [EXTENSION] SET EXTENSIONPARAM = @ExtensionParam WHERE EXTENSIONCODE = @ExtensionCode  AND EXTENSIONSTATUS = @ExtensionStatus " + Environment.NewLine
               + "AND (EXTENSIONPARAM = ''  OR EXTENSIONPARAM = NULL OR EXTENSIONPARAM = '{}')" + Environment.NewLine

               + "END" + Environment.NewLine
               + "END";

            RqParam rq = new RqParam(sql);
            rq.AddInputParameter("@ExtensionCode", SqlDbType.VarChar, sSmsNetMessage);
            rq.AddInputParameter("@ExtensionStatus", SqlDbType.VarChar, EXTENSION_STATUS.STATUS_READY);
            rq.AddInputParameter("@ExtensionParam", SqlDbType.NVarChar, "");
            rq.AddInputParameter("@ConfigADVSmsSettings", SqlDbType.VarChar, ConfigADVSmsSettings);

            eDal.ExecuteNonQuery(rq, out error);

            if (error.Length > 0)
            {
                throw new XRMUpgradeException(error, "EnxtensionLinkMobility : Mise à jour dans la table Extension", "10.603.0000");
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
