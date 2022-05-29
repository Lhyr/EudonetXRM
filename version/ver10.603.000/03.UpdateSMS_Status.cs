using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using Com.Eudonet.Internal.SpecifTools;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

public class VersionFile
{
    /// <summary>
    /// Création du champ status
    /// </summary>
    /// <param name="sender"></param>
    public static void Upgrade(object sender)
    {
        string sql = string.Empty;
        string error = string.Empty;
        eUpgrader upgraderSender = (eUpgrader)sender;
        eudoDAL eDal = upgraderSender.EDal;
        ePrefSQL prefsql = upgraderSender.Pref;
        string sError;
        try
        {
            sql = @"SELECT top 1 USERID FROM [USER] WHERE USERLEVEL >= 99 and IsNull(UserDisabled ,0) = 0 order by UserLevel desc";
            int UID = eDal.ExecuteScalar<int>(new RqParam(sql), out sError);
            if (sError.Length > 0)
                throw eDal.InnerException ?? new EudoException(sError);


            ePrefUser prefuser = ePrefUser.GetNew(prefsql, new eUserInfo(UID, eDal));

            //récup de tous les templates sms sans le champ "status"
            sql = @"SELECT [tab].[DescId] FROM [DESC] tab
					left join [DESC] fld on fld.[File] = [tab].[File] and lower(fld.Field) = 'tpl29'
				WHERE [tab].[TYPE] = 4
				AND [fld].[descid] is null";

            RqParam rq = new RqParam(sql);

            var dt = eDal.Execute(rq);
            if (eDal.InnerException != null)
                throw eDal.InnerException;

            if (dt.HasRows)
            {
                while (dt.Read())
                {
                    int nDescId = dt.GetEudoNumeric("descid");
                    //création du champ
                    eAdminFieldInfos fld = new eAdminFieldInfos(prefuser, nDescId + (int)MailField.DESCID_SMS_STATUS);
                    fld.Format = FieldFormat.TYP_CHAR;
                    fld.PopupType = PopupType.DATA;
                    fld.ReadOnly = true;//modif uniquement via eudoprocess / job maj
                    fld.SuperAdminOnly = true;
                    fld.NoAdmin = true;
                    fld.CatSystem = true;

                    fld.Labels = new Dictionary<int, string>();
                    fld.Labels.Add(0, eResApp.GetRes(prefuser, 1755));


                    var result = fld.Create(eDal, out sError);

                    //création valeur de catalogue
                    StringBuilder sbError = new StringBuilder();
                    eCatalog cat = new eCatalog(eDal, prefuser, PopupType.DATA, prefuser.User, nDescId + (int)MailField.DESCID_SMS_STATUS);

                    InsertCatValue(cat, 2508, "LM_0", ref sbError, iResIdToolTIp: 2537, _pref: prefuser); //Envoi OK
                    InsertCatValue(cat, 2509, "LM_300001", ref sbError, iResIdToolTIp: 2538, _pref: prefuser); //Posté
                    InsertCatValue(cat, 2510, "LM_301000", ref sbError, iResIdToolTIp: 2539, _pref: prefuser); //Acquis
                    InsertCatValue(cat, 2511, "LM_300130", ref sbError, iResIdToolTIp: 2540, _pref: prefuser); //Rejet opérateur

                    InsertCatValue(cat, 2512, "LM_302000", ref sbError, iResIdToolTIp: 2541, _pref: prefuser);  // délivré
                    InsertCatValue(cat, 2513, "LM_302001", ref sbError, iResIdToolTIp: 2542, _pref: prefuser);  // non délivré
                    InsertCatValue(cat, 2514, "LM_302004", ref sbError, iResIdToolTIp: 2543, _pref: prefuser); // mobile non dispo
                    InsertCatValue(cat, 2515, "LM_", ref sbError, iResIdToolTIp: 2544, _pref: prefuser);      // adr non valide
                    InsertCatValue(cat, 2516, "LM_900104", ref sbError, iResIdToolTIp: 2544, _pref: prefuser); // num non valide

                    InsertCatValue(cat, 2517, "LM_302003", ref sbError, iResIdToolTIp: 2545, _pref: prefuser); //num non attribué
                    InsertCatValue(cat, 2518, "LM_300160", ref sbError, iResIdToolTIp: 2546, _pref: prefuser);  //msg trop long
                    InsertCatValue(cat, 2519, "LM_300170", ref sbError, iResIdToolTIp: 2547, _pref: prefuser);  //sms rejetté
                    InsertCatValue(cat, 2520, "LM_300020", ref sbError, iResIdToolTIp: 2548, _pref: prefuser);  // Connect erreur
                    InsertCatValue(cat, 2521, "LM_900100", ref sbError, iResIdToolTIp: 2549, _pref: prefuser);  // blacklist
                    InsertCatValue(cat, 2521, "LM_900101", ref sbError, iResIdToolTIp: 2549, _pref: prefuser);
                    InsertCatValue(cat, 2522, "LM_900302", ref sbError, iResIdToolTIp: 2550, _pref: prefuser);  //Vos trop sms
                    InsertCatValue(cat, 2523, "LM_300140", ref sbError, iResIdToolTIp: 2551, _pref: prefuser);  // sms incorcete
                    InsertCatValue(cat, 2524, "LM_300150", ref sbError, iResIdToolTIp: 2552, _pref: prefuser);  //from incorrect            
                    InsertCatValue(cat, 2525, "LM_300161", ref sbError, iResIdToolTIp: 2553, _pref: prefuser);  //car invalide
                    InsertCatValue(cat, 2526, "LM_302002", ref sbError, iResIdToolTIp: 2554, _pref: prefuser);  // pas de repport
                    InsertCatValue(cat, 2527, "LM_900000", ref sbError, iResIdToolTIp: 2555, _pref: prefuser);  // en attente sms
                    InsertCatValue(cat, 2215, "LM_900009", ref sbError, iResIdToolTIp: 2556, _pref: prefuser);  // Annuké
                    InsertCatValue(cat, 2528, "LM_900500", ref sbError, iResIdToolTIp: 2557, _pref: prefuser);  // stop base
                    InsertCatValue(cat, 2529, "LM_900105", ref sbError, iResIdToolTIp: 2558, _pref: prefuser);  //N filtré
                    InsertCatValue(cat, 2530, "LM_900200", ref sbError, iResIdToolTIp: 2559, _pref: prefuser);  //bloqué pays
                    InsertCatValue(cat, 2531, "LM_302010", ref sbError, iResIdToolTIp: 2560, _pref: prefuser); //délivré+stop
                    InsertCatValue(cat, 2532, "LM_300010", ref sbError, iResIdToolTIp: 2561, _pref: prefuser); //mt profile
                    InsertCatValue(cat, 2533, "LM_300019", ref sbError, iResIdToolTIp: 2562, _pref: prefuser);  //queued for delivery
                    InsertCatValue(cat, 2534, "LM_300162", ref sbError, iResIdToolTIp: 2563, _pref: prefuser);  // msg vide
                    InsertCatValue(cat, 2535, "LM_302005", ref sbError, iResIdToolTIp: 2564, _pref: prefuser);  //transiftion portabilité
                    InsertCatValue(cat, 2536, "LM_302030", ref sbError, iResIdToolTIp: 2565, _pref: prefuser);  //délivré + click

                    //tâche #2 673 : rajouter un statut par défaut à la création de fiche SMS
					int id = InsertCatValue(cat, 6546, "Default", ref sbError, iResIdToolTIp: 2585, _pref: prefuser);  //Envoi en cours

					eAdminDescInternal adminDesc = new eAdminDescInternal(nDescId + (int)MailField.DESCID_SMS_STATUS);
					adminDesc.SetDesc(eLibConst.DESC.DEFAULT, id.ToString());
                    adminDesc.SetDesc(eLibConst.DESC.DEFAULTFORMAT, "0");
					adminDesc.Save(prefuser, eDal, out sError);
                    if (sError.Length > 0)
                        sbError.Append(sError);

			
                    if (sbError.Length > 0)
                    {
                        sError = String.Concat("Une erreur s'est produite lors de la génération des catalogues SMS status : ", sbError.ToString());
                        throw new EudoException(sError);
                    }
                }
            }

            if (error.Length > 0)
            {
                throw new XRMUpgradeException(error, "ExtensionLinkMobility : Mise à jour dans la table Extension", "10.603.0000");
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


    protected static int InsertCatValue(eCatalog cat,
    Int32 iResId,
    string sData,
    ref StringBuilder sbError,
     ePrefUser _pref,
    Int32 iParentId = -1,
    int iResIdToolTIp = 0
    )
    {
        String sError = "";
        int catId = 0;

        Dictionary<String, String> diLabels = new Dictionary<String, string>();
        Dictionary<String, String> diLabelsTT = new Dictionary<String, string>();
        for (int i = 0; i < eLibConst.MAX_LANG; i++)
        {
            diLabels.Add(String.Concat("LANG_", i.ToString().PadLeft(2, '0')), eResApp.GetRes(i, iResId));
            diLabelsTT.Add(String.Concat("TIP_LANG_", i.ToString().PadLeft(2, '0')), iResIdToolTIp > 0 ? eResApp.GetRes(i, iResIdToolTIp) : "");
        }

        eCatalog.CatalogValue catVal = new eCatalog.CatalogValue(0, iParentId, eResApp.GetRes(_pref, iResId), sData);

        eCatalog.CatalogValue newCatValue = cat.Insert(catVal, out sError, diLabels, diLabelsTT);
        if (sError.Length > 0)
            sbError.AppendLine(sError);
        if (newCatValue != null)
            catId = newCatValue.Id;

        return catId;
    }

}
