using Com.Eudonet.Internal;
using EudoExtendedClasses;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;
using System.Net;
using static Com.Eudonet.Internal.eLibConst;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Renderer du module d'administration de l'add-in Outlook
    /// </summary>
    public class eAdminStorenOutlookAddinRenderer : eAdminStoreFileRenderer
    {

        eOutlookAddinSetting _outlookAddinSettings = new eOutlookAddinSetting();

        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        private eAdminStorenOutlookAddinRenderer(ePref pref, eAdminExtension extension)
            : base(pref, extension)
        {
        }



        /// <summary>
        /// Génération du renderer de paramètres de l'extension Add-in Outlook
        /// </summary>
        /// <param name="pref">Objet Pref</param>
        /// <param name="ext">Extension sur l'EudoStore (HotCom)</param>
        /// <returns>Le renderer</returns>
        public static eAdminStorenOutlookAddinRenderer CreateAdminStoreOutlookAddinRenderer(ePref pref, eAdminExtension ext)
        {
            if (pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                throw new EudoAdminInvalidRightException();

            eAdminStorenOutlookAddinRenderer rdr = new eAdminStorenOutlookAddinRenderer(pref, ext);
            rdr.Generate();
            return rdr;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            if (base.Init())
            {
                if (string.IsNullOrEmpty(_eRegisteredExt?.Param))
                    _outlookAddinSettings = eOutlookAddinSetting.GetEmptyMapping();
                else
                    _outlookAddinSettings = eOutlookAddinSetting.FromJsonString(_eRegisteredExt.Param);

                return true;
            }
            return false;
        }


        private eAdminExtensionOutlookAddin ExtensionOutlookAddin
        {
            get
            {
                return (eAdminExtensionOutlookAddin)Extension;
            }
        }

        /// <summary>
        /// Création du panneau de settings
        /// </summary>
        protected override void CreateSettingsPanel()
        {

            base.CreateSettingsPanel();

            List<Tuple<string, string, eAdminField>> lstInfos = new List<Tuple<string, string, eAdminField>>();

            string baseUrl = eLibTools.GetServerConfig("AddinOutlookBaseurl", eModelTools.GetBaseUrl()) + "addins/outlook/resources/manifest.xml";
            string sText = "";
            try
            {
                // Creates an HttpWebRequest for the specified URL. 
                HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(baseUrl);
                // Sends the HttpWebRequest and waits for a response.
                HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();
                if (myHttpWebResponse.StatusCode == HttpStatusCode.OK)
                    sText = baseUrl;

                myHttpWebResponse.Close();
            }
            catch
            {
                sText = eResApp.GetRes(_ePref, 2327);// "L'addin outlook n'est pas disponible sur ce serveur.";
            }



            Panel sectionTitleContainer = new Panel();
            sectionTitleContainer.Attributes.Add("class", "mandatoryParamWarning");
            HtmlGenericControl sectionTitle = new HtmlGenericControl();
            sectionTitle.Attributes.Add("class", "mandatoryParamTitle");
            sectionTitle.InnerText = eResApp.GetRes(_ePref, 2329);
            sectionTitleContainer.Controls.Add(sectionTitle);
            ExtensionParametersContainer.Controls.Add(sectionTitleContainer);


            lstInfos.Add(new Tuple<string, string, eAdminField>(eResApp.GetRes(_ePref, 2326), sText, null)); // Url Addin                                                                                         // lstInfos.Add(new Tuple<string, string>(eResApp.GetRes(_ePref, 2312), _eRegisteredExt?.ActivationKey ?? "")); //Clé d'activation
            lstInfos.Add(new Tuple<string, string, eAdminField>(eResApp.GetRes(_ePref, 2313), _eRegisteredExt?.DateEnabled.ToString("yyyy/MM/dd HH:mm") ?? "", null)); // date d'activation


            //Ajout section infos
            ExtensionParametersContainer.Controls.Add(GetInfosSection("addinInfos", eResApp.GetRes(_ePref, 7905), "", lstInfos));

            //Chargement des champs
            _outlookAddinSettings.LoadMapDesc(_ePref);





            if (_outlookAddinSettings.dicCommonFieldsWrite.Count() < 3 || _outlookAddinSettings.dicEmailTabs.Count() == 0 || _outlookAddinSettings.dicMailFields.Count() == 0)
            {
                Panel sectionParamInvalids = new Panel();
                sectionParamInvalids.Attributes.Add("class", "mandatoryParamWarning");
                sectionTitle = new HtmlGenericControl();
                sectionTitle.Attributes.Add("class", "mandatoryParamTitle");
                sectionTitle.InnerText = String.Concat(
                    "Le paramètrage de l'addin nécessite : ", Environment.NewLine,
                    " - Une table email >", Environment.NewLine,
                    " - Un champ 'courriel' accessible en lecture sur Contact ou Adresse", Environment.NewLine,
                    " - 3 champs en lecture sur Contact pour le mapping de Nom, Prénom et Particule"
                    );
                sectionParamInvalids.Controls.Add(sectionTitle);
                ExtensionParametersContainer.Controls.Add(sectionParamInvalids);

                return;
            }

            //Section choix table email
            List<ParamMappingDescId> lst = new List<ParamMappingDescId>();
            lst.Add(ParamMappingDescId.GetParamMapping("", (int)OUTLOOK_ADDIN_KEY.EMAIL_TAB, _outlookAddinSettings.dicEmailTabs, _outlookAddinSettings.GetKeyMapping(OUTLOOK_ADDIN_KEY.EMAIL_TAB).DescId, mandatory: true));
            ExtensionParametersContainer.Controls.Add(GetMappingSection("outlookAddinMain", eResApp.GetRes(Pref, 6408), lst)); // "Sélectionner le fichier de destination", "Fichier de destination" - TOCHECKRES


            //Section Mapping Champ
            lst = new List<ParamMappingDescId>
            {

                //Champ contact en écriture
                ParamMappingDescId.GetParamMapping(eResApp.GetRes(Pref, 5136), (int)OUTLOOK_ADDIN_KEY.NAME, _outlookAddinSettings.dicCommonFieldsWrite, _outlookAddinSettings.GetKeyMapping(OUTLOOK_ADDIN_KEY.NAME).DescId, mandatory:true),
                ParamMappingDescId.GetParamMapping(eResApp.GetRes(Pref, 5137), (int)OUTLOOK_ADDIN_KEY.FIRSTNAME, _outlookAddinSettings.dicCommonFieldsWrite, _outlookAddinSettings.GetKeyMapping(OUTLOOK_ADDIN_KEY.FIRSTNAME).DescId, mandatory:true),

                //Autre
                ParamMappingDescId.GetParamMapping(eResApp.GetRes(Pref, 1590), (int)OUTLOOK_ADDIN_KEY.PARTICLE, _outlookAddinSettings.dicCommonFieldsWrite, _outlookAddinSettings.GetKeyMapping(OUTLOOK_ADDIN_KEY.PARTICLE).DescId),
                ParamMappingDescId.GetParamMapping(eResApp.GetRes(Pref, 6166), (int)OUTLOOK_ADDIN_KEY.PHOTO , _outlookAddinSettings.dicPhotoFields, _outlookAddinSettings.GetKeyMapping(OUTLOOK_ADDIN_KEY.PHOTO).DescId, mandatory:true),

                // Champ information
                ParamMappingDescId.GetParamMapping(eResApp.GetRes(Pref, 1591), (int)OUTLOOK_ADDIN_KEY.CIVILITY, _outlookAddinSettings.dicCommonFields, _outlookAddinSettings.GetKeyMapping(OUTLOOK_ADDIN_KEY.CIVILITY).DescId),
                ParamMappingDescId.GetParamMapping(eResApp.GetRes(Pref, 607), (int)OUTLOOK_ADDIN_KEY.TITLE, _outlookAddinSettings.dicCommonFields, _outlookAddinSettings.GetKeyMapping(OUTLOOK_ADDIN_KEY.NAME).DescId),

                //Champ email - en écriture
                ParamMappingDescId.GetParamMapping(eResApp.GetRes(Pref, 5142), (int)OUTLOOK_ADDIN_KEY.EMAIL, _outlookAddinSettings.dicMailFields, _outlookAddinSettings.GetKeyMapping(OUTLOOK_ADDIN_KEY.EMAIL).DescId, mandatory:true),

                //Information addresse
                ParamMappingDescId.GetParamMapping(eResApp.GetRes(Pref, 1591), (int)OUTLOOK_ADDIN_KEY.COMPANY, _outlookAddinSettings.dicCommonFields, _outlookAddinSettings.GetKeyMapping(OUTLOOK_ADDIN_KEY.COMPANY).DescId),
                ParamMappingDescId.GetParamMapping(eResApp.GetRes(Pref, 1593), (int)OUTLOOK_ADDIN_KEY.STREET_1, _outlookAddinSettings.dicCommonFields, _outlookAddinSettings.GetKeyMapping(OUTLOOK_ADDIN_KEY.STREET_1).DescId),
                ParamMappingDescId.GetParamMapping(eResApp.GetRes(Pref, 1594), (int)OUTLOOK_ADDIN_KEY.STREET_2, _outlookAddinSettings.dicCommonFields, _outlookAddinSettings.GetKeyMapping(OUTLOOK_ADDIN_KEY.STREET_2).DescId),
                ParamMappingDescId.GetParamMapping(eResApp.GetRes(Pref, 7834), (int)OUTLOOK_ADDIN_KEY.STREET_3, _outlookAddinSettings.dicCommonFields, _outlookAddinSettings.GetKeyMapping(OUTLOOK_ADDIN_KEY.STREET_3).DescId),

                ParamMappingDescId.GetParamMapping(eResApp.GetRes(Pref, 6106), (int)OUTLOOK_ADDIN_KEY.POSTALCODE, _outlookAddinSettings.dicCommonFields, _outlookAddinSettings.GetKeyMapping(OUTLOOK_ADDIN_KEY.POSTALCODE).DescId),
                ParamMappingDescId.GetParamMapping(eResApp.GetRes(Pref, 1183), (int)OUTLOOK_ADDIN_KEY.CITY, _outlookAddinSettings.dicCommonFields, _outlookAddinSettings.GetKeyMapping(OUTLOOK_ADDIN_KEY.CITY).DescId),
                ParamMappingDescId.GetParamMapping(eResApp.GetRes(Pref, 5145), (int)OUTLOOK_ADDIN_KEY.COUNTRY, _outlookAddinSettings.dicCommonFields, _outlookAddinSettings.GetKeyMapping(OUTLOOK_ADDIN_KEY.COUNTRY).DescId),

                //Champ téléphone
                ParamMappingDescId.GetParamMapping(eResApp.GetRes(Pref, 657), (int)OUTLOOK_ADDIN_KEY.TEL_FIXED, _outlookAddinSettings.dicPhoneFields, _outlookAddinSettings.GetKeyMapping(OUTLOOK_ADDIN_KEY.TEL_FIXED).DescId),
                ParamMappingDescId.GetParamMapping(eResApp.GetRes(Pref, 7836), (int)OUTLOOK_ADDIN_KEY.TEL_MOBILE, _outlookAddinSettings.dicPhoneFields, _outlookAddinSettings.GetKeyMapping(OUTLOOK_ADDIN_KEY.TEL_MOBILE).DescId),
                ParamMappingDescId.GetParamMapping(eResApp.GetRes(Pref, 7837), (int)OUTLOOK_ADDIN_KEY.TEL_COMPANY, _outlookAddinSettings.dicPhoneFields, _outlookAddinSettings.GetKeyMapping(OUTLOOK_ADDIN_KEY.TEL_COMPANY).DescId)
            };

            ExtensionParametersContainer.Controls.Add(GetMappingSection("outlookAddinMapping", eResApp.GetRes(Pref, 7858), lst));

        }










        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sectionLabel"></param>
        /// <param name="lstMapp"></param>
        /// <returns></returns>
        private Panel GetMappingSection(string id, string sectionLabel, List<ParamMappingDescId> lstMapp)
        {
            //Création du bloc
            Panel sectionPanel = GetModuleSection(String.Concat("section_", id), sectionLabel);

            //Récupération de son conteneur principal ( soi
            Panel sectionPanelContainer = (Panel)FindControlRecursive(sectionPanel, "stepContent_section_" + id);

            //Création des lignes de mappings
            foreach (var a in lstMapp)
            {

                //Sélectionner une rubrique
                a.List.Insert(0, new ParamMappingDescId() { Label = String.Concat("<", eResApp.GetRes(Pref, 7862), ">"), Key = -1 });

                //Séparateur
                a.List.Insert(1, new ParamMappingDescId() { Label = "--------------------", Key = 0, Css = "BotStep", IsDisabled = true });

                sectionPanelContainer.Controls.Add(GetMappingDropDownList(a, id, "nsAdminAddin.changeMapping(this," + a.Key + ");"));
            }



            return sectionPanel;
        }







        /// <summary>
        /// Génération du contenu
        /// </summary>
        /// <returns>true si le contenu a été généré avec succès</returns>
        protected override bool Build()
        {
            // On indique au JavaScript (nsAdminMobile) que le module à recharger après modification est l'extension Add-in Outlook, et non l'extension Mobile
            AddCallBackScript("nsAdminMobile.currentExtensionModule = USROPT_MODULE_ADMIN_EXTENSIONS_OUTLOOKADDIN;");

            // Puis on appelle le builder parent
            return base.Build();
        }


    }
}