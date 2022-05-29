using Com.Eudonet.Internal;
using EudoQuery;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminExtensionFromStore : eAdminExtension
    {
        Dictionary<eLibConst.CONFIG_DEFAULT, string> dicoConfig;

        public eAdminExtensionFromStore() : base()
        {

        }

        [JsonConstructor]
        public eAdminExtensionFromStore(eAdminExtensionInfo infos) : base()
        {
            this.Infos = infos;
        }

        public eAdminExtensionFromStore(ePref pref) : base(pref, eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_FROMSTORE)
        {
            dicoConfig = _pref.GetConfigDefault(new eLibConst.CONFIG_DEFAULT[]
                { /*eLibConst.CONFIG_DEFAULT.MOBILEENABLED*/ });
        }



        /// <summary>
        /// Indique si l'extension comporte des paramètres à afficher dans l'onglet Paramètres
        /// </summary>
        public override bool ShowParametersTab
        {
            get
            {
                if (Infos != null)
                    return Infos.HasCustomParameter && Pref.User.UserLevel >= UserLevel.LEV_USR_SUPERADMIN.GetHashCode();

                return false; // A ajuster selon un paramètre récupéré via l'API
            }
        }

        public override bool EnableProcess(bool bEnable, out string sError)
        {
            bool bReturnValue = false;
            sError = String.Empty;

            try
            {
                string sEnable = bEnable ? "1" : "0";



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

                // A ajuster selon un paramètre récupéré via l'API
                //dicoConfig[eLibConst.CONFIG_DEFAULT.MOBILEENABLED] = sEnable;

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

            return infoList;
        }

        /// <summary>
        /// Vérifie  si l'extension est activée
        /// </summary>
        /// <param name="sError"></param>
        /// <returns></returns>
        public override bool IsEnabledProcess(out string sError)
        {
            sError = "";

            if (Infos == null)
                return false;



            //status global
            if (Infos.Status == EXTENSION_STATUS.STATUS_DISABLED
                || Infos.Status == EXTENSION_STATUS.STATUS_DESACTIVATION_ASKED
                || Infos.Status == EXTENSION_STATUS.STATUS_NON_INSTALLED
                || Infos.Status == EXTENSION_STATUS.STATUS_UNDEFINED
                )
                Infos.IsEnabled = false;
            else
                Infos.IsEnabled = true;

            return Infos.IsEnabled;
        }
    }
}