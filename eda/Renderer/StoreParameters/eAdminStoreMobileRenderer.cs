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

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Renderer du module d'administration des versions Mobile (iOS / Android)
    /// </summary>
    public class eAdminStoreMobileRenderer : eAdminStoreFileRenderer
    {
        Dictionary<Int32, String> _tabListPlanning;
        Dictionary<Int32, String> _tabListMain;
        Dictionary<Int32, String> _tabListStandard;
        Dictionary<Int32, String> _tabListOutlookAddinMainMail;
        Dictionary<Int32, String> _tabListOutlookAddinMainPlanning;

        Dictionary<int, int> _tabListStandardInterEventDescIds;
        Dictionary<eLibConst.MOBILE, eMobileMapping> _mobileMapping;

        List<eFieldLiteAdmin> _fieldListMain;
        IEnumerable<eFieldLiteAdmin> _fieldListMainPP;
        IEnumerable<eFieldLiteAdmin> _fieldListMainPM;
        IEnumerable<eFieldLiteAdmin> _fieldListMainADR;
        List<eFieldLiteAdmin> _fieldListPlanning;
        List<eFieldLiteAdmin> _fieldListEvent;
        List<eFieldLiteAdmin> _fieldListStandard;
        List<eFieldLiteAdmin> _fieldListOutlookAddinMain;

        eLibConst.MOBILEMAPPINGTYPE _tp;

        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public eAdminStoreMobileRenderer(ePref pref, eAdminExtension extension)
            : base(pref, extension)
        {
            _tp = extension is eAdminExtensionMobile ? eLibConst.MOBILEMAPPINGTYPE.MOBILE : eLibConst.MOBILEMAPPINGTYPE.ADDINOUTLOOK;
        }

        /// <summary>
        /// Colonne de tri
        /// </summary>
        public string SortCol { get; internal set; }

        /// <summary>
        /// Tri
        /// </summary>
        public int Sort { get; internal set; }


        /// <summary>
        /// Génération du rendu
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="ext"></param>
        /// <returns></returns>
        public static eAdminStoreMobileRenderer CreateAdminStoreMobileRenderer(ePref pref, eAdminExtension ext)
        {
            if (pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                throw new EudoAdminInvalidRightException();

            eAdminStoreMobileRenderer rdr = new eAdminStoreMobileRenderer(pref, ext);
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
                    Extension.DAL.OpenDatabase();

                    Dictionary<Int32, String> deletedOrVirtualTabListPlanning = new Dictionary<int, string>();
                    Dictionary<Int32, String> deletedOrVirtualTabListMain = new Dictionary<int, string>();
                    Dictionary<Int32, String> deletedOrVirtualTabListStandard = new Dictionary<int, string>();
                    Dictionary<Int32, String> deletedOrVirtualTabListOutlookAddinMain = new Dictionary<int, string>();
                    Dictionary<int, int> standardTabEventNums = new Dictionary<int, int>();

                    _mobileMapping = eMobileMapping.LoadMapping(Pref, out error);

                    foreach (eMobileMapping v in _mobileMapping.Values)
                        v.Tp = _tp;

                    #region Récupération des tables source
                    try
                    {
                        _tabListPlanning = eDataTools.GetFiles(Extension.DAL, Pref, new List<EdnType>() { EdnType.FILE_PLANNING },
                            out error, ref deletedOrVirtualTabListPlanning, SortCol, Sort, true);
                        _tabListMain = eDataTools.GetFiles(Extension.DAL, Pref, new List<EdnType>() { EdnType.FILE_MAIN },
                            out error, ref deletedOrVirtualTabListMain, SortCol, Sort, true);
                        _tabListStandard = eDataTools.GetFiles(Extension.DAL, Pref, new List<EdnType>() { EdnType.FILE_STANDARD },
                            out error, ref deletedOrVirtualTabListStandard, SortCol, Sort, true);

                        _tabListOutlookAddinMainMail = eDataTools.GetFiles(Extension.DAL, Pref, new List<EdnType>() { EdnType.FILE_MAIL },
                            out error, ref deletedOrVirtualTabListOutlookAddinMain, SortCol, Sort, true);


                        _tabListOutlookAddinMainPlanning = eDataTools.GetFiles(Extension.DAL, Pref, new List<EdnType>() { EdnType.FILE_PLANNING },
                            out error, ref deletedOrVirtualTabListOutlookAddinMain, SortCol, Sort, true);

                        // Filtrage sur les tables Invitation : uniquement les tables liées aux tables principales proposées
                        // On dresse un tableau des liens entre les tables Standard et leurs tables Event liées
                        // Puis on ajoutera ce tableau en JS sur la page pour permettre de filtrer les tables et les champs proposés
                        // dans la section "Standard" (Invitations) en JS
                        _tabListStandardInterEventDescIds = new Dictionary<int, int>();
                        HashSet<int> tabsWithoutInterEvent = new HashSet<int>();
                        foreach (int tab in _tabListStandard.Keys)
                        {
                            TableLite standardTab = new TableLite(tab);
                            if (standardTab.ExternalLoadInfo(Extension.DAL, out error))
                            {
                                if (standardTab.InterEVTDescid > 0)
                                    _tabListStandardInterEventDescIds[tab] = standardTab.InterEVTDescid;
                                else
                                    tabsWithoutInterEvent.Add(tab);
                            }
                        }
                        foreach (int tab in tabsWithoutInterEvent)
                            _tabListStandard.Remove(tab);

                        // Filtrage sur toutes les collections
                        List<int> filteredTabs = new List<int>() {
                            (int)TableType.PP,
                            (int)TableType.PM,
                            (int)TableType.ADR,
                            (int)TableType.DOUBLONS,
                            (int)TableType.FILEDATA,
                            (int)TableType.HISTO,
                            (int)TableType.USER,
                            (int)TableType.PJ,
                            (int)TableType.GROUP,
                            (int)TableType.FILTER,
                            (int)TableType.REPORT,
                            (int)TableType.NOTIFICATION,
                            (int)TableType.NOTIFICATION_TRIGGER,
                            (int)TableType.NOTIFICATION_UNSUBSCRIBER,
                            (int)TableType.BOUNCEMAIL,
                            (int)TableType.CAMPAIGN,
                            (int)TableType.CAMPAIGNSTATS,
                            (int)TableType.CAMPAIGNSTATSADV,
                            (int)TableType.UNSUBSCRIBEMAIL,
                            (int)TableType.TRACKLINK,
                            (int)TableType.TRACKLINKLOG,
                            (int)TableType.FORMULARXRM
                        };
                        foreach (int tab in filteredTabs)
                        {
                            _tabListPlanning.Remove(tab);
                            _tabListMain.Remove(tab);
                            _tabListStandard.Remove(tab);
                            _tabListOutlookAddinMainMail.Remove(tab);
                            _tabListOutlookAddinMainPlanning.Remove(tab);
                        }
                    }
                    catch (Exception ex)
                    {
                        error = String.Concat(Environment.NewLine, ex.Message);
                    }
                    #endregion

                    try
                    {
                        _fieldListMainPP = GetTabFields((int)TableType.PP);
                        _fieldListMainPM = GetTabFields((int)TableType.PM);
                        _fieldListMainADR = GetTabFields((int)TableType.ADR);

                        _fieldListMain = new List<eFieldLiteAdmin>();
                        _fieldListMain.AddRange(_fieldListMainPP);
                        _fieldListMain.AddRange(_fieldListMainPM);
                        _fieldListMain.AddRange(_fieldListMainADR);

                        // TOCHECK FONCTIONNEL : AJOUT DES TABLES PP/PM/ADR POUR L'ADD-IN OUTLOOK
                        _fieldListOutlookAddinMain = new List<eFieldLiteAdmin>();
                        _fieldListOutlookAddinMain.AddRange(_fieldListMainPP);
                        _fieldListOutlookAddinMain.AddRange(_fieldListMainPM);
                        _fieldListOutlookAddinMain.AddRange(_fieldListMainADR);
                    }
                    catch (Exception ex)
                    {
                        error = String.Concat(Environment.NewLine, ex.Message);
                    }

                    try
                    {
                        eMobileMapping currentMobileMapping = null;
                        _fieldListPlanning = new List<eFieldLiteAdmin>();
                        if (_mobileMapping.TryGetValue(eLibConst.MOBILE.CALENDAR_ACTION, out currentMobileMapping))
                        {
                            /*
                            if (currentMobileMapping.Tab != null && currentMobileMapping.Tab.Count > 0)
                            {
                                foreach (int key in currentMobileMapping.Tab)
                                    if (!deletedOrVirtualTabListPlanning.ContainsKey(key))
                                        _fieldListPlanning.AddRange(GetTabFields(key));
                            }
                            */
                            int currentFieldTab = currentMobileMapping.DescId - currentMobileMapping.DescId % 100;
                            if (!deletedOrVirtualTabListPlanning.ContainsKey(currentFieldTab))
                                _fieldListPlanning.AddRange(GetTabFields(currentFieldTab));
                        }
                        foreach (int key in _tabListPlanning.Keys)
                        {
                            if (!deletedOrVirtualTabListPlanning.ContainsKey(key))
                                _fieldListPlanning.AddRange(GetTabFields(key));
                        }
                    }
                    catch (Exception ex)
                    {
                        error = String.Concat(Environment.NewLine, ex.Message);
                    }

                    try
                    {
                        eMobileMapping currentMobileMapping = null;
                        _fieldListEvent = new List<eFieldLiteAdmin>();
                        if (_mobileMapping.TryGetValue(eLibConst.MOBILE.EVENT_DATE, out currentMobileMapping))
                        {
                            /*
                            if (currentMobileMapping.Tab != null && currentMobileMapping.Tab.Count > 0)
                            {
                                foreach (int key in currentMobileMapping.Tab)
                                    if (!deletedOrVirtualTabListMain.ContainsKey(key))
                                        _fieldListEvent.AddRange(GetTabFields(key));
                            }
                            */
                            int currentFieldTab = currentMobileMapping.DescId - currentMobileMapping.DescId % 100;
                            if (!deletedOrVirtualTabListMain.ContainsKey(currentFieldTab))
                                _fieldListEvent.AddRange(GetTabFields(currentFieldTab));
                        }
                        foreach (int key in _tabListMain.Keys)
                        {
                            if (!deletedOrVirtualTabListMain.ContainsKey(key))
                                _fieldListEvent.AddRange(GetTabFields(key));
                        }
                    }
                    catch (Exception ex)
                    {
                        error = String.Concat(Environment.NewLine, ex.Message);
                    }

                    try
                    {
                        eMobileMapping currentMobileMapping = null;
                        _fieldListStandard = new List<eFieldLiteAdmin>();
                        if (_mobileMapping.TryGetValue(eLibConst.MOBILE.EVENT_INVITATION_CONFIRMATION, out currentMobileMapping))
                        {
                            /*
                            if (currentMobileMapping.Tab != null && currentMobileMapping.Tab.Count > 0)
                            {
                                foreach (int key in currentMobileMapping.Tab)
                                    if (!deletedOrVirtualTabListStandard.ContainsKey(key))
                                        _fieldListStandard.AddRange(GetTabFields(key));
                            }
                            */
                            int currentFieldTab = currentMobileMapping.DescId - currentMobileMapping.DescId % 100;
                            if (!deletedOrVirtualTabListStandard.ContainsKey(currentFieldTab))
                                _fieldListStandard.AddRange(GetTabFields(currentFieldTab));
                        }
                        foreach (int key in _tabListStandard.Keys)
                            if (!deletedOrVirtualTabListStandard.ContainsKey(key))
                                _fieldListStandard.AddRange(GetTabFields(key));
                    }
                    catch (Exception ex)
                    {
                        error = String.Concat(Environment.NewLine, ex.Message);
                    }
                }
                catch (Exception ex)
                {
                    error = String.Concat(Environment.NewLine, ex.Message);
                }
                finally
                {
                    Extension.DAL.CloseDatabase();
                }
            }
            else
                throw new Exception(error);

            return true;
        }

        /// <summary>
        /// Ajoute le panel de rendu des paramètres
        /// </summary>
        /// <returns></returns>
        protected override void CreateSettingsPanel()
        {

            base.CreateSettingsPanel();

            #region Définition des champs à afficher
            List<eLibConst.MOBILE> mainMobileFieldsList = new List<eLibConst.MOBILE>() {
                eLibConst.MOBILE.NAME,
                eLibConst.MOBILE.FIRSTNAME, // iso v7
                eLibConst.MOBILE.PARTICLE, // iso v7
                eLibConst.MOBILE.CIVILITY, // iso v7
                eLibConst.MOBILE.TITLE,
                eLibConst.MOBILE.EMAIL };


            if (_tp == eLibConst.MOBILEMAPPINGTYPE.ADDINOUTLOOK)
            {

                //Champ de recherche email principal
                //  mainMobileFieldsList.Add(eLibConst.MOBILE.FREE_3);

            }
            else
            {
                mainMobileFieldsList.Add(eLibConst.MOBILE.PHOTO); //, // iso v7
            }

            mainMobileFieldsList.AddRange(new List<eLibConst.MOBILE>() {


                eLibConst.MOBILE.COMPANY,
                eLibConst.MOBILE.STREET_1,
                eLibConst.MOBILE.STREET_2,
                eLibConst.MOBILE.STREET_3, // iso v7
                eLibConst.MOBILE.POSTALCODE,
                eLibConst.MOBILE.CITY,
                eLibConst.MOBILE.COUNTRY,
                eLibConst.MOBILE.TEL_FIXED,
                eLibConst.MOBILE.TEL_MOBILE,
                eLibConst.MOBILE.TEL_COMPANY,
            });
            if (_tp == eLibConst.MOBILEMAPPINGTYPE.MOBILE)
            {

                mainMobileFieldsList.Add(eLibConst.MOBILE.FREE_1);
                mainMobileFieldsList.Add(eLibConst.MOBILE.FREE_2);
                mainMobileFieldsList.Add(eLibConst.MOBILE.FREE_3);

            }



            List<eLibConst.MOBILE> planningMobileFieldsList = new List<eLibConst.MOBILE>()
            {
                eLibConst.MOBILE.CALENDAR_DATE_BEGIN,
                eLibConst.MOBILE.CALENDAR_DATE_END,
                eLibConst.MOBILE.CALENDAR_ACTION,
                eLibConst.MOBILE.CALENDAR_LOCATION,
                eLibConst.MOBILE.CALENDAR_MEMO,
                eLibConst.MOBILE.CALENDAR_COLOR
            };
            List<eLibConst.MOBILE> eventMobileFieldsList = new List<eLibConst.MOBILE>()
            {
                eLibConst.MOBILE.EVENT_DATE, // iso v7
                eLibConst.MOBILE.EVENT_NAME // iso v7
            };
            List<eLibConst.MOBILE> standardMobileFieldsList = new List<eLibConst.MOBILE>()
            {
                eLibConst.MOBILE.EVENT_INVITATION_CONFIRMATION // iso v7
            };




            #endregion

            #region Construction des listes de contrôles à afficher
            List<Panel> mainFieldsList = new List<Panel>();
            List<Panel> planningFieldsList = new List<Panel>();
            List<Panel> eventFieldsList = new List<Panel>();
            List<Panel> standardFieldsList = new List<Panel>();

            mainFieldsList = GetFieldControlsList("main", mainMobileFieldsList, _fieldListMain);
            planningFieldsList = GetFieldControlsList("planning", planningMobileFieldsList, _fieldListPlanning);
            ;
            #endregion

            #region Regroupement des contrôles en sections et ajout des contrôles sur la page


            ExtensionParametersContainer.Controls.Add(GetFieldsSection("main", eResApp.GetRes(Pref, 7858), eResApp.GetRes(Pref, 830), null, mainFieldsList)); // "Sélection des champs principaux" - TOCHECKRES

            if (this.Extension is eAdminExtensionMobile)
            {
                ExtensionParametersContainer.Controls.Add(GetFieldsSection("planning", eResApp.GetRes(Pref, 7859), eResApp.GetRes(Pref, 462), _tabListPlanning, planningFieldsList)); // "Sélection des champs de l'onglet Planning" - TOCHECKRES
                                                                                                                                                                                      //    ExtensionParametersContainer.Controls.Add(GetFieldsSection("event", eResApp.GetRes(Pref, 7860), eResApp.GetRes(Pref, 461), _tabListMain, eventFieldsList)); // "Sélection des champs de l'onglet Evènements" - TOCHECKRES
                                                                                                                                                                                      //   ExtensionParametersContainer.Controls.Add(GetFieldsSection("standard", eResApp.GetRes(Pref, 7861), eResApp.GetRes(Pref, 463), _tabListStandard, standardFieldsList)); // "Sélection des champs de type Invitation affichés sur l'onglet Evènements" - TOCHECKRES
            }
            #endregion

            #region Ajout du JS permettant de contrôler les champs de type Standard proposés en fonction de l'Event sélectionné
            string jsContents = String.Empty;
            foreach (int tab in _tabListStandardInterEventDescIds.Keys)
                jsContents = String.Concat(jsContents, "nsAdminMobile.interEventDescIds[", tab, "] = ", _tabListStandardInterEventDescIds[tab], ";");
            AddCallBackScript(jsContents);
            #endregion




        }



        /// <summary>
        /// Génération du contenu
        /// </summary>
        /// <returns>true si le contenu a été généré avec succès</returns>
        protected override bool Build()
        {
            if (base.Build())
            {
                bool extensionEnabled = Extension.Infos.IsEnabled;

                return true;
            }
            return false;
        }

        #region méthodes privées
        private IEnumerable<eFieldLiteAdmin> GetTabFields(int tab)
        {
            IEnumerable<eFieldLiteAdmin> list = RetrieveFields.GetEmpty(Pref)
                .AddOnlyThisTabs(new int[] { tab })
                .SetExternalDal(Extension.DAL)
                .ResultFieldsInfo(eFieldLiteAdmin.FactoryTable(Pref), eFieldLiteAdmin.Factory(Pref));

            // Exclus les rubriques avec libelle vide (cibles etendues)
            list = list?.Where(fld => !String.IsNullOrEmpty(fld.Libelle));

            // Tri par libelle
            list = list?.OrderBy(fld => fld.Libelle);

            return list;
        }


        private Panel GetInfosSection(string id, string sectionLabel, string sectionTabLabel, List<Tuple<string, string>> infos)
        {
            Panel sectionPanel = GetModuleSection(String.Concat("section_", id), sectionLabel);
            Panel sectionPanelContainer = (Panel)sectionPanel.Controls[sectionPanel.Controls.Count - 1];


            if (infos != null)
            {
                foreach (var fieldInfos in infos)
                {


                    Panel field = new Panel();
                    field.CssClass = "field fieldinfosection";
                    field.ID = String.Concat(id, "fieldinfosection_", id);
                    HtmlGenericControl fieldLabel = new HtmlGenericControl("label");
                    fieldLabel.InnerText = fieldInfos.Item1;

                    field.Controls.Add(fieldLabel);

                    HtmlInputText fieldValue = new HtmlInputText();
                    fieldValue.Value = fieldInfos.Item2;

                    fieldValue.Attributes.Add("title", eResApp.GetRes(_ePref, 2314)); //Double-cliquez pour copier la valeur dans le presse papier

                    fieldValue.Attributes.Add("ondblclick", "nsAdminField.CopyValueToClipBoard(event)");

                    fieldValue.Attributes.Add("readonly", "1");
                    fieldValue.Size = 60;

                    field.Controls.Add(fieldValue);

                    sectionPanelContainer.Controls.Add(field);
                }
            }
            return sectionPanel;
        }

        List<eLibConst.MOBILE> _lstIsPlanningField = new List<eLibConst.MOBILE>()
            {
                eLibConst.MOBILE.CALENDAR_DATE_BEGIN,
                eLibConst.MOBILE.CALENDAR_DATE_END,
                eLibConst.MOBILE.CALENDAR_ACTION,
                eLibConst.MOBILE.CALENDAR_LOCATION,
                eLibConst.MOBILE.CALENDAR_MEMO,
                eLibConst.MOBILE.CALENDAR_COLOR
            };



        List<eLibConst.MOBILE> _lstIsEvt = new List<eLibConst.MOBILE>()
            {
                eLibConst.MOBILE.EVENT_DATE,
                eLibConst.MOBILE.EVENT_NAME
            };


        List<eLibConst.MOBILE> _lstIsInvit = new List<eLibConst.MOBILE>()
            {
                eLibConst.MOBILE.EVENT_INVITATION_CONFIRMATION
            };
        /// <summary>
        /// Permet de filtrer en fonction de critère additionnel
        /// </summary>
        /// <param name="mobileField"></param>
        /// <param name="fld"></param>
        /// <returns></returns>
        protected virtual bool IsAllowed(eLibConst.MOBILE mobileField, eFieldLiteAdmin fld)
        {
            return true;
        }

        private List<Panel> GetFieldControlsList(string idPrefix, List<eLibConst.MOBILE> mobileFields, List<eFieldLiteAdmin> fieldList)
        {
            string noFieldSelectionValueLabel = String.Concat("<", eResApp.GetRes(Pref, 7862), ">"); // Sélectionner une rubrique
            List<Panel> fieldControlsList = new List<Panel>();

            foreach (eLibConst.MOBILE mobileField in mobileFields)
            {
                // Filtrage de la liste des Field de la table pour ne retenir que ceux dont le type correspond à ceux acceptés par le champ mappé
                List<eFieldLiteAdmin> filteredFieldList = new List<eFieldLiteAdmin>();
                if (fieldList != null)
                {
                    foreach (eFieldLiteAdmin field in fieldList)
                    {
                        if (eMobileMapping.GetAllowedFieldFormats(mobileField, _tp).Contains(field.Format) && IsAllowed(mobileField, field))
                            filteredFieldList.Add(field);
                    }
                }

                // Conversion de la liste de Field en dictionnaire <int, string> affichable en HTML, où int = DescId, et string = Libellé affiché (Table.champ)
                Dictionary<int, string> fieldListDescidLabel = new Dictionary<int, string>();
                foreach (eFieldLiteAdmin field in filteredFieldList)
                {
                    fieldListDescidLabel[field.Descid] = String.Concat(field.TableLibelle, ".", field.Libelle);
                }

                int selectedValue = -1;
                string displayName = eResApp.GetRes(Pref, eMobileMapping.GetMobileFieldResId(mobileField, _tp));
                bool readOnly = false;
                bool canBeCustomized = eMobileMapping.CanBeCustomized(Pref, mobileField);
                eMobileMapping currentMobileMapping = null;
                if (_mobileMapping.TryGetValue(mobileField, out currentMobileMapping))
                {
                    selectedValue = currentMobileMapping.DescId;
                    displayName = eResApp.GetRes(Pref, currentMobileMapping.GetMobileFieldResId());

                    //Du a la facon dont est géré le mapping et son double usage, on doit gérer ici le champ de fusion <PP> sur Libre 3 pour le mapping addi outlook
                    if (mobileField == eLibConst.MOBILE.FREE_3)
                    {

                        var f = fieldList.Find(d => d.Table.DescId == 200 || d.Table.DescId == 400);
                        if (f != null)
                            displayName = displayName.Replace("<PP>", f.TableLibelle);
                    }
                    else if (mobileField == eLibConst.MOBILE.EMAIL)
                    {

                        var f = fieldList.Find(d => d.Table.DescId == 400 || d.Table.DescId == 200);
                        if (f != null)
                            displayName = displayName.Replace("<ADR>", "");
                    }

                    readOnly = currentMobileMapping.ReadOnly;
                    // Assouplissement par rapport à la v7 : un champ marqué comme Customized = 1 en base est affiché comme modifiable et mappable,
                    // même s'il s'agit d'un champ considéré comme "système". Cela permet d'offrir la possibilité de débloquer ou de bloquer le mapping de
                    // certains champs selon les exigences de l'administrateur/chef de projet Eudonet.
                    // De même, si l'admin est un super-administrateur, on affiche tous les champs comme mappables afin de lui permettre de corriger
                    // d'éventuels mappings corrompus, ce qui était impossible en v7 sans passer par SQL.
                    canBeCustomized =
                        (Pref.User.UserLevel >= (int)UserLevel.LEV_USR_SUPERADMIN) ||
                        currentMobileMapping.Customized; //currentMobileMapping.CanBeCustomized(); 
                }

                Panel fieldPanel = GetFieldDropDownList(mobileField, String.Concat(idPrefix, "_", mobileField.ToString()), idPrefix, displayName, selectedValue, String.Empty, noFieldSelectionValueLabel, fieldListDescidLabel);

                if (this.Extension is eAdminExtensionMobile)
                {
                    // Case à cocher Lecture seule
                    bool disabled = !canBeCustomized;
                    eCheckBoxCtrl chkReadOnly = new eCheckBoxCtrl(readOnly, disabled);
                    chkReadOnly.ID = String.Concat("mobileFieldList_", idPrefix, "_", mobileField.ToString(), "_readOnly");
                    chkReadOnly.AddText(eResApp.GetRes(Pref, 882)); // Lecture seule
                    if (!disabled)
                        chkReadOnly.AddClick(String.Concat("nsAdminMobile.changeField(document.getElementById('mobileFieldListDdl_", idPrefix, "_", mobileField.ToString(), "'), true);"));
                    fieldPanel.Controls.Add(chkReadOnly);
                }

                //SHA : vérifie si tous les paramètres obligatoires de l'Addin Outlook sont renseignés
                //if(this.Extension.Module == eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_OUTLOOKADDIN)
                //{
                //    List<DropDownList> listDDL = new List<DropDownList>();
                //    foreach (var field in fieldPanel.Controls)
                //    {
                //        if (field is DropDownList)
                //        {
                //            DropDownList fieldDDL = field as DropDownList;
                //            if ((fieldDDL.ID.Contains("outlookAddinMain") 
                //                || fieldDDL.ID.Contains("main_NAME")
                //                || fieldDDL.ID.Contains("main_FIRSTNAME")
                //                || fieldDDL.ID.Contains("main_EMAIL")
                //                || fieldDDL.ID.Contains("main_FREE_3")) && fieldDDL.SelectedItem.ToString() == noFieldSelectionValueLabel)
                //                listDDL.Add(fieldDDL);
                //        }
                //    }

                //    if (listDDL.Any())
                //        invalidMandatoryParams = true;
                //}

                fieldControlsList.Add(fieldPanel);
            }

            return fieldControlsList;
        }

        private Panel GetFieldsSection(string id, string sectionLabel, string sectionTabLabel, Dictionary<int, string> tabList, List<Panel> fieldControlsList)
        {
            Panel sectionPanel = GetModuleSection(String.Concat("section_", id), sectionLabel);
            Panel sectionPanelContainer = (Panel)sectionPanel.Controls[sectionPanel.Controls.Count - 1];

            // La présélection de la valeur dans la liste des tables se fait en récupérant le TabID du premier champ mappé pour cette section
            // Ex : pour Planning, si le premier champ mappé est Date de début, on prend son DescId et on sélectionne la table lui correspondant dans la liste
            int selectedValue = 0;
            eMobileMapping selectedMobileMapping = null;
            eLibConst.MOBILE tabKey = eLibConst.MOBILE.UNDEFINED;
            bool foundValue = false;





            switch (id)
            {
                case "planning":
                    selectedMobileMapping = _mobileMapping.FirstOrDefault(d => d.Value.DescId > 0 && _lstIsPlanningField.Contains(d.Key)).Value;
                    foundValue = selectedMobileMapping != null && selectedMobileMapping.Key != eLibConst.MOBILE.UNDEFINED;
                    tabKey = selectedMobileMapping?.Key ?? eLibConst.MOBILE.UNDEFINED;
                    break;
                case "event":
                    selectedMobileMapping = _mobileMapping.FirstOrDefault(d => d.Value.DescId > 0 && _lstIsEvt.Contains(d.Key)).Value;
                    foundValue = selectedMobileMapping != null && selectedMobileMapping.Key != eLibConst.MOBILE.UNDEFINED;
                    tabKey = selectedMobileMapping?.Key ?? eLibConst.MOBILE.UNDEFINED;
                    break;
                case "standard":
                    selectedMobileMapping = _mobileMapping.FirstOrDefault(d => d.Value.DescId > 0 && _lstIsInvit.Contains(d.Key)).Value;
                    foundValue = selectedMobileMapping != null && selectedMobileMapping.Key != eLibConst.MOBILE.UNDEFINED;
                    tabKey = selectedMobileMapping?.Key ?? eLibConst.MOBILE.UNDEFINED;
                    break;
                case "outlookAddinMain":
                    tabKey = eLibConst.MOBILE.FREE_1;
                    foundValue = _mobileMapping.TryGetValue(eLibConst.MOBILE.FREE_1, out selectedMobileMapping);
                    break;
                case "outlookAddinMainPlanning":
                    tabKey = eLibConst.MOBILE.FREE_2;
                    foundValue = _mobileMapping.TryGetValue(eLibConst.MOBILE.FREE_2, out selectedMobileMapping);
                    break;
            }
            if (selectedMobileMapping != null && foundValue && tabKey != eLibConst.MOBILE.UNDEFINED)
                selectedValue = selectedMobileMapping.DescId - selectedMobileMapping.DescId % 100;

            if (tabList != null)
            {
                string noTabSelectionValueLabel = String.Concat("<", eResApp.GetRes(Pref, 588), ">");
                if (_tp == eLibConst.MOBILEMAPPINGTYPE.MOBILE)
                    sectionPanelContainer.Controls.Add(GetTabDropDownList(tabKey, id, id, sectionTabLabel, selectedValue, "", noTabSelectionValueLabel, tabList));
                else
                    sectionPanelContainer.Controls.Add(GetTabAsFieldDropDownList(tabKey, id, id, sectionTabLabel, selectedValue, "", noTabSelectionValueLabel, tabList));
            }

            if (fieldControlsList != null)
                foreach (Panel fieldPanel in fieldControlsList)
                {
                    sectionPanelContainer.Controls.Add(fieldPanel);
                }

            return sectionPanel;
        }

        private Panel GetFieldDropDownList(
            eLibConst.MOBILE key, string id, string section, string label, int selectedValue, string allItemsValueLabel, string noSelectionValueLabel, Dictionary<int, string> fieldList
        )
        {
            return GetDropDownList(key, section, id, label, selectedValue, allItemsValueLabel, noSelectionValueLabel, fieldList, "mobileField", "nsAdminMobile.changeField(this, true);");
        }

        private Panel GetTabDropDownList(
            eLibConst.MOBILE key, string section, string id, string label, int selectedValue, string allItemsValueLabel, string noSelectionValueLabel, Dictionary<int, string> tabList
        )
        {
            return GetDropDownList(key, section, id, label, selectedValue, allItemsValueLabel, noSelectionValueLabel, tabList, "mobileTab", "nsAdminMobile.changeTab(this);");
        }



        private Panel GetTabAsFieldDropDownList(
            eLibConst.MOBILE key, string section, string id, string label, int selectedValue, string allItemsValueLabel, string noSelectionValueLabel, Dictionary<int, string> tabList
        )
        {
            return GetDropDownList(key, section, id, label, selectedValue, allItemsValueLabel, noSelectionValueLabel, tabList, "mobileField", "nsAdminMobile.changeField(this, true);");
        }

        private Panel GetDropDownList(
            eLibConst.MOBILE key, string section, string id, string label, int selectedValue, string allItemsValueLabel, string noSelectionValueLabel, Dictionary<int, string> itemList,
            string prefix, string jsOnChange
        )
        {
            Panel field = new Panel();
            field.CssClass = "field";
            field.ID = String.Concat(prefix, "List_", id);
            HtmlGenericControl fieldLabel = new HtmlGenericControl("label");
            fieldLabel.InnerText = label;

            //SHA : correction bug 71 446
            HtmlGenericControl mandatoryParam = new HtmlGenericControl("span");
            if (this.Extension.Module == eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_OUTLOOKADDIN)
            {
                switch (id)
                {
                    case "outlookAddinMain":
                    case "main_NAME":
                    case "main_FIRSTNAME":
                    case "main_EMAIL":
                    case "main_FREE_3":
                        mandatoryParam.Attributes.Add("class", "mandatoryParam");
                        mandatoryParam.InnerText = "*";
                        break;
                }
            }

            DropDownList ddl = new DropDownList();
            ddl.ID = String.Concat(prefix, "ListDdl_", id);
            if (itemList.ContainsKey(selectedValue))
                ddl.SelectedValue = selectedValue.ToString();
            ddl.Attributes.Add("onchange", jsOnChange);

            ddl.DataSource = itemList;
            ddl.DataTextField = "Value";
            ddl.DataValueField = "Key";
            ddl.DataBind();

            string sSep = "--------------------";
            //Séparateur
            ListItem li = new ListItem(sSep, "0");
            ddl.Items.Insert(0, li);
            li.Attributes.Add("disabled", "1");
            li.Attributes.Add("class", "BotSep");

            if (noSelectionValueLabel.Trim().Length > 0)
                ddl.Items.Insert(0, new ListItem(noSelectionValueLabel, "-1"));

            if (allItemsValueLabel.Trim().Length > 0)
                ddl.Items.Insert(0, new ListItem(allItemsValueLabel, "0"));

            if (section == "planning" && _lstIsPlanningField.Contains(key) && prefix == "mobileField")
            {
                var selectedMobileMapping = _mobileMapping.FirstOrDefault(d => d.Value.DescId > 0 && _lstIsPlanningField.Contains(d.Key)).Value;
                int nTab = 0;
                if (selectedMobileMapping != null && selectedMobileMapping.Key != eLibConst.MOBILE.UNDEFINED)
                    nTab = selectedMobileMapping.DescId - selectedMobileMapping.DescId % 100;


                {

                    foreach (ListItem itm in ddl.Items)
                    {
                        int nValue;
                        if (Int32.TryParse(itm.Value, out nValue) && nValue > 0)
                        {
                            int nTabItm = nValue - nValue % 100;
                            if (nTabItm > 100 && nTabItm != nTab)
                            {
                                itm.Attributes.Add("style", "display:none");
                            }


                        }
                    }

                }
            }

            // Attributs concernant le champ dans la table MOBILE
            int fieldValue = eMobileMapping.GetMobileFieldResId(key, _tp);
            bool readOnly = false;
            bool canBeCustomized = eMobileMapping.CanBeCustomized(Pref, key);
            eMobileMapping currentMobileMapping = null;
            if (_mobileMapping.TryGetValue(key, out currentMobileMapping))
            {
                fieldValue = currentMobileMapping.Field;
                readOnly = currentMobileMapping.ReadOnly;
                // Assouplissement par rapport à la v7 : un champ marqué comme Customized = 1 en base est affiché comme modifiable et mappable,
                // même s'il s'agit d'un champ considéré comme "système". Cela permet d'offrir la possibilité de débloquer ou de bloquer le mapping de
                // certains champs selon les exigences de l'administrateur/chef de projet Eudonet.
                // De même, si l'admin est un super-administrateur, on affiche tous les champs comme mappables afin de lui permettre de corriger
                // d'éventuels mappings corrompus, ce qui était impossible en v7 sans passer par SQL.
                canBeCustomized =
                    (Pref.User.UserLevel >= (int)UserLevel.LEV_USR_SUPERADMIN) ||
                    currentMobileMapping.Customized; //currentMobileMapping.CanBeCustomized(); 
            }
            field.Attributes.Add("key", ((int)key).ToString());
            field.Attributes.Add("section", section);
            field.Attributes.Add("userid", "0"); // TODO / TOCHECK ?
            field.Attributes.Add("readonly", readOnly ? "1" : "0"); // La mise à jour de ce champ sera gérée par la case à cocher
            List<string> availableTabIds = new List<string>();
            foreach (int descId in itemList.Keys)
            {
                int tabId = descId - descId % 100;
                if (!availableTabIds.Contains(tabId.ToString()))
                    availableTabIds.Add(tabId.ToString());
            }
            field.Attributes.Add("tab", String.Join(";", availableTabIds.ToArray())); // TODO / TOCHECK ?
            field.Attributes.Add("field", fieldValue.ToString()); // TODO / TOCHECK ?
            field.Attributes.Add("customized", canBeCustomized ? "1" : "0");

            // Si le champ n'est pas personnalisable : contrôle grisé
            if (!canBeCustomized)
            {
                field.Attributes.Add("disabled", "disabled");
                ddl.Attributes.Add("disabled", "disabled");
            }

            field.Controls.Add(fieldLabel);
            //SHA : correction bug 71 446
            if (this.Extension.Module == eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_OUTLOOKADDIN)
            {
                switch (id)
                {
                    case "outlookAddinMain":
                    case "main_NAME":
                    case "main_FIRSTNAME":
                    case "main_EMAIL":
                    case "main_FREE_3":
                        field.Controls.Add(mandatoryParam);
                        break;
                }
            }

            field.Controls.Add(ddl);
            return field;
        }

        #endregion méthodes privées

    }
}