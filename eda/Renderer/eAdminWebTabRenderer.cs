using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{



    /// <summary>
    /// Classe qui génére la nav bar d'admin des onglets/signets web
    /// </summary>
    public class eAdminWebTabNavBarRenderer : eAdminRenderer
    {
        /// <summary>
        /// Tab
        /// </summary>
        protected Int32 _nTab;
        /// <summary>
        /// File Id
        /// </summary>
        protected Int32 _nFileId;
        /// <summary>
        /// Liste des spécifs
        /// </summary>
        protected List<eSpecif> _specifs;
        /// <summary>
        /// Liste des grilles
        /// </summary>
        protected List<eRecord> _lstXrmGrids;
        /// <summary>
        /// Emplacement du webtab : en signet ou par défaut en sous-onglet
        /// </summary>
        protected eXrmWidgetContext.eGridLocation _location;

        /// <summary>
        /// Nav bar d'admin des onglet web
        /// </summary>
        /// <param name="pref">The preference.</param>
        /// <param name="nTab">The n tab.</param>
        /// <param name="nFileId">The n file identifier.</param>
        /// <param name="location">The location.</param>
        private eAdminWebTabNavBarRenderer(ePref pref, int nTab, int nFileId, eXrmWidgetContext.eGridLocation location = eXrmWidgetContext.eGridLocation.Default)
        {

            _rType = RENDERERTYPE.Admin;
            Pref = pref;
            _nTab = nTab;
            _nFileId = nFileId;
            // this._ednType = ednType;
            _location = location;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="eAdminWebTabNavBarRenderer"/> class.
        /// </summary>
        protected eAdminWebTabNavBarRenderer() { }



        /// <summary>
        /// Gets the admin web tab nav bar renderer.
        /// </summary>
        /// <param name="pref">The preference.</param>
        /// <param name="nTab">Onglet auquel la grille est rattachée</param>
        /// <param name="fileId">fiche de l'onglet dans le cas des pages d'accueil</param>
        /// <param name="location">The location.</param>
        /// <returns></returns>
        /// <exception cref="EudoAdminInvalidRightException"></exception>
        public static eAdminWebTabNavBarRenderer GetAdminWebTabNavBarRenderer(ePref pref, int nTab, int fileId, eXrmWidgetContext.eGridLocation location = eXrmWidgetContext.eGridLocation.Default)
        {
            if (pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                throw new EudoAdminInvalidRightException();


            eAdminWebTabNavBarRenderer a = new eAdminWebTabNavBarRenderer(pref, nTab, fileId, location);

            a.Generate();

            return a;
        }



        /// <summary>
        /// Récupère toutes les specifs de type page web
        /// </summary>
        protected override Boolean Init()
        {
            if (!base.Init())
                return false;

            try
            {
                _specifs = eSpecif.GetSpecifList(Pref, new List<eLibConst.SPECIF_TYPE>() { eLibConst.SPECIF_TYPE.TYP_WEBTAB_INTERNAL, eLibConst.SPECIF_TYPE.TYP_WEBTAB_EXTERNAL }, _nTab);
                _lstXrmGrids = eWebTabSubMenuRenderer.GetGridsList(Pref, _nTab, _nFileId);
            }
            catch (Exception ex)
            {
                _sErrorMsg = "eWebTab::Init : " + ex.Message;
                _eException = ex;

                return false;
            }

            return true;
        }


        /// <summary>
        /// Savoir s'il y a des éléments a afficher
        /// </summary>
        /// <returns></returns>
        private bool hasSpecifItems()
        {
            return _specifs?.Count > 0;
        }

        private bool hasGridItems()
        {
            return _lstXrmGrids?.Count > 0;
        }

        private bool hasItems()
        {
            return hasSpecifItems() || hasGridItems();
        }

        /// <summary>
        /// Construit le sous menu
        /// </summary>
        /// <returns></returns>
        protected override Boolean Build()
        {
            _pgContainer = new Panel();
            _pgContainer.ID = "admWebTab"; // ID utilisé par eMainList.DoPlanning()
            _pgContainer.CssClass = "admin-webtabs";




            HtmlGenericControl listWebTabs = new HtmlGenericControl("ul");
            listWebTabs.ID = "ulWebTabs";
            listWebTabs.Attributes.Add("class", "ulAdminTabs");
            _pgContainer.Controls.Add(listWebTabs);

            HtmlGenericControl _pgContent = new HtmlGenericControl("div");
            _pgContent.ID = "listheader";
            _pgContainer.Controls.Add(_pgContent);

            String sDisplayOrder = String.Format("{0}_{1}", (int)TableType.XRMGRID, (int)XrmGridField.DisplayOrder);
            int displayOrder, oldDisplayOrder = 0;

            //Si la table possede des grilles et que la premiere grille possede un displayOrder a 0
            //alors affichage de la grille en premier, puis des autres grilles si displayOrder se suit, sinon "liste des fiches"
            if (hasGridItems() && Int32.TryParse(_lstXrmGrids[0].GetFieldByAlias(sDisplayOrder).DisplayValue, out displayOrder) && displayOrder == 0)
            {
                bool bAddListItem = false;
                foreach (eRecord rec in _lstXrmGrids)
                {
                    if (Int32.TryParse(rec.GetFieldByAlias(sDisplayOrder).DisplayValue, out displayOrder))
                    {
                        if (displayOrder == 0 || displayOrder == oldDisplayOrder + 1)
                        {
                            addGridWebTab(rec, listWebTabs);
                        }
                        else
                        {
                            if (!bAddListItem) //Si on a pas deja ajouter l item liste des fiches on l ajoute une seule fois
                            {
                                AddFilesListTab(listWebTabs);
                                bAddListItem = true;
                            }
                            addGridWebTab(rec, listWebTabs); //On affiche la grille
                        }
                        oldDisplayOrder = displayOrder;
                    }
                }
                if (!bAddListItem) //Si toutes les grilles etait avant l item liste des fiches on l affiche en fin de liste
                    AddFilesListTab(listWebTabs);
            }
            else //Si pas de grilles ou grille.displayOrder > 0, on commence par l affichage de liste des fiches
            {
                AddFilesListTab(listWebTabs);
                foreach (eRecord rec in _lstXrmGrids)
                {
                    addGridWebTab(rec, listWebTabs);
                }
            }

            foreach (eSpecif specif in _specifs)
            {
                addWebTab(specif, listWebTabs);
            }

            if (!hasItems())
            {
                AddDropArea(listWebTabs);
                return false;
            }

            AddDropArea(listWebTabs);

            return true;
        }

        private void addWebTab(eSpecif specif, HtmlGenericControl listWebTabs)
        {
            addWebTab(DragType.WEB_TAB, specif.SpecifId, specif.Label, listWebTabs);
        }

        /// <summary>
        /// rajoute les sous onglets web qui sont paramétrés sous la forme de Grid (nouveau système)
        /// </summary>
        /// <param name="file"></param>
        /// <param name="listWebTabs"></param>
        private void addGridWebTab(eRecord record, HtmlGenericControl listWebTabs)
        {
            String sTitleAlias = String.Format("{0}_{1}", (int)TableType.XRMGRID, (int)XrmGridField.Title);
            addWebTab(DragType.GRID, record.MainFileid, record.GetFieldByAlias(sTitleAlias)?.DisplayValue, listWebTabs);
        }

        private void addWebTab(DragType dragType, int iId, string sLabel, HtmlGenericControl listWebTabs)
        {
            HtmlGenericControl li, options, duplicate, config, delete;


            //   subMenu.Controls.Add(newMenuItem(iterator.Current.SpecifId, iterator.Current.Label));
            li = new HtmlGenericControl("li");
            listWebTabs.Controls.Add(li);


            li.Attributes.Add("edragtype", ((int)dragType).ToString());
            li.Attributes.Add("draggable", "true");
            li.Attributes.Add("fid", iId.ToString());
            HtmlGenericControl span = new HtmlGenericControl();
            span.InnerHtml = sLabel;
            li.Controls.Add(span);


            options = new HtmlGenericControl("ul");
            options.ID = String.Concat("webTabOptions_", iId);
            li.Controls.Add(options);
            options.Attributes.Add("class", "webTabOptions");
            // ul.Attributes.Add("did", fieldDescid.ToString());


            /** possibilité de duplication de la grille. Si on n'est pas en signet grille. G.L */
            if (_location != eXrmWidgetContext.eGridLocation.Bkm) {
                duplicate = new HtmlGenericControl("li");
                duplicate.Attributes.Add("did", _nTab.ToString());
                duplicate.Attributes.Add("gid", iId.ToString());
                duplicate.Attributes.Add("onclick", "nsAdmin.FnDuplicateGrid(this);");
                duplicate.Attributes.Add("class", "icon-clone configOption");
                duplicate.Attributes.Add("title", string.Format(eResApp.GetRes(Pref, 2437), sLabel));
                options.Controls.Add(duplicate); 
            }

            // config
            config = new HtmlGenericControl("li");
            options.Controls.Add(config);
            if (dragType == DragType.WEB_TAB)
                config.Attributes.Add("onclick", String.Concat("nsAdmin.editWebTabProperties(", _nTab, ",", iId, ");"));
            else if (dragType == DragType.GRID)
            {

                config.Attributes.Add("did", _nTab.ToString());
                config.Attributes.Add("gid", iId.ToString());
                config.Attributes.Add("onclick", String.Concat("nsAdminGridSubMenu.editGrid(event);"));

            }

            config.Attributes.Add("class", "icon-cog configOption");
            config.Attributes.Add("title", eResApp.GetRes(Pref, 7817));

            if (_location == eXrmWidgetContext.eGridLocation.Bkm)
                config.Attributes.Add("data-gridlocation", ((int)_location).ToString());


            // La grille est supprimer avec le signet grille
            // TODO Ajouter ce button une fois l'ajout des grille dans le signet est autorisé
            if (_location == eXrmWidgetContext.eGridLocation.Bkm)
                return;

            // suppression
            delete = new HtmlGenericControl("li");
            options.Controls.Add(delete);

            if (dragType == DragType.WEB_TAB)
                delete.Attributes.Add("onclick", String.Concat("nsAdmin.deleteWebTab(", _nTab, ",", iId, ");"));
            else if (dragType == DragType.GRID)
            {
                delete.Attributes.Add("did", _nTab.ToString());
                delete.Attributes.Add("gid", iId.ToString());
                delete.Attributes.Add("onclick", String.Concat("nsAdminGridSubMenu.deleteGrid(event);"));
            }

            delete.Attributes.Add("class", "deleteOption icon-delete");
        }


        /// <summary>
        /// rajoute l'entrée "Liste des fiches"
        /// </summary>
        /// <param name="listWebTabs"></param>
        protected virtual void AddFilesListTab(HtmlGenericControl listWebTabs)
        {
            if (_nTab != 0 && _nTab != (int)TableType.XRMHOMEPAGE)
            {

                HtmlGenericControl li = new HtmlGenericControl("li");
                listWebTabs.Controls.Add(li);


                HtmlGenericControl span = new HtmlGenericControl();
                span.InnerHtml = eResApp.GetRes(Pref, 1485); // liste des fiches
                li.Attributes.Add("fid", "0"); //liste
                li.Attributes.Add("edragtype", DragType.WEB_TAB_LIST.GetHashCode().ToString());
                li.Attributes.Add("draggable", "true");
                li.Controls.Add(span);
            }
        }

        private void AddDropArea(HtmlGenericControl parentNode)
        {
            //li invisible qui apparait au drag n drop
            HtmlGenericControl li = new HtmlGenericControl("li");
            parentNode.Controls.Add(li);

            li.Attributes.Add("edragtype", DragType.WEB_TAB.GetHashCode().ToString());
            li.Attributes.Add("draggable", "false");
            li.Attributes.Add("fid", "0");
            li.Attributes.Add("class", "lidroparea");
            li.Attributes.Add("id", "admWebTabDropArea");

            HtmlGenericControl span = new HtmlGenericControl();
            span.InnerHtml = "&nbsp;";
            li.Controls.Add(span);
        }

        /// <summary>
        /// End
        /// </summary>
        /// <param name="subMenu"></param>
        /// <returns></returns>
        public Boolean End(HtmlGenericControl subMenu)
        {
            return true;
        }

    }

    /// <summary>
    /// Classe qui génére la nav bar d'admin des onglets web sous un fichier de type onglet web
    /// </summary>
    public class eAdminWebTabNavBarForWebTabFileRenderer : eAdminWebTabNavBarRenderer
    {

        /// <summary>
        /// Nav bar d'admin des onglet web sous un fichier de type onglet web
        /// </summary>
        /// <param name="pref">The preference.</param>
        /// <param name="nTab">The n tab.</param>
        /// <param name="fileId">The file identifier.</param>
        /// <param name="location">The location.</param>
        private eAdminWebTabNavBarForWebTabFileRenderer(ePref pref, int nTab, int fileId, eXrmWidgetContext.eGridLocation location)
        {

            _rType = RENDERERTYPE.Admin;
            this.Pref = pref;
            this._nTab = nTab;
            this._nFileId = fileId;

            this._location = location;

            // this._ednType = ednType;
        }

        /// <summary>
        /// Gets the admin web tab nav bar for web tab renderer.
        /// </summary>
        /// <param name="pref">The preference.</param>
        /// <param name="nTab">The n tab.</param>
        /// <param name="fileId">The file identifier.</param>
        /// <param name="location">The location.</param>
        /// <returns></returns>
        /// <exception cref="EudoAdminInvalidRightException"></exception>
        public static eAdminWebTabNavBarRenderer GetAdminWebTabNavBarForWebTabRenderer(ePref pref, int nTab, int fileId, eXrmWidgetContext.eGridLocation location = eXrmWidgetContext.eGridLocation.Default)
        {
            if (pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                throw new EudoAdminInvalidRightException();


            eAdminWebTabNavBarForWebTabFileRenderer a = new eAdminWebTabNavBarForWebTabFileRenderer(pref, nTab, fileId, location);

            a.Generate();

            return a;
        }

        /// <summary>
        /// rajoute l'entrée "Liste des fiches"
        /// </summary>
        /// <param name="listWebTabs"></param>
        protected override void AddFilesListTab(HtmlGenericControl listWebTabs)
        {
            return;
        }
    }

    /// <summary>
    /// Classe qui génére la nav bar d'admin des onglets web sous un fichier de type onglet web
    /// </summary>
    public class eAdminWebTabNavBarForXrmHomePageRenderer : eAdminWebTabNavBarRenderer
    {


        /// <summary>
        /// Nav bar d'admin des onglet web sous un fichier de type pages d'accueil
        /// </summary>
        /// <param name="_pref"></param>
        /// <param name="_nTab"></param>
        private eAdminWebTabNavBarForXrmHomePageRenderer(ePref pref, int nTab, int nFileId)
        {

            _rType = RENDERERTYPE.Admin;
            this.Pref = pref;
            this._nTab = nTab;
            this._nFileId = nFileId;

            // this._ednType = ednType;
        }

        public static eAdminWebTabNavBarForXrmHomePageRenderer GetAdminWebTabNavBar(ePref pref, int nTab, int nFileId)
        {
            if (pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                throw new EudoAdminInvalidRightException();


            eAdminWebTabNavBarForXrmHomePageRenderer a = new eAdminWebTabNavBarForXrmHomePageRenderer(pref, nTab, nFileId);

            a.Generate();

            return a;
        }


        protected override bool End()
        {
            if (!base.End())
                return false;


            // Info sur la table parente
            _pgContainer.Attributes.Add("pTab", _nTab.ToString());
            _pgContainer.Attributes.Add("pfId", _nFileId.ToString());
            _pgContainer.Attributes.Add("ednType", ((int)EdnType.FILE_GRID).ToString());

            return true;
        }


        /// <summary>
        /// rajoute l'entrée "Liste des fiches"
        /// </summary>
        /// <param name="listWebTabs"></param>
        protected override void AddFilesListTab(HtmlGenericControl listWebTabs)
        {
            return;
        }
    }


    /// <summary>
    /// classe qui gère le menu droite des onglets web
    /// </summary>
    public class eAdminWebTabParameterRenderer : eAdminSpecifPropertiesRenderer
    {

        #region constructeur
        protected eAdminWebTabParameterRenderer(ePref pref, Int32 nTab, Int32 nSpecifId) :
            base(pref, eAdminUpdateProperty.CATEGORY.SPECIFS, nTab, nSpecifId, new List<eLibConst.SPECIF_TYPE>() { eLibConst.SPECIF_TYPE.TYP_WEBTAB_INTERNAL, eLibConst.SPECIF_TYPE.TYP_WEBTAB_EXTERNAL }, "partWebTab")
        {
            _blockTitle = eResApp.GetRes(Pref, 6919);
        }

        #endregion


        #region création du renderer

        /// <summary>
        /// Retourne un eAdminWebTabParameterRenderer
        /// </summary>
        /// <param name="pref">préférence user</param>
        /// <param name="nTab">Table de la spécif</param>
        /// <param name="nSpecifId">Id de la spécif</param>
        /// <returns></returns>
        public static eAdminWebTabParameterRenderer GetAdminWebTabParameterRenderer(ePref pref, Int32 nTab, Int32 nSpecifId)
        {
            eAdminWebTabParameterRenderer a = new eAdminWebTabParameterRenderer(pref, nTab, nSpecifId);
            return a;
        }

        protected override void SetLabels()
        {
            base.SetLabels();

            _labelName = eResApp.GetRes(Pref, 7596);
            _labelURL = eResApp.GetRes(Pref, 7598);
            _labelAdminURL = eResApp.GetRes(Pref, 7601);

            if (_specif != null)
            {
                if (_specif.Type == eLibConst.SPECIF_TYPE.TYP_WEBTAB_EXTERNAL)
                {
                    _labelURL = String.Concat(_labelURL, " (", eResApp.GetRes(Pref, 7828), ")");
                }
                else if (_specif.Type == eLibConst.SPECIF_TYPE.TYP_WEBTAB_INTERNAL)
                {
                    _labelURL = String.Concat(_labelURL, " (", eResApp.GetRes(Pref, 7829), ")");
                }
            }

        }

        protected override void SetAvailableOpenModes()
        {
            _openModes = new List<eLibConst.SPECIF_OPENMODE> { eLibConst.SPECIF_OPENMODE.IFRAME };
        }

        protected override eAdminField CreateLabelField()
        {
            eAdminField field = base.CreateLabelField();
            ((WebControl)field.FieldControl).ID = "admWebTabLabel";
            return field;
        }

        protected override eAdminField CreateUrlField()
        {
            eAdminField field = base.CreateUrlField();
            ((WebControl)field.FieldControl).ID = "admWebTabURL";
            return field;
        }

        protected override eAdminField CreateUrlParamField()
        {
            eAdminField field = base.CreateUrlParamField();
            if (_specif != null)
            {
                field.SetControlAttribute("opt", "1");
            }
            return field;
        }

        protected override eAdminField CreateAdminUrlField()
        {
            eAdminField field = base.CreateAdminUrlField();

            WebControl control = ((WebControl)field.FieldControl);
            control.ID = "admWebTabURL";
            if (_specif != null)
            {
                if (_specif.Type == eLibConst.SPECIF_TYPE.TYP_WEBTAB_EXTERNAL)
                {
                    // SPH : la regexp de vérif d'url est mauvais voir eTool.js Validator.isUrl
                    // une regexp efficace pour les url 'complète' est assez compliqué (pour être "complète", on va avoir une regex de + de 5k caractères.)
                    //control.Attributes.Add("format", FieldFormat.TYP_WEB.GetHashCode().ToString());
                    control.Attributes.Add("format", ((int)FieldFormat.TYP_CHAR).ToString());

                }
                else if (_specif.Type == eLibConst.SPECIF_TYPE.TYP_WEBTAB_INTERNAL)
                {
                    control.Attributes.Add("format", ((int)FieldFormat.TYP_CHAR).ToString());
                    control.Attributes.Add("especif", "1");
                }

                control.Attributes.Add("opt", "1");
            }
            else
                control.Attributes.Add("format", FieldFormat.TYP_WEB.GetHashCode().ToString());

            return field;
        }

        //protected virtual Control GetIsSpecifParam(Panel panelContent)
        //{
        //    return null;
        //}

        /// <summary>
        /// génère l'option du mode d'ouverture de la spécif
        /// (dans ce cas le mode d'ouverture est toujours IFRAME)
        /// </summary>
        /// <returns></returns>
        protected override eAdminField CreateOpenModeField()
        {
            return null;
        }

        #endregion



    }


    /// <summary>
    /// Entrée de l'entré menu pour le drag n drop des web tab.
    /// Utilisé dans la construction du tab menu droite "contenu de l'onglet" cf eAdminTabContentRenderer
    /// </summary>
    public class eAdminWebTabMenuRenderer : eAdminBlockRenderer
    {

        private eAdminWebTabMenuRenderer(ePref pref, String title)
            : base(pref, null, title, "")
        {

        }




        /// <summary>
        /// Encapsulation du constructeur pour le renderer de l'entrée de menu d'ajout d'onglet web
        /// </summary>
        /// <param name="pref"></param>
        /// <returns></returns>
        public static eAdminBlockRenderer CreateWebTabMenuRenderer(ePref pref)
        {

            eAdminBlockRenderer features = new eAdminWebTabMenuRenderer(pref, eResApp.GetRes(pref, 6914));
            return features;
        }

        /// <summary>
        /// Build
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {
            if (base.Build())
            {
                _panelContent.ID = "WebTabPart";


                HtmlGenericControl ul = new HtmlGenericControl("ul");
                HtmlGenericControl li;
                HtmlGenericControl icon;
                HtmlGenericControl span;


                #region KHA on désactive la création d'onglet web pour favoriser la création de grille
                ////spécifs
                //li = new HtmlGenericControl("li");
                //ul.Attributes.Add("click", "");
                //ul.Attributes.Add("eactive", "0");

                ////drag&droppage 
                //li.Attributes.Add("draggable", "true");
                //li.Attributes.Add("edragtype", DragType.WEB_TAB_MENU.GetHashCode().ToString());
                //li.Attributes.Add("esubtype", eLibConst.SPECIF_TYPE.TYP_WEBTAB_INTERNAL.GetHashCode().ToString());
                //li.Attributes.Add("title", eResApp.GetRes(Pref, 7816));
                //li.ID = "admin_webtab";

                //icon = new HtmlGenericControl("span");
                //icon.Attributes.Add("class", "icon-cloud-upload  fieldIcon");
                //li.Controls.Add(icon);
                //span = new HtmlGenericControl("span");
                //span.InnerText = eResApp.GetRes(Pref, 6945); //onglets web
                //li.Controls.Add(span);
                //ul.Controls.Add(li);

                //// externe
                //li = new HtmlGenericControl("li");
                //ul.Attributes.Add("click", "");
                //ul.Attributes.Add("eactive", "0");

                ////drag&droppage 
                //li.Attributes.Add("draggable", "true");
                //li.Attributes.Add("edragtype", DragType.WEB_TAB_MENU.GetHashCode().ToString());
                //li.Attributes.Add("esubtype", eLibConst.SPECIF_TYPE.TYP_WEBTAB_EXTERNAL.GetHashCode().ToString());
                //li.Attributes.Add("title", eResApp.GetRes(Pref, 7816));

                //li.ID = "admin_webtabext";

                //icon = new HtmlGenericControl("span");
                //icon.Attributes.Add("class", "icon-cloud-upload  fieldIcon");
                //li.Controls.Add(icon);
                //span = new HtmlGenericControl("span");
                //span.InnerText = eResApp.GetRes(Pref, 7969); //onglet web
                //li.Controls.Add(span);
                //ul.Controls.Add(li);
                #endregion

                // Grid
                li = new HtmlGenericControl("li");
                ul.Attributes.Add("click", "");
                ul.Attributes.Add("eactive", "0");

                //drag&droppage 
                li.Attributes.Add("draggable", "true");
                li.Attributes.Add("edragtype", ((int)DragType.GRID_MENU).ToString());
                //li.Attributes.Add("esubtype", eLibConst.SPECIF_TYPE.TYP_WEBTAB_EXTERNAL.GetHashCode().ToString());
                li.Attributes.Add("title", eResApp.GetRes(Pref, 7816));

                li.ID = "admin_gridtab";

                icon = new HtmlGenericControl("span");
                icon.Attributes.Add("class", "icon-cloud-upload  fieldIcon");
                li.Controls.Add(icon);
                span = new HtmlGenericControl("span");
                span.InnerText = eResApp.GetRes(Pref, 7977); //Grille
                li.Controls.Add(span);


                ul.Controls.Add(li);


                _panelContent.Controls.Add(ul);

                return true;
            }
            else
            {
                return false;
            }

        }



    }




}
