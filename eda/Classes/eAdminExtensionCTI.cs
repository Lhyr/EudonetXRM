using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminExtensionCTI : eAdminExtension
    {
        Dictionary<eLibConst.CONFIG_DEFAULT, string> dicoConfig;

        [JsonConstructor]
        public eAdminExtensionCTI(eAdminExtensionInfo infos) : base()
        {
            this.Infos = infos;
        }

        public eAdminExtensionCTI(ePref pref) : base(pref, eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_CTI)
        {
            dicoConfig = _pref.GetConfigDefault(new eLibConst.CONFIG_DEFAULT[]
                { eLibConst.CONFIG_DEFAULT.CTIENABLED,
                    eLibConst.CONFIG_DEFAULT.CTIDevice
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
        /// Indique si l'extension comporte des paramètres à afficher dans l'onglet Paramètres
        /// </summary>
        public override bool ShowParametersTab
        {
            get
            {
                return true;
            }
        }

        public override bool IsEnabledProcess(out string sError)
        {
            bool bReturnValue = false;
            sError = String.Empty;

            
            try
            {
                bReturnValue = dicoConfig[eLibConst.CONFIG_DEFAULT.CTIENABLED] == "1";
            }
            catch (Exception e)
            {
                sError = String.Concat(e.Message, Environment.NewLine, e.StackTrace);
            }

            return bReturnValue;
        }

        /// <summary>
        /// Active l'extension
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
                string sEnable = bEnable ? "1" : "0";

                Pref.SetConfigDefault(new List<SetParam<String>>() { new SetParam<String>(eLibConst.CONFIG_DEFAULT.CTIENABLED.ToString(), sEnable) });

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

                dicoConfig[eLibConst.CONFIG_DEFAULT.CTIENABLED] = sEnable;

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

            string sDevice = dicoConfig[eLibConst.CONFIG_DEFAULT.CTIDevice];
            int nDevice = 0;
            eLibConst.CTI_DEVICE_TYPE typ = eLibConst.CTI_DEVICE_TYPE.CTI_DEVICE_CUSTOM;
            if (int.TryParse(sDevice, out nDevice) && Enum.IsDefined(typeof(eLibConst.CTI_DEVICE_TYPE), nDevice))
                typ = (eLibConst.CTI_DEVICE_TYPE)nDevice;

            string deviceLabel = eAdminTools.GetCTITypeLabel(Pref, typ);

            #region Device
            HtmlGenericControl spanInfo = new HtmlGenericControl("span");
            HtmlGenericControl spanInfoLabel = new HtmlGenericControl("span");
            HtmlGenericControl spanInfoValue = new HtmlGenericControl("span");
            spanInfoLabel.Attributes.Add("class", "extensionInfoLabel");
            spanInfoValue.Attributes.Add("class", "extensionInfoValue");
            spanInfoLabel.InnerHtml = String.Concat(eResApp.GetRes(Pref, 7788), " : ");
            spanInfoValue.InnerHtml = deviceLabel;
            spanInfo.Controls.Add(spanInfoLabel);
            spanInfo.Controls.Add(spanInfoValue);
            infoList.Add(spanInfo);
            #endregion

            return infoList;
        }
    }
}