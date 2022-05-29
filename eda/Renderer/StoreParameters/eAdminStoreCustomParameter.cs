using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;
using System.Web.UI.HtmlControls;
using System.Collections.Generic;
using Com.Eudonet.Internal.eda;
using EudoCommonInterface.mailchecker;
using Com.Eudonet.Internal.mailchecker;
using System.Web.UI;
using Com.Eudonet.Common.Enumerations;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Renderer du module d'administration de IP Dédiée
    /// </summary>
    public class eAdminStoreCustomParameter : eAdminStoreSpecifRenderer
    {
        //eDedicatedIpSetting _dedicatedIpSetting = new eDedicatedIpSetting();

        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public eAdminStoreCustomParameter(ePref pref, eAdminExtension extension) : base(pref, extension)
        {

        }

        /// <summary>
        /// Création du rendu mode Fiche de l'extension IP Dédiée
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="ext"></param>
        /// <returns></returns>
        public static eAdminStoreCustomParameter CreateAdminStoreCustomParamRenderer(ePref pref, eAdminExtension ext)
        {
            if (pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                throw new EudoAdminInvalidRightException();

          
            eAdminStoreCustomParameter rdr = new eAdminStoreCustomParameter(pref, ext);

            rdr.Generate();
            return rdr;
        }

        /// <summary>
        /// Initialisation des params
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            if (base.Init())
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Build des params
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {
            if (base.Build())
            {
              
                return true;
            }
            return false;
        }

     
 
        /// <summary>
        /// écran de paramétrage
        /// </summary>
        /// <returns></returns>
        protected override void CreateSettingsPanel()
        {
            base.CreateSettingsPanel();           

            Panel customPanel = new Panel();

            customPanel.ID = "ExtensionStore";
            customPanel.Style.Add("display", "flex");
            customPanel.Style.Add("align-items", "center");

            Label labelParams = new Label();
            labelParams.Attributes.Add("name", "params");
            labelParams.Text = eResApp.GetRes(Pref, 3066);
            labelParams.Style.Add("margin-left", " 50px");
            labelParams.Style.Add("margin-right", " 50px");
            
            customPanel.Controls.Add(labelParams);

            HtmlTextArea eparamsInput = new HtmlTextArea();
            eparamsInput.ID = "paramsInput";
            eparamsInput.Name = "paramsInput";
            eparamsInput.Rows = 10;
            eparamsInput.Cols = 20;
            eparamsInput.Style.Add("height", "250px");
            eparamsInput.Style.Add("width", "535px");
            eparamsInput.Value = _eRegisteredExt.Param;
            eparamsInput.Attributes.Add("onchange", "nsAdmin.updateParams(this,"+ _eRegisteredExt.Id + ");");
            customPanel.Controls.Add(eparamsInput);

            ExtensionParametersContainer.Controls.Add(customPanel);
        }

    }
}