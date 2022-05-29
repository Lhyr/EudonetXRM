using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Internal;
using Newtonsoft.Json;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Extension correspondant à l'interface de paramétrage de la synchronisation Exchange version Office 365 (API Graph)
    /// </summary>
    public class eAdminExtensionSynchroExchange : eAdminExtension
    {
        /// <summary>
        /// Constructeur principal
        /// </summary>
        /// <param name="infos"></param>
        [JsonConstructor]
        public eAdminExtensionSynchroExchange(eAdminExtensionInfo infos) : base()
        {
            this.Infos = infos;
        }


        /// <summary>
        /// Constructeur avec Pref et indication explicite de l'extension fille concernée
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="module"></param>
        public eAdminExtensionSynchroExchange(ePref pref, eUserOptionsModules.USROPT_MODULE module) : base(pref, module)
        {
            string strError = String.Empty;
        }

        /// <summary>
        /// Constructeur avec Pref
        /// </summary>
        /// <param name="pref"></param>
        public eAdminExtensionSynchroExchange(ePref pref) : base(pref, eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO_OFFICE365)
        {
            string strError = String.Empty;
        }

        /// <summary>
        /// Vérifie si l'extension est activée en base
        /// </summary>
        /// <param name="sError"></param>
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

        /// <summary>
        /// Activation de l'extension
        /// </summary>
        /// <param name="bEnable"></param>
        /// <param name="sError"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Après activation de l'extension
        /// </summary>
        /// <param name="bEnable"></param>
        /// <param name="sError"></param>
        /// <returns></returns>
        public override bool AfterEnableProcess(bool bEnable, out string sError)
        {
            bool bReturnValue = true;
            sError = String.Empty;

            return bReturnValue;
        }

        /// <summary>
        /// Renvoie les informations à afficher dans l'encart supérieur de la fiche Extension
        /// </summary>
        /// <returns></returns>
        public override List<HtmlGenericControl> GetModuleInfo()
        {
            List<HtmlGenericControl> infoList = new List<HtmlGenericControl>();
            return infoList;
        }



        /// <summary>
        /// Pour les synchros, il y a un panneau paramètres
        /// </summary>
        public override bool ShowParametersTab
        {
            get
            {
                return true;
            }
        }
    }

    
}