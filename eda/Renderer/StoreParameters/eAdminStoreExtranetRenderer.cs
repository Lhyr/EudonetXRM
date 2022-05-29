using System;
using System.Collections.Generic;
using Com.Eudonet.Internal;
using Com.Eudonet.Core.Model;
using EudoQuery;
using Com.Eudonet.Core.Model.extranet;
using Newtonsoft.Json;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal.eda;
using Com.Eudonet.Common.Cryptography;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Administration de l'extension Zapier
    /// </summary>
    public class eAdminStoreExtranetRenderer : eAdminStoreFileRenderer
    {
        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public eAdminStoreExtranetRenderer(ePref pref, eAdminExtension extension) : base(pref, extension)
        {
        }


        private eAdminExtensionExtranet MyExtension
        {
            get
            {
                return (eAdminExtensionExtranet)_extension;
            }
        }

        /// <summary>
        /// Création de l'écran
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="ext"></param>
        /// <returns></returns>
        public static eAdminStoreExtranetRenderer CreateAdminStoreExtranetRenderer(ePref pref, eAdminExtension ext)
        {
            if (pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                throw new EudoAdminInvalidRightException();

            eAdminStoreExtranetRenderer rdr = new eAdminStoreExtranetRenderer(pref, ext);

            rdr.Generate();
            return rdr;
        }

        protected override bool Init()
        {
            if (base.Init())
            {
                if (MyExtension.NeedSaveRegistredExt)
                    eExtension.UpdateExtension(_eRegisteredExt, _ePref, _ePref.User, eModelTools.GetRootPhysicalDatasPath(), _ePref.ModeDebug);

                return true;
            }

            return false;
        }


        protected override bool Build()
        {

            if (base.Build())
            {
                AddCallBackScript("nsAdmin.addScript('eExtranetAdmin', 'ADMIN_STORE');", true);
                return true;
            }


            return false;
        }

        /// <summary>
        /// Ecran de paramétrage
        /// </summary>
        protected override void CreateSettingsPanel()
        {
            base.CreateSettingsPanel();

            //Informations

            // eExtranetParam newEx = eExtranetParam.GetNewExtranetParam(_ePref, -1);
            //MyExtension.lstExtranet.Add(newEx);

            foreach (eExtranetParam ext in MyExtension.lstExtranet)
            {

                List<eBasicInputField> listInfos = new List<eBasicInputField>();

                int nbExtranet = ext.Id;
                bool bButtonRenewDisabled = false;
                string sToolTip = "";




                // listInfos.Add(new eBasicInputField() { Label = eResApp.GetRes(_ePref, 2467), Value = ext.Id.ToString() });

                //Si connexion via EudoAdmin, le loginid n'est pas renseigné.
                //En attente de correction de ce bug, désactivation du bouton de regen
                if (_ePref.LoginId == 0)
                {
                    bButtonRenewDisabled = true;
                    sToolTip = eResApp.GetRes(_ePref, 2466);
                }

                //Btn régenération
                eAdminField button = eAdminButtonField.GetEAdminButtonField(
                    new eAdminButtonParams()
                    {
                        Label = " Régénérer le token ",
                        ID = "RENEW_EXT_" + nbExtranet,
                        Disabled = bButtonRenewDisabled,
                        ToolTip = sToolTip,
                        OnClick = "nsExtranetAdmin.updateParam(this," + ext.Id + ", { needConfirm:true, msgTitle: top._res_2463, msgBody : top._res_2464  });",
                        Attributes = new List<Tuple<string, string>>() { new Tuple<string, string>("dsc", "21|5") }
                    });

                listInfos.Add(new eBasicInputField() { Label = eResApp.GetRes(_ePref, 2407), Value = ext.Token, Fld = button, EndId = "token_" + ext.Id }); // Clé Extranet


                //Vérification cohérence du token
                var def = new { Salt = "", DBUID = "", ExtranetID = 0, SubId = 0 };
                var t = JsonConvert.DeserializeAnonymousType(CryptoTripleDES.Decrypt(ext.Token, CryptographyConst.KEY_CRYPT_TOKEN), def);

                if (t.SubId == 0 || t.DBUID != _ePref.DatabaseUid || ext.Id != t.ExtranetID)
                    listInfos.Add(new eBasicInputField() { Label = eResApp.GetRes(_ePref, 2465), Value = "", EndId = "invalid_" + ext.Id });

                Panel section = GetInfosSection(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_EXTRANET.ToString() + "_" + nbExtranet,
                    eResApp.GetRes(_ePref, 2956).Replace("##NAME##", (string.IsNullOrEmpty(ext.Name) ? ext.Id.ToString() : ext.Name)), "", listInfos);

                if (ext.Id == -1)
                    section.Attributes.Add("display", "none");

                //Blocs infos
                ExtensionParametersContainer.Controls.Add(section);



                section.Attributes.Add("ednextid", ext.Id.ToString());





                Panel targetPanel = null;



                ExtensionParametersContainer.Controls.Add(section);

                if (section.Controls.Count > 0 && section.Controls[section.Controls.Count - 1] is Panel)
                    targetPanel = (Panel)section.Controls[section.Controls.Count - 1];
                if (targetPanel == null)
                    return;



                // Activé
                AddCheckboxOptionField(targetPanel, "chkActv_" + nbExtranet, "Activé", "",
                    eAdminUpdateProperty.CATEGORY.EXTRANETPARAMS, eLibConst.EXTRANETPARAMS.ISACTIVE.GetHashCode(),
                    typeof(eLibConst.CONFIGADV), ext.IsActive,
                    onClick: "nsExtranetAdmin.updateParam(this, " + ext.Id + " );"
                    )
                    ;


                //Nom
                string customTextboxCSSClasses = "optionField";
                string customPanelCSSClasses = "fieldInline";
                string customLabelCSSClasses = "labelField optionField";

                AddTextboxOptionField(targetPanel, "txtSMTPServerName_" + nbExtranet, "Nom", "",
                    eAdminUpdateProperty.CATEGORY.EXTRANETPARAMS, eLibConst.EXTRANETPARAMS.NAME.GetHashCode(),
                    typeof(eLibConst.EXTRANETPARAMS),
                    ext.Name, EudoQuery.AdminFieldType.ADM_TYPE_CHAR, eAdminTextboxField.LabelType.INLINE,
                      onChange: "nsExtranetAdmin.updateParam(this," + ext.Id + ");",
                    customPanelCSSClasses: customPanelCSSClasses,
                    customLabelCSSClasses: customLabelCSSClasses,
                    customTextboxCSSClasses: customTextboxCSSClasses);

                // TOML
                Panel pnlParams = new Panel();
                pnlParams.ID = "blockTOMLParams";
                AddTextboxOptionField(pnlParams, "txtToml_" + nbExtranet, "TOML", "",
                  eAdminUpdateProperty.CATEGORY.EXTRANETPARAMS, eLibConst.EXTRANETPARAMS.TOML.GetHashCode(),
                    typeof(eLibConst.CONFIGADV), ext.TOML, AdminFieldType.ADM_TYPE_CHAR,
                    eAdminTextboxField.LabelType.INLINE,
                    onChange: "nsExtranetAdmin.updateParam(this," + ext.Id + ");",

                    nbRows: 10,
                    customPanelCSSClasses: "fieldInline",
                    customLabelCSSClasses: "labelField optionField",
                    customTextboxCSSClasses: "xnetTOML");




                targetPanel.Controls.Add(pnlParams);

                //Max User
                AddTextboxOptionField(targetPanel, "txtExtranetMaxUser_" + nbExtranet, "Nombre maximum d'utilisateurs", "Entre 1 et 50",
                    eAdminUpdateProperty.CATEGORY.EXTRANETPARAMS, eLibConst.EXTRANETPARAMS.MAX_USER.GetHashCode(),
                    typeof(eLibConst.EXTRANETPARAMS),
                    ext.MaxConccurentUser.ToString(), EudoQuery.AdminFieldType.ADM_TYPE_CHAR, eAdminTextboxField.LabelType.INLINE,
                      onChange: "nsExtranetAdmin.updateParam(this," + ext.Id + ");",
                    customPanelCSSClasses: customPanelCSSClasses,
                    customLabelCSSClasses: customLabelCSSClasses,
                    customTextboxCSSClasses: customTextboxCSSClasses);




                eAdminField buttonDel = eAdminButtonField.GetEAdminButtonField(
                new eAdminButtonParams()
                {
                    Label = eResApp.GetRes(_ePref, 19),
                    ID = "DEL_EXTRANET_" + ext.Id,
                    Disabled = false,
                    OnClick = "nsExtranetAdmin.deleteExt(" + ext.Id + ");",

                });




                buttonDel.Generate(targetPanel);
                buttonDel.SetFieldAttribute("style", "display:block");
            }




            eAdminField buttonAdd = eAdminButtonField.GetEAdminButtonField(
                new eAdminButtonParams()
                {
                    Label = " Ajouter un extranet ",
                    ID = "ADD_EXTRANET",
                    Disabled = false,
                    OnClick = "nsExtranetAdmin.updateParam(this, 0);",
                    Attributes = new List<Tuple<string, string>>() { new Tuple<string, string>("dsc", "21|6"), new Tuple<string, string>("stid", _extension.Infos.ExtensionFileId.ToString()) }
                });




            buttonAdd.Generate(ExtensionParametersContainer);
            buttonAdd.SetFieldAttribute("style", "display:block");




            return;


            //           
        }
    }
}