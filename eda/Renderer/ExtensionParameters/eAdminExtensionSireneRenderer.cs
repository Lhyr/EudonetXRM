using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Renderer du module d'administration du référentiel Sirene
    /// </summary>
    public class eAdminExtensionSireneRenderer : eAdminExtensionFileRenderer
    {
        Dictionary<int, List<eSireneMapping>> _existingMappings;
        List<SireneResultMetaDataField> _sireneFields;
        List<SireneResultMetaDataCategory> _sireneCategories;
        string _mappingInformation = String.Empty;
        string _availableFieldsInformation = String.Empty;
        
        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public eAdminExtensionSireneRenderer(ePref pref, eAdminExtension extension, string initialTab)
            : base(pref, extension, initialTab)
        {

        }

        /// <summary>
        /// Création du rendu mode Fiche de l'extension Sirene
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="extensionFileIdFromStore"></param>
        /// <param name="bNoInternet"></param>
        /// <param name="initialTab"></param>
        /// <returns></returns>
        public static eAdminExtensionSireneRenderer CreateAdminExtensionSireneRenderer(ePref pref, int extensionFileIdFromStore, bool bNoInternet, string initialTab)
        {
            if (pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                throw new EudoAdminInvalidRightException();

            eAdminExtension ext = bNoInternet ? eAdminExtension.initExtensionFromJson(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SIRENE, pref) :
                eAdminExtension.InitFromModule(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SIRENE, pref, extensionFileIdFromStore);

            return new eAdminExtensionSireneRenderer(pref, ext, initialTab);
        }

        /// <summary>
        /// Initialisation des params
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            string error = String.Empty;

            if (base.Init())
            {
                try
                {
                    _existingMappings = eSireneMapping.GetMappings(Pref, out error);
                    if (error.Length > 0)
                        _mappingInformation = error;
                    else
                        _mappingInformation = eResApp.GetRes(Pref, 8562).Replace("<TABCOUNT>", _existingMappings.Count.ToString()); // X onglets bénéficient actuellement du référentiel Sirene. Pour paramétrer les champs utilisés, rendez-vous dans l'administration de chaque onglet.

                    error = eSireneMapping.GetSireneFieldsAndCategories(Pref, out _sireneFields, out _sireneCategories);
                    if (error.Length > 0)
                        _availableFieldsInformation = error;
                    else
                        _availableFieldsInformation = eResApp.GetRes(Pref, 8563).Replace("<FIELDCOUNT>", _sireneFields.Count.ToString()).Replace("<CATCOUNT>", _sireneCategories.Count.ToString());
                }
                catch (Exception ex)
                {
                    error = String.Concat(Environment.NewLine, ex.Message);
                }
                finally
                {
                }
            }
            else
                throw new Exception(error);

            return true;
        }

        /// <summary>
        /// Génération du contenu
        /// </summary>
        /// <returns>true si le contenu a été généré avec succès</returns>
        protected override bool Build()
        {
            if (base.Build())
            {
                Panel targetPanel = null;

                Panel section = GetModuleSection(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SIRENE.ToString(), eResApp.GetRes(Pref, 7905)); // Informations
                ExtensionParametersContainer.Controls.Add(section);

                if (section.Controls.Count > 0 && section.Controls[section.Controls.Count - 1] is Panel)
                    targetPanel = (Panel)section.Controls[section.Controls.Count - 1];
                if (targetPanel == null)
                    return false;

                HtmlGenericControl mappingInformationLabel = new HtmlGenericControl("p");
                mappingInformationLabel.InnerHtml = _mappingInformation;

                HtmlGenericControl availableFieldsInformationLabel = new HtmlGenericControl("p");
                availableFieldsInformationLabel.InnerHtml = _availableFieldsInformation;

                targetPanel.Controls.Add(mappingInformationLabel);
                targetPanel.Controls.Add(availableFieldsInformationLabel);

                return true;
            }
            else
                return false;
        }
    }
}