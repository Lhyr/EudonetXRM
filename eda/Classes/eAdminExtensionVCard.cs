using Com.Eudonet.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminExtensionVCard : eAdminExtension
    {
        Dictionary<eLibConst.CONFIG_DEFAULT, string> dicoConfig;

        [JsonConstructor]
        public eAdminExtensionVCard(eAdminExtensionInfo infos) : base()
        {
            this.Infos = infos;
        }

        public eAdminExtensionVCard(ePref pref) : base(pref, eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_VCARD)
        {
            dicoConfig = _pref.GetConfigDefault(new eLibConst.CONFIG_DEFAULT[]
                { eLibConst.CONFIG_DEFAULT.VCARDMAPPING });
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
                List<eExtension> a = eExtension.GetExtensionsByCode(_pref, Infos.ExtensionNativeId);
                eExtension newExt;
                if (a.Count == 0)
                {
                    newExt = eExtension.GetNewExtension(Infos.ExtensionNativeId);
                }
                else
                {
                    newExt = a.Find(zz => zz.UserId == 0);
                    if (newExt == null)
                        newExt = eExtension.GetNewExtension(Infos.ExtensionNativeId);
                }

                newExt.Status = bEnable ? EXTENSION_STATUS.STATUS_READY : EXTENSION_STATUS.STATUS_DISABLED;
                
                string rootPhysicalDatasPath = eModelTools.GetRootPhysicalDatasPath();
                eExtension.UpdateExtension(newExt, _pref, _pref.User, rootPhysicalDatasPath, _pref.ModeDebug);

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
            bool bReturnValue = true;
            sError = String.Empty;

            return bReturnValue;
        }

        public override List<HtmlGenericControl> GetModuleInfo()
        {
            List<HtmlGenericControl> infoList = new List<HtmlGenericControl>();
            return infoList;
        }

    }
}