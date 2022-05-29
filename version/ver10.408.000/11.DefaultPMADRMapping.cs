using Com.Eudonet.Internal;
using Com.Eudonet.Xrm;
using EudoExtendedClasses;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Text;
using Com.Eudonet.Core.Model;

public class VersionFile
{
    /// <summary>
    ///  #51630 Ajout de la colonne système géolocalisation descid=xx74
    /// </summary>
    /// <param name="sender"></param>
    public static void Upgrade(Object sender)
    {
        eUpgrader upgraderSender = (eUpgrader)sender;
        eudoDAL eDal = upgraderSender.EDal;

        String error = String.Empty;

        try
        {
            DescAdvDataSet descAdv = new DescAdvDataSet();
            descAdv.LoadAdvParams(eDal, new List<int>() { (int)TableType.ADR }, new List<DESCADV_PARAMETER>() { DESCADV_PARAMETER.PM_ADR_MAPPING });
            if (String.IsNullOrEmpty(descAdv.GetAdvInfoValue((int)TableType.ADR, DESCADV_PARAMETER.PM_ADR_MAPPING)))
            {
                ePmAddressMapping map = ePmAddressMapping.GetDefault();
                DescAdvObj obj = DescAdvObj.GetSingle((int)TableType.ADR, DESCADV_PARAMETER.PM_ADR_MAPPING, map.ToJsonString());
                obj.Save(eDal);
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