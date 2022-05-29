using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using static Com.Eudonet.Internal.eLibConst;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Renderer du module d'administration de linkedin
    /// </summary>
    public class eAdminStoreLinkedinRenderer : eAdminStoreFileRenderer
    {
        eLinkedinSetting _linkedinSetting = new eLinkedinSetting();

        /// <summary>
        /// constructeur par défaut
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="extension"></param>
        private eAdminStoreLinkedinRenderer(ePref pref, eAdminExtension extension)
            : base(pref, extension)
        {

        }

        /// <summary>
        /// Génération du renderer de paramètres de l'extension linkedin
        /// </summary>
        /// <param name="pref">Objet Pref</param>
        /// <param name="ext">Extension sur l'EudoStore (HotCom)</param>
        /// <returns>Le renderer</returns>
        public static eAdminStoreLinkedinRenderer CreateeAdminStoreLinkedinRenderer(ePref pref, eAdminExtension ext)
        {
            if (pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                throw new EudoAdminInvalidRightException();

            eAdminStoreLinkedinRenderer rdr = new eAdminStoreLinkedinRenderer(pref, ext);
            rdr.Generate();
            return rdr;
        }

        /// <summary>
        /// initialisation d'extension
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            if (base.Init())
            {
                if (string.IsNullOrEmpty(_eRegisteredExt?.Param))
                    _linkedinSetting = eLinkedinSetting.GetEmptyMapping();
                else
                    _linkedinSetting = eLinkedinSetting.FromJsonString(_eRegisteredExt.Param);

                return true;
            }
            return false;
        }

        /// <summary>
        /// Création du panneau de settings
        /// </summary>
        protected override void CreateSettingsPanel()
        {
            base.CreateSettingsPanel();

            //Chargement des champs
            _linkedinSetting.LoadMapDesc(_ePref);


            if (_linkedinSetting.dicCommonFieldsWrite.Count() < 3 || !_linkedinSetting.dicEmailTabs.Any() || !_linkedinSetting.dicMailFields.Any())
            {
                Panel sectionParamInvalids = new Panel();
                sectionParamInvalids.Attributes.Add("class", "mandatoryParamWarning");
                HtmlGenericControl sectionTitle = new HtmlGenericControl();
                sectionTitle.Attributes.Add("class", "mandatoryParamTitle");
                sectionTitle.InnerText = String.Concat(
                    "Le paramètrage de linkedin nécessite : ", Environment.NewLine,
                    " - Une table email >", Environment.NewLine,
                    " - Un champ 'courriel' accessible en lecture sur Contact ou Adresse", Environment.NewLine,
                    " - 3 champs en lecture sur Contact pour le mapping de Nom, Prénom et Linkedin Profil"
                    );
                sectionParamInvalids.Controls.Add(sectionTitle);
                ExtensionParametersContainer.Controls.Add(sectionParamInvalids);

                return;
            }

            //Section Mapping Champ
            List<ParamMappingDescId> lst = new List<ParamMappingDescId>
            {
                //Champ contact en écriture(Name,FirstName)
                ParamMappingDescId.GetParamMapping(eResApp.GetRes(Pref, 5136), (int)LINKEDIN_KEY.NAME, _linkedinSetting.dicCommonFieldsWrite  , _linkedinSetting.GetKeyMapping(LINKEDIN_KEY.NAME).DescId,mandatory:true),
                ParamMappingDescId.GetParamMapping(eResApp.GetRes(Pref, 5137), (int)LINKEDIN_KEY.FIRSTNAME, _linkedinSetting.dicCommonFieldsWrite, _linkedinSetting.GetKeyMapping(LINKEDIN_KEY.FIRSTNAME).DescId,mandatory:true),

                //compagny
                ParamMappingDescId.GetParamMapping(eResApp.GetRes(Pref, 5129), (int)LINKEDIN_KEY.COMPANY, _linkedinSetting.dicCommonFields, _linkedinSetting.GetKeyMapping(LINKEDIN_KEY.COMPANY).DescId),
          
                // Champ information
                ParamMappingDescId.GetParamMapping(eResApp.GetRes(Pref, 607), (int)LINKEDIN_KEY.JOB_TITLE, _linkedinSetting.dicCommonFields, _linkedinSetting.GetKeyMapping(LINKEDIN_KEY.JOB_TITLE).DescId),
                 
                //Autre Photo
                ParamMappingDescId.GetParamMapping(eResApp.GetRes(Pref, 6166), (int)LINKEDIN_KEY.PHOTO , _linkedinSetting.dicPhotoFields, _linkedinSetting.GetKeyMapping(LINKEDIN_KEY.PHOTO).DescId,mandatory:true),

                //Email
                ParamMappingDescId.GetParamMapping(eResApp.GetRes(Pref, 5142), (int)LINKEDIN_KEY.EMAIL , _linkedinSetting.dicMailFields, _linkedinSetting.GetKeyMapping(LINKEDIN_KEY.EMAIL).DescId),

                //Champ téléphone
                ParamMappingDescId.GetParamMapping(eResApp.GetRes(Pref, 7836), (int)LINKEDIN_KEY.TEL_MOBILE, _linkedinSetting.dicCommonFields, _linkedinSetting.GetKeyMapping(LINKEDIN_KEY.TEL_MOBILE).DescId),

                //Réseau sociaux
                ParamMappingDescId.GetParamMapping(eResApp.GetRes(Pref, 6170), (int)LINKEDIN_KEY.LINKEDIN_ID, _linkedinSetting.dicCommonFields, _linkedinSetting.GetKeyMapping(LINKEDIN_KEY.LINKEDIN_ID).DescId,mandatory:true),
                ParamMappingDescId.GetParamMapping(eResApp.GetRes(Pref, 3047), (int)LINKEDIN_KEY.TWITTER_ID, _linkedinSetting.dicCommonFields, _linkedinSetting.GetKeyMapping(LINKEDIN_KEY.TWITTER_ID).DescId),
               
                //adresse
                ParamMappingDescId.GetParamMapping(eResApp.GetRes(Pref, 1593), (int)LINKEDIN_KEY.STREET_1, _linkedinSetting.dicCommonFields, _linkedinSetting.GetKeyMapping(LINKEDIN_KEY.STREET_1).DescId),
                ParamMappingDescId.GetParamMapping(eResApp.GetRes(Pref, 6106), (int)LINKEDIN_KEY.POSTALCODE, _linkedinSetting.dicCommonFields, _linkedinSetting.GetKeyMapping(LINKEDIN_KEY.POSTALCODE).DescId),
                ParamMappingDescId.GetParamMapping(eResApp.GetRes(Pref, 1183), (int)LINKEDIN_KEY.CITY, _linkedinSetting.dicCommonFields, _linkedinSetting.GetKeyMapping(LINKEDIN_KEY.CITY).DescId),
                ParamMappingDescId.GetParamMapping(eResApp.GetRes(Pref, 5145), (int)LINKEDIN_KEY.COUNTRY, _linkedinSetting.dicCommonFields, _linkedinSetting.GetKeyMapping(LINKEDIN_KEY.COUNTRY).DescId),

                // custom Field
                 ParamMappingDescId.GetParamMapping(String.Concat( eResApp.GetRes(Pref, 7102)," ",1), (int)LINKEDIN_KEY.CustomField, _linkedinSetting.dicCommonFields, _linkedinSetting.GetKeyMapping(LINKEDIN_KEY.CustomField).DescId),
                 ParamMappingDescId.GetParamMapping(String.Concat( eResApp.GetRes(Pref, 7102)," ",2), (int)LINKEDIN_KEY.CustomField1, _linkedinSetting.dicCommonFields, _linkedinSetting.GetKeyMapping(LINKEDIN_KEY.CustomField1).DescId),
                 ParamMappingDescId.GetParamMapping(String.Concat( eResApp.GetRes(Pref, 7102)," ",3), (int)LINKEDIN_KEY.CustomField2, _linkedinSetting.dicCommonFields, _linkedinSetting.GetKeyMapping(LINKEDIN_KEY.CustomField2).DescId),
                 ParamMappingDescId.GetParamMapping(String.Concat( eResApp.GetRes(Pref, 7102)," ",4), (int)LINKEDIN_KEY.CustomField3, _linkedinSetting.dicCommonFields, _linkedinSetting.GetKeyMapping(LINKEDIN_KEY.CustomField3).DescId),
                 ParamMappingDescId.GetParamMapping(String.Concat( eResApp.GetRes(Pref, 7102)," ",5), (int)LINKEDIN_KEY.CustomField4, _linkedinSetting.dicCommonFields, _linkedinSetting.GetKeyMapping(LINKEDIN_KEY.CustomField4).DescId),
            };

            ExtensionParametersContainer.Controls.Add(GetMappingSection("linkedinMapping", eResApp.GetRes(Pref, 7858), lst));


        }
        Panel sectionPanel;

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
            sectionPanel = GetModuleSection(String.Concat("section_", id), sectionLabel);

            //Récupération de son conteneur principal ( soi
            Panel sectionPanelContainer = (Panel)FindControlRecursive(sectionPanel, "stepContent_section_" + id);

            //Création des lignes de mappings
            foreach (var a in lstMapp)
            {

                //Sélectionner une rubrique
                a.List.Insert(0, new ParamMappingDescId() { Label = String.Concat("<", eResApp.GetRes(Pref, 7862), ">"), Key = -1 });

                //Séparateur
                a.List.Insert(1, new ParamMappingDescId() { Label = "--------------------", Key = 0, Css = "BotStep", IsDisabled = true });

                sectionPanelContainer.Controls.Add(GetMappingDropDownList(a, id, "nsAdminLinkedin.changeMapping(this," + a.Key + ");"));
            }

            return sectionPanel;
        }


    }
}