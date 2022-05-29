using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using System.Xml;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminExtensionVCardRenderer : eAdminExtensionFileRenderer
    {
        private Dictionary<Int32, string> dicFields;
        private Dictionary<string, int> dicMappings;
        private bool eudoDropCreatePmAuthorized = false;

        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public eAdminExtensionVCardRenderer(ePref pref, eAdminExtension extension, string initialTab)
            : base(pref, extension, initialTab)
        {

        }

        public static eAdminExtensionVCardRenderer CreateAdminExtensionVCardRenderer(ePref pref, int extensionFileIdFromStore,bool bNoInternet, string initialTab)
        {
            if (pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                throw new EudoAdminInvalidRightException();

            eAdminExtension ext = bNoInternet ? eAdminExtension.initExtensionFromJson(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_VCARD, pref) :
              eAdminExtension.InitFromModule(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_VCARD, pref, extensionFileIdFromStore);

            return new eAdminExtensionVCardRenderer(pref, ext, initialTab);
        }

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

        protected override bool Build()
        {
            if (base.Build())
            {
                Panel targetPanel = null;

                #region VCardMapping
                Panel section = GetModuleSection(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_VCARD.ToString(), eResApp.GetRes(Pref, 7830));
                ExtensionParametersContainer.Controls.Add(section);

                if (section.Controls.Count > 0 && section.Controls[section.Controls.Count - 1] is Panel)
                    targetPanel = (Panel)section.Controls[section.Controls.Count - 1];
                if (targetPanel == null)
                    return false;

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
                ExtensionParametersContainer.Controls.Add(section);

                if (section.Controls.Count > 0 && section.Controls[section.Controls.Count - 1] is Panel)
                    targetPanel = (Panel)section.Controls[section.Controls.Count - 1];
                if (targetPanel == null)
                    return false;


                Dictionary<string, string> dicoRadioButton = new Dictionary<string, string>();
                dicoRadioButton.Add("1", eResApp.GetRes(Pref, 58));
                dicoRadioButton.Add("0", eResApp.GetRes(Pref, 59));
                string rbSelectedValue = eudoDropCreatePmAuthorized ? "1" : "0";

                AddRadioButtonOptionField(targetPanel, "rbEDCreatePmAuthorized", "rbEDCreatePmAuthorized", eResApp.GetRes(Pref, 1739), "",
                    eAdminUpdateProperty.CATEGORY.CONFIGDEFAULT, (int)eLibConst.CONFIG_DEFAULT.EDCREATEPMAUTHORIZED, typeof(eLibConst.CONFIG_DEFAULT),
                    dicoRadioButton, rbSelectedValue, EudoQuery.FieldFormat.TYP_BIT);
                #endregion

                return true;
            }
            return false;
        }
    }
}