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
    public class eAdminStoreSireneRenderer : eAdminStoreFileRenderer
    {
        Dictionary<int, List<eSireneMapping>> _existingMappings;
        List<SireneResultMetaDataField> _sireneFields;
        List<SireneResultMetaDataCategory> _sireneCategories;
        string _mappingInformation = String.Empty;
        string _availableFieldsInformation = String.Empty;



        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="extension"></param>
        public eAdminStoreSireneRenderer(ePref pref, eAdminExtension extension)
            : base(pref, extension)
        {

        }

        /// <summary>
        /// Création du rendu mode Fiche de l'extension Sirene
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="ext"></param>
        /// <returns></returns>
        public static eAdminStoreSireneRenderer CreateAdminStoreSireneRenderer(ePref pref, eAdminExtension ext)
        {
            if (pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                throw new EudoAdminInvalidRightException();

            eAdminStoreSireneRenderer rdr = new eAdminStoreSireneRenderer(pref, ext);

            rdr.Generate();
            return rdr;
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
        /// écran de paramétrage
        /// </summary>
        protected override void CreateSettingsPanel()
        {
            base.CreateSettingsPanel();
            Panel targetPanel = null;

            Panel section = GetModuleSection(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SIRENE.ToString(), eResApp.GetRes(Pref, 7905)); // Informations

            if (section.Controls.Count > 0 && section.Controls[section.Controls.Count - 1] is Panel)
                targetPanel = (Panel)section.Controls[section.Controls.Count - 1];
            if (targetPanel == null)
            {
                ExtensionParametersContainer.Controls.Add(new LiteralControl("Une erreur est survenue durant l'écran de paramétrage"));
                return;
            }
            ExtensionParametersContainer.Controls.Add(section);

            HtmlGenericControl mappingInformationLabel = new HtmlGenericControl("p");
            mappingInformationLabel.InnerHtml = _mappingInformation;

            HtmlGenericControl availableFieldsInformationLabel = new HtmlGenericControl("p");
            availableFieldsInformationLabel.InnerHtml = _availableFieldsInformation;

            targetPanel.Controls.Add(mappingInformationLabel);
            targetPanel.Controls.Add(availableFieldsInformationLabel);

        }
    }
}