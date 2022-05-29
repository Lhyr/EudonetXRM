using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Internal;
using EudoQuery;
using Newtonsoft.Json;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminExtensionSnapshot : eAdminExtension
    {
        private IDictionary<eLibConst.CONFIG_DEFAULT, string> dicConfig;

        [JsonConstructor]
        public eAdminExtensionSnapshot(eAdminExtensionInfo infos) : base()
        {
            this.Infos = infos;
        }

        public eAdminExtensionSnapshot(ePref pref) : base(pref, eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SNAPSHOT)
        {
            dicConfig = pref.GetConfigDefault(
                new HashSet<eLibConst.CONFIG_DEFAULT> {
                        eLibConst.CONFIG_DEFAULT.EUDOSNAPENABLED,
                        eLibConst.CONFIG_DEFAULT.SNAPSTATUS,
                        eLibConst.CONFIG_DEFAULT.SNAPID
                });
        }

        /// <summary>
        /// Indique si l'intégralité du mode Fiche de l'extension (notamment l'onglet Paramètres) doit être rafraîchi après activation/désactivation
        /// </summary>
        public override bool NeedsFullRefreshAfterEnable
        {
            get
            {
                return false;
            }
        }

 

        /// <summary>
        /// Effectue le traitement pour vérifier si l'extension est activée
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="dal"></param>
        /// <param name="strError"></param>
        /// <returns></returns>
        public override bool IsEnabledProcess(out string sError)
        {
            bool bSnapshotEnabled = false;
            sError = String.Empty;

            try
            {
                bSnapshotEnabled = dicConfig[eLibConst.CONFIG_DEFAULT.EUDOSNAPENABLED] == "1";
            }
            catch (Exception e)
            {
                sError = String.Concat(e.Message, Environment.NewLine, e.StackTrace);
            }
            return bSnapshotEnabled;
        }

        public override bool EnableProcess(bool bEnable, out string sError)
        {
            bool bReturnValue = false;
            sError = String.Empty;

            try
            {
                string sEnable = bEnable ? "1" : "0";

                Pref.SetConfigDefault(new List<SetParam<String>>() { new SetParam<String>(eLibConst.CONFIG_DEFAULT.EUDOSNAPENABLED.ToString(), sEnable) });

                bReturnValue = true;
            }
            catch (Exception e)
            {
                sError = String.Concat(e.Message, Environment.NewLine, e.StackTrace);
            }

            return bReturnValue;
        }

        public override bool AfterEnableProcess(bool bEnable, out string sError)
        {
            bool bReturnValue = false;
            sError = String.Empty;

            try
            {
                string sEnable = bEnable ? "1" : "0";

                dicConfig[eLibConst.CONFIG_DEFAULT.EUDOSNAPENABLED] = sEnable;

                bReturnValue = true;
            }
            catch (Exception e)
            {
                sError = String.Concat(e.Message, Environment.NewLine, e.StackTrace);
            }

            return bReturnValue;
        }

        public override List<HtmlGenericControl> GetModuleInfo()
        {
            List<HtmlGenericControl> infoList = new List<HtmlGenericControl>();

            infoList.Add(GetSnapshotStatusLabel());
            infoList.Add(GetSnapshotIDLabel());

            return infoList;
        }

        public HtmlGenericControl GetSnapshotStatusLabel()
        {
            HtmlGenericControl statusDiv = new HtmlGenericControl("div");

            string statusDescription = String.Empty;
            eLibConst.EUDOSNAP_STATUS status = (eLibConst.EUDOSNAP_STATUS)eLibTools.GetNum(dicConfig[eLibConst.CONFIG_DEFAULT.SNAPSTATUS]);
            switch (status)
            {
                case eLibConst.EUDOSNAP_STATUS.PROD_NORMAL: statusDescription = eResApp.GetRes(_pref, 7892); break; // "base de production administrable, non liée à une base de recette"; break;
                case eLibConst.EUDOSNAP_STATUS.PROD_READONLY: statusDescription = eResApp.GetRes(_pref, 7893); break; // "base de production non administrable, liée à une base de recette en cours de paramétrage"; break;
                case eLibConst.EUDOSNAP_STATUS.VALID_PENDING: statusDescription = eResApp.GetRes(_pref, 7894); break; // "base de recette en attente de préparation, sans fonctionnalités de traçage"; break;
                case eLibConst.EUDOSNAP_STATUS.VALID_NORMAL: statusDescription = eResApp.GetRes(_pref, 7895); break; // "base de recette en cours de paramétrage, avec fonctionnalités de traçage"; break;
            }

            if (statusDescription.Length > 0)
            {
                HtmlGenericControl statusLabel = new HtmlGenericControl("span");
                statusLabel.Attributes.Add("class", "extensionInfoLabel");
                statusLabel.InnerHtml = String.Concat(eResApp.GetRes(_pref, 7891), " ", statusDescription, "."); // Vous utilisez actuellement une
                statusDiv.Controls.Add(statusLabel);
            }

            return statusDiv;
        }

        public HtmlGenericControl GetSnapshotIDLabel()
        {
            HtmlGenericControl idDiv = new HtmlGenericControl("div");

            if (dicConfig[eLibConst.CONFIG_DEFAULT.SNAPID].Trim().Length > 0)
            {
                HtmlGenericControl idLabel = new HtmlGenericControl("span");
                idLabel.Attributes.Add("class", "extensionInfoLabel");
                idLabel.InnerHtml = eResApp.GetRes(_pref, 7896); //  "L'identifiant actuel de la base est";
                idDiv.Controls.Add(idLabel);

                HtmlGenericControl idValue = new HtmlGenericControl("span");
                idValue.Attributes.Add("class", "extensionInfoValue");
                idValue.InnerHtml = String.Concat("&nbsp;", dicConfig[eLibConst.CONFIG_DEFAULT.SNAPID]);
                idDiv.Controls.Add(idValue);
            }

            return idDiv;
        }
    }
}