using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using System.Xml;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using Com.Eudonet.Core.Model;
using System.Web.UI;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// administration  de la vcard
    /// </summary>
    public class eAdminStoreVCardRenderer : eAdminStoreFileRenderer
    {
        private Dictionary<Int32, string> dicFields;
        private Dictionary<string, int> dicMappings;
        private bool eudoDropCreatePmAuthorized = false;


        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public eAdminStoreVCardRenderer(ePref pref, eAdminExtension extension)
            : base(pref, extension)
        {

        }
        /// <summary>
        /// statique qui crée l'écran
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="ext"></param>
        /// <returns></returns>
        public static eAdminStoreVCardRenderer CreateAdminStoreVCardRenderer(ePref pref, eAdminExtension ext)
        {
            if (pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                throw new EudoAdminInvalidRightException();

            eAdminStoreVCardRenderer rdr = new eAdminStoreVCardRenderer(pref, ext);

            rdr.Generate();
            return rdr;
        }

        /// <summary>
        /// initialisation
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            if (base.Init())
            {

                #region VCardMapping
                string sMapping = String.Empty;
                string sError;

                EudoQuery.eudoDAL dal = eLibTools.GetEudoDAL(Pref);
                try
                {
                    dal.OpenDatabase();

                    //chargement valeurs pour les dropdownlist
                    dicFields = eSqlVCardMapping.GetFieldsList(dal, out sError);
                    if (sError.Length > 0) { throw new Exception(sError); }

                    //chargement mapping
                    sMapping = eSqlVCardMapping.GetMapping(dal, out sError);
                    if (sError.Length > 0) { throw new Exception(sError); }
                }
                catch (Exception ex)
                {
                    return false;
                }
                finally
                {
                    dal.CloseDatabase();
                }

                // Parse du mapping      
                try
                {
                    dicMappings = new Dictionary<string, int>();
                    XmlDocument xmlVCard = new XmlDocument();
                    xmlVCard.LoadXml(sMapping);
                    foreach (XmlNode xField in xmlVCard.SelectSingleNode("//VCardMapping"))
                    {
                        Int32 nFieldDescId = 0;
                        if (!dicMappings.ContainsKey(xField.Name) && Int32.TryParse(xField.InnerText, out nFieldDescId) && nFieldDescId > 0)
                            dicMappings.Add(xField.Name, nFieldDescId);
                    }
                }
                catch (XmlException ex)
                {
                    //xml parse error
                }
                catch (Exception ex)
                {
                    return false;
                }
                #endregion

                #region Autorisation EudoDrop
                Dictionary<eLibConst.CONFIG_DEFAULT, String> config = Pref.GetConfigDefault(new eLibConst.CONFIG_DEFAULT[] { eLibConst.CONFIG_DEFAULT.EDCREATEPMAUTHORIZED });
                if (config[eLibConst.CONFIG_DEFAULT.EDCREATEPMAUTHORIZED] == "1")
                    eudoDropCreatePmAuthorized = true;
                #endregion

                return true;
            }
            return false;
        }

        /// <summary>
        /// écran de paramétrage
        /// </summary>
        protected override void CreateSettingsPanel()
        {
            base.CreateSettingsPanel();
            Panel targetPanel = null;

            #region VCardMapping
            Panel section = GetModuleSection(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_VCARD.ToString(), eResApp.GetRes(Pref, 7830));

            if (section.Controls.Count > 0 && section.Controls[section.Controls.Count - 1] is Panel)
                targetPanel = (Panel)section.Controls[section.Controls.Count - 1];
            if (targetPanel == null)
            {
                ExtensionParametersContainer.Controls.Add(new LiteralControl("Une erreur est survenue durant l'écran de paramétrage"));
                return;
            }
            ExtensionParametersContainer.Controls.Add(section);

            string customDropdownCSSClasses = "optionField";
            string customPanelCSSClasses = "fieldInline";
            string customLabelCSSClasses = "labelField optionField";

            foreach (eLibConst.VCARD_MAPPING_TYPE type in Enum.GetValues(typeof(eLibConst.VCARD_MAPPING_TYPE)))
            {
                /* La construction de la liste est faites dans le foreach, sinon il y a des problèmes de références aux objets entre les différentes dropdownlist */
                List<ListItem> listeItems = new List<ListItem>();
                listeItems.Add(new ListItem(eResApp.GetRes(Pref, 238), "0"));
                foreach (KeyValuePair<int, string> kvp in dicFields.OrderBy(kvp => kvp.Value))
                {
                    listeItems.Add(new ListItem(kvp.Value, kvp.Key.ToString()));
                }

                string typeMappingName = eLibTools.GetVCARDMappingNodeName(type);
                string typeMappingLabel = eLibTools.GetVCARDMappingLabel(type, Pref);

                string id = String.Concat("ddl", typeMappingName);
                string label = String.Concat(typeMappingLabel, " : ");

                string selectedValue = "0";
                if (dicMappings.ContainsKey(typeMappingName) && dicFields.ContainsKey(dicMappings[typeMappingName]))
                    selectedValue = dicMappings[typeMappingName].ToString();

                AddDropdownOptionField(targetPanel, id, label, "",
                    0, 0, typeof(eLibConst.CONFIG_DEFAULT), listeItems, selectedValue, EudoQuery.FieldFormat.TYP_NUMERIC,
                    eAdminDropdownField.eAdminDropdownFieldRenderType.INLINE, customDropdownCSSClasses: customDropdownCSSClasses, customPanelCSSClasses: customPanelCSSClasses, customLabelCSSClasses: customLabelCSSClasses,
                    onChange: String.Concat("nsAdmin.updateVCardMappings(this, '", typeMappingName, "');"), sortItemsByLabel: false);
            }

            #endregion

            #region Autorisation EudoDrop
            section = GetModuleSection(String.Concat(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_VCARD.ToString(), "_EudoDrop"), String.Concat(eResApp.GetRes(Pref, 444), " ", eResApp.GetRes(Pref, 7869)));

            if (section.Controls.Count > 0 && section.Controls[section.Controls.Count - 1] is Panel)
                targetPanel = (Panel)section.Controls[section.Controls.Count - 1];
            if (targetPanel == null)
            {
                ExtensionParametersContainer.Controls.Add(new LiteralControl("Une erreur est survenue durant l'écran de paramétrage"));
                return;
            }
            ExtensionParametersContainer.Controls.Add(section);


            Dictionary<string, string> dicoRadioButton = new Dictionary<string, string>();
            dicoRadioButton.Add("1", eResApp.GetRes(Pref, 58));
            dicoRadioButton.Add("0", eResApp.GetRes(Pref, 59));
            string rbSelectedValue = eudoDropCreatePmAuthorized ? "1" : "0";

            AddRadioButtonOptionField(targetPanel, "rbEDCreatePmAuthorized", "rbEDCreatePmAuthorized", eResApp.GetRes(Pref, 1739), "",
                eAdminUpdateProperty.CATEGORY.CONFIGDEFAULT, (int)eLibConst.CONFIG_DEFAULT.EDCREATEPMAUTHORIZED, typeof(eLibConst.CONFIG_DEFAULT),
                dicoRadioButton, rbSelectedValue, EudoQuery.FieldFormat.TYP_BIT);
            #endregion

        }
    }
}