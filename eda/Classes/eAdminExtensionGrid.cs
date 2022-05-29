using Com.Eudonet.Internal;
using EudoQuery;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{


    public class eAdminExtensionGrid : eAdminExtension
    {
        Dictionary<eLibConst.CONFIG_DEFAULT, string> dicoConfig;

        [JsonConstructor]
        public eAdminExtensionGrid(eAdminExtensionInfo infos) : base()
        {
            this.Infos = infos;
        }

        public eAdminExtensionGrid(ePref pref) : base(pref, eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_GRID)
        {
            dicoConfig = _pref.GetConfigDefault(new eLibConst.CONFIG_DEFAULT[]
                { eLibConst.CONFIG_DEFAULT.GRIDENABLED });
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
        /// Indique si l'extension comporte des paramètres à afficher dans l'onglet Paramètres
        /// </summary>
        public override bool ShowParametersTab
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Vérifie si l'extension est activée en base
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="dal"></param>
        /// <param name="strError"></param>
        /// <returns></returns>
        public override bool IsEnabledProcess(out string sError)
        {
            sError = String.Empty;

            try
            {
                // Impossible de vérifier si l'extension est activé si on a pas les infos
                if (Infos == null)
                {

                    return false;
                }

                List<eExtension> lst = eExtension.GetExtensionsByCode(_pref, Infos.ExtensionNativeId);
                if (lst.Count == 0)
                {
                    Infos.IsEnabled = false;
                }
                else
                {
                    var userDef = lst.Find(aa => aa.UserId == 0);
                    if (userDef != null)
                    {

                        //status global
                        if (userDef.Status == EXTENSION_STATUS.STATUS_DISABLED
                            || userDef.Status == EXTENSION_STATUS.STATUS_DESACTIVATION_ASKED
                            || userDef.Status == EXTENSION_STATUS.STATUS_NON_INSTALLED
                            || userDef.Status == EXTENSION_STATUS.STATUS_UNDEFINED
                            )
                            Infos.IsEnabled = false;
                        else
                            Infos.IsEnabled = true;

                        Infos.Status = userDef.Status;
                    }
                    else
                    {
                        Infos.IsEnabled = false;

                        Infos.Status = EXTENSION_STATUS.STATUS_NON_INSTALLED;
                    }
                }
            }
            catch (Exception e)
            {
                sError = String.Concat(e.Message, Environment.NewLine, e.StackTrace);
            }
            return Infos.IsEnabled;
        }


        public override bool EnableProcess(bool bEnable, out string sError)
        {
            bool bReturnValue = false;
            sError = String.Empty;

            try
            {
                string sEnable = bEnable ? "1" : "0";

                Pref.SetConfigDefault(new List<SetParam<String>>() { new SetParam<String>(eLibConst.CONFIG_DEFAULT.GRIDENABLED.ToString(), sEnable) });

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

                dicoConfig[eLibConst.CONFIG_DEFAULT.GRIDENABLED] = sEnable;

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
            List<Tuple<string, string>> infoData = new List<Tuple<string, string>>();

            infoData.Add(new Tuple<string, string>(eResApp.GetRes(_pref, 8067), eResApp.GetRes(_pref, 8068)));

            foreach (Tuple<string, string> info in infoData)
            {
                HtmlGenericControl spanInfo = new HtmlGenericControl("span");
                HtmlGenericControl spanInfoLabel = new HtmlGenericControl("span");
                HtmlGenericControl spanInfoValue = new HtmlGenericControl("span");
                spanInfoLabel.Attributes.Add("class", "extensionInfoLabel");
                spanInfoValue.Attributes.Add("class", "extensionInfoValue");
                spanInfoLabel.InnerHtml = String.Concat(info.Item1, "&nbsp;");
                spanInfoValue.InnerHtml = info.Item2;
                spanInfo.Controls.Add(spanInfoLabel);
                spanInfo.Controls.Add(spanInfoValue);
                infoList.Add(spanInfo);
            }

            return infoList;
        }
    }  

}